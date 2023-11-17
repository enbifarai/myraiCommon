using myRai.DataAccess;
using myRaiCommonModel;
using myRaiData;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Globalization;
using myRaiHelper;

using myRai.Business;
using myRai.Models;
using System.Reflection;

namespace myRaiCommonManager
{
    public class EccezioniManager
    {
        public static string ControllaFerie(string matricola, DateTime Dinizio, DateTime Dfine, string sedegapp, string reparto, bool gestitoSirio, string nominativo)
        {
            if ((Dfine - Dinizio).TotalDays >= 7)
            {
                int dayWeek = (int)Dinizio.DayOfWeek;
                if (dayWeek == 0) dayWeek = 7;

                if (dayWeek == 1)
                {
                    return "Non applicabile (giorno di inizio Lunedi)";
                }
                List<DateTime> LD = GetNonLavoratoRiposoInPeriodo(matricola, Dinizio, Dfine);
                if (LD.Count == 0)
                {
                    return "Non applicabile (Non lavorato/riposo non trovati)";
                }
                if (dayWeek == 2)
                {
                    string esito = SwapCodiceOrario(matricola, Dinizio.AddDays(-1), LD.First(), sedegapp, reparto, gestitoSirio, nominativo);
                    return esito;
                }
                else if (dayWeek > 2)
                {
                    DateTime DInizioSettimana = Dinizio.AddDays(-1 * (dayWeek - 1));
                    string esito = SwapCodiceOrario(matricola, DInizioSettimana, LD[0], sedegapp, reparto, gestitoSirio, nominativo);
                    if (esito != null)
                        return esito;

                    if (LD.Count > 1)
                    {
                        string esito2 = SwapCodiceOrario(matricola, DInizioSettimana.AddDays(1), LD[1], sedegapp, reparto, gestitoSirio, nominativo);
                        if (esito2 != null)
                            return esito2;
                    }


                    return null;
                }
                else
                    return "Non applicabile";
            }
            else
                return null;
        }

        public static string SwapCodiceOrario(string matricola, DateTime D1, DateTime D2, string sedegapp, string reparto, bool gestitoSirio, string nominativo)
        {
            var day1 = AnalisiEccezioni.GetGiornata(CommonManager.GetCurrentUserMatricola(), D1.ToString("ddMMyyyy"), matricola);
            var day2 = AnalisiEccezioni.GetGiornata(CommonManager.GetCurrentUserMatricola(), D2.ToString("ddMMyyyy"), matricola);

            if (day1.giornata.orarioReale == day2.giornata.orarioReale)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = "Orari uguali per matricola " + matricola + " " + D1.ToString("dd/MM/yyyy") + " " + D2.ToString("dd/MM/yyyy"),
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "SwapCodiceOrario"
                });
                return "Stesso codice orario nelle due date";
            }
            if (String.IsNullOrWhiteSpace(day1.giornata.orarioReale) || String.IsNullOrWhiteSpace(day2.giornata.orarioReale))
            {
                string err = "Orario non definito per matricola " + matricola + " " + D1.ToString("dd/MM/yyyy") + " " + D2.ToString("dd/MM/yyyy");
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = err,
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "SwapCodiceOrario"
                });
                return "Orario non definito per una o piu date, impossibile predisporre cambio turno";
            }

            var db = new digiGappEntities();
            myRaiData.MyRai_Richieste R1 = new MyRai_Richieste()
            {
                codice_sede_gapp = sedegapp,
                data_richiesta = DateTime.Now,
                id_stato = 10,
                da_pianoferie = false,
                da_proposta_auto = false,
                matricola_richiesta = matricola,
                id_tipo_richiesta = 1,
                periodo_dal = D1,
                periodo_al = D1,
                motivo_richiesta = "Nuovo turno : " + GetTurnoDescrizione(day2.giornata.orarioReale),
                reparto = reparto,
                gestito_sirio = gestitoSirio,
                nominativo = nominativo
            };
            db.MyRai_Richieste.Add(R1);

            myRaiData.MyRai_Eccezioni_Richieste ER1 = new MyRai_Eccezioni_Richieste();
            ER1.MyRai_Richieste = R1;
            ER1.azione = "I";
            ER1.cod_eccezione = "CTD";
            ER1.data_eccezione = D1;
            ER1.quantita = 1;
            ER1.motivo_richiesta = "Nuovo turno : " + GetTurnoDescrizione(day2.giornata.orarioReale);
            ER1.id_stato = 10;
            ER1.codice_sede_gapp = sedegapp;
            ER1.data_creazione = DateTime.Now;
            ER1.tipo_eccezione = "C";
            ER1.ValoriParamExtraJSON = "[{\"key\":\"TURNO\",\"value\":\"##\"}]".Replace("##", day2.giornata.orarioReale);
            ER1.turno = day1.giornata.orarioReale;
            db.MyRai_Eccezioni_Richieste.Add(ER1);

            myRaiData.MyRai_Richieste R2 = new MyRai_Richieste()
            {
                codice_sede_gapp = sedegapp,
                data_richiesta = DateTime.Now,
                id_stato = 10,
                da_pianoferie = false,
                da_proposta_auto = false,
                matricola_richiesta = matricola,
                id_tipo_richiesta = 1,
                periodo_dal = D2,
                periodo_al = D2,
                motivo_richiesta = "Nuovo turno : " + GetTurnoDescrizione(day1.giornata.orarioReale),
                reparto = reparto,
                gestito_sirio = gestitoSirio,
                nominativo = nominativo
            };
            myRaiData.MyRai_Eccezioni_Richieste ER2 = new MyRai_Eccezioni_Richieste();
            ER2.MyRai_Richieste = R2;
            ER2.azione = "I";
            ER2.cod_eccezione = "CTD";
            ER2.data_eccezione = D2;
            ER2.quantita = 1;
            ER2.motivo_richiesta = "Nuovo turno : " + GetTurnoDescrizione(day1.giornata.orarioReale);
            ER2.id_stato = 10;
            ER2.codice_sede_gapp = sedegapp;
            ER2.data_creazione = DateTime.Now;
            ER2.tipo_eccezione = "C";
            ER2.ValoriParamExtraJSON = "[{\"key\":\"TURNO\",\"value\":\"##\"}]".Replace("##", day1.giornata.orarioReale);
            ER2.turno = day2.giornata.orarioReale;
            db.MyRai_Eccezioni_Richieste.Add(ER2);

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "SwapCodiceOrario"
                });
                return ex.Message;
            }
            return null;
        }

        public static List<DateTime> GetNonLavoratoRiposoInPeriodo(string matricola, DateTime D1, DateTime D2)
        {
            MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client client = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
            client.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            DateTime Dinizio = new DateTime(D1.Year, D1.Month, 1);
            DateTime Dfine = new DateTime(D1.Year, D1.Month, DateTime.DaysInMonth(D1.Year, D1.Month));
            var Response = client.GetSchedaPresenzeMese(matricola, Dinizio, Dfine);
            List<MyRaiServiceInterface.MyRaiServiceReference1.InfoPresenza> Giorni = Response.Giorni.ToList();

            if (D1.Month != D2.Month)
            {
                Dinizio = new DateTime(D2.Year, D2.Month, 1);
                Dfine = new DateTime(D2.Year, D2.Month, DateTime.DaysInMonth(D2.Year, D2.Month));
                var Response2 = client.GetSchedaPresenzeMese(matricola, Dinizio, Dfine);
                Giorni.AddRange(Response2.Giorni.ToList());
            }
            List<DateTime> LD = new List<DateTime>();
            DateTime Dcurrent = D1;
            while (Dcurrent <= D2)
            {
                var d = Giorni.Where(x => x.data == Dcurrent).FirstOrDefault();
                if (d != null && (d.CodiceOrario == "95" || d.CodiceOrario == "96"))
                {
                    LD.Add(Dcurrent);

                }
                Dcurrent = Dcurrent.AddDays(1);
            }
            return LD;
        }
        public static List<string> GetAssorbibili ( string TextPart , int NumericPart )
        {
            var db = new myRaiData.digiGappEntities( );
            string ecc = TextPart + NumericPart.ToString( );

            var list = db.MyRai_Eccezioni_Ammesse.Where( x => x.cod_eccezione.StartsWith( TextPart ) && x.cod_eccezione != ecc ).ToList( );
            var listAss = new List<string>( );

            if ( list.Any( ) )
            {
                foreach ( var e in list )
                {
                    EccezioneSplit esplit = HaParteAlfaParteNum( e.cod_eccezione );
                    if ( esplit != null && esplit.NumericPart < NumericPart )
                    {
                        listAss.Add( e.cod_eccezione );
                    }
                }
            }
            return listAss;
        }
        public static EccezioneSplit HaParteAlfaParteNum ( string CodiceEccezione )
        {
            Regex R = new Regex( "(?<alfa>[A-Z]{1,3})(?<num>[0-9]{1,2})" );
            MatchCollection mc = R.Matches( CodiceEccezione );

            if ( mc.Count > 0 )
            {
                return new EccezioneSplit
                {
                    IsTextAndNumeric = true ,
                    TextPart = mc[0].Groups["alfa"].Value ,
                    NumericPart = Convert.ToInt32( mc[0].Groups["num"].Value )
                };
            }
            else
                return null;
        }
        public static bool HaGiorniMultipli ( int IdRichiesta )
        {
            var db = new digiGappEntities( );
            MyRai_Richieste rich = db.MyRai_Richieste.Where( x => x.id_richiesta == IdRichiesta ).FirstOrDefault( );
            return rich.periodo_al > rich.periodo_dal;
        }


        public static List<MyRai_Richieste> RendiSingoliGiorni ( int IdRichiesta )
        {
            var db = new digiGappEntities( );
            MyRai_Richieste r = db.MyRai_Richieste.Where( x => x.id_richiesta == IdRichiesta ).FirstOrDefault( );
            if ( r == null )
                return null;

            try
            {
                List<MyRai_Richieste> ListaNewRich = new List<MyRai_Richieste>( );
                List<int> LID = r.MyRai_Eccezioni_Richieste.Where( x => x.azione == "I" && x.numero_documento != 0 ).OrderBy( x => x.data_eccezione ).Select( x => x.id_eccezioni_richieste ).ToList( );

                foreach ( int id in LID )
                {
                    MyRai_Eccezioni_Richieste er = db.MyRai_Eccezioni_Richieste.Where( x => x.id_eccezioni_richieste == id ).FirstOrDefault( );

                    MyRai_Richieste rich = db.MyRai_Richieste.Where( x => x.id_richiesta == IdRichiesta ).FirstOrDefault( );
                    MyRai_Richieste Rnew = CommonManager.CopyFromRichieste( rich );
                    Rnew.periodo_dal = er.data_eccezione;
                    Rnew.periodo_al = er.data_eccezione;
                    Rnew.id_stato = er.id_stato;
                    Rnew.TipoQuadratura = ( Utente.GetQuadratura( ) == Quadratura.Giornaliera ? 0 : 1 );
                    db.MyRai_Richieste.Add( Rnew );
                    er.MyRai_Richieste = Rnew;
                    var Storno = rich.MyRai_Eccezioni_Richieste.Where( x => x.azione == "C" && x.numero_documento_riferimento == er.numero_documento ).FirstOrDefault( );
                    if ( Storno != null )
                        Storno.MyRai_Richieste = Rnew;
                    ListaNewRich.Add( Rnew );
                }
                r.id_stato = 60;
                db.SaveChanges( );// only 1

                Logger.LogAzione( new MyRai_LogAzioni( )
                {
                    applicativo = "Portale" ,
                    data = DateTime.Now ,
                    matricola = CommonManager.GetCurrentUserMatricola( ) ,
                    operazione = "FrammentazioneRichiesta" ,
                    provenienza = "RendiSingoliGiorni" ,
                    descrizione_operazione = "Id Richiesta:" + IdRichiesta + " ==>" + string.Join( "," , ListaNewRich.Select( x => x.id_richiesta ).ToArray( ) )
                } );
                return ListaNewRich;
            }
            catch ( Exception ex )
            {
                Logger.LogErrori( new MyRai_LogErrori( )
                {
                    applicativo = "Portale" ,
                    data = DateTime.Now ,
                    error_message = ex.ToString( ) ,
                    matricola = CommonManager.GetCurrentUserMatricola( ) ,
                    provenienza = "RendiSingoliGiorni"
                } );
                return null;
            }
        }

        public static List<string> GetFromAmmesse ( List<MyRai_Eccezioni_Ammesse> L )
        {
            return L.Select( x => x.cod_eccezione ).ToList( );
        }

        public static int GetMinutiCarenzaPerSede ( string sede , DateTime DataRichiesta )
        {
            var db = new digiGappEntities( );
            var se = db.L2D_SEDE_GAPP.Where( x => x.cod_sede_gapp == sede &&
                 x.data_inizio_validita <= DataRichiesta && x.data_fine_validita >= DataRichiesta )
                .FirstOrDefault( );

            if ( se == null || se.minimo_car == null )
                return 0;
            else
            {
                int car = 0;
                int.TryParse( se.minimo_car , out car );
                return car;
            }
        }

        public static Ferie ConvertiWcfToAsmxFerie ( MyRaiServiceInterface.MyRaiServiceReference1.GetFerieResponse wcfFerie )
        {
            return EccezioniHelper.ConvertiWcfToAsmxFerie( wcfFerie );
        }

        public static int GetMinutiUscita ( string codOrario )
        {
            var db = new myRaiData.digiGappEntities( );
            string minUscita = db.L2D_ORARIO.Where( x => x.cod_orario == codOrario ).Select( x => x.uscita_iniziale ).FirstOrDefault( );
            int d = 0;
            int.TryParse( minUscita , out d );
            return d;
        }

        public static int GetMinutiEntrata ( string codOrario )
        {
            var db = new myRaiData.digiGappEntities( );
            string minEntrata = db.L2D_ORARIO.Where( x => x.cod_orario == codOrario ).Select( x => x.entrata_iniziale ).FirstOrDefault( );
            int d = 0;
            int.TryParse( minEntrata , out d );
            return d;
        }

        public static Boolean ConsentitaDaProntuario ( Giornata g )
        {
            return EccezioniHelper.ConsentitaDaProntuario( g );
        }

        public static string TestCodice ( string currentMatricola , string matricola , string data , string codeccezione , string codiceCsharp )
        {
            try
            {
                var db = new digiGappEntities( );
                MyRai_Eccezioni_Ammesse ammessa = db.MyRai_Eccezioni_Ammesse.Where( x => x.cod_eccezione == codeccezione ).FirstOrDefault( );
                if ( ammessa == null )
                    return "Eccezione non trovata";

                //WSDigigapp w = new WSDigigapp()
                //{
                //	Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                //		CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1])
                //};
                //dayResponse resp = w.getEccezioni(matricola, data, "BU", 75);

                WSDigigappDataController service = new WSDigigappDataController( );

                dayResponse resp = service.GetEccezioni( currentMatricola , matricola , data , "BU" , 75 );




                List<MyRai_Eccezioni_Ammesse> ammesse = new List<MyRai_Eccezioni_Ammesse>( );
                ammesse.Add( ammessa );
                EsitoCompilatore es = QualiDaRimuovere( resp , ammesse , matricola , codiceCsharp );
                if ( es.Errore != null )
                    return es.Errore;
                else
                {
                    if ( es.EccezioniDaRimuovere == null || es.EccezioniDaRimuovere.Count == 0 )
                        return "Eccezione " + codeccezione + " NON RIMOSSA, può essere richiesta";
                    else
                        return "Eccezione " + codeccezione + " RIMOSSA, non può essere richiesta";
                }
            }
            catch ( Exception ex )
            {
                return ex.Message + ex.InnerException;
            }
        }

        public static L2D_SEDE_GAPP GetSedeGappFromL2D ( string sede , DateTime data )
        {
            var db = new digiGappEntities( );

            var sedegapp = db.L2D_SEDE_GAPP.Where( x => x.cod_sede_gapp.Trim( ) == sede
                  && x.data_inizio_validita <= data
                  && x.data_fine_validita >= data ).FirstOrDefault( );

            return sedegapp;
        }

        public static EsitoCompilatore QualiDaRimuovere ( dayResponse resp , List<MyRai_Eccezioni_Ammesse> ammesse , string matricola , string CodiceEccezioneProvenienteDaTest = null )
        {
            var extraAssemblies = GetExtraAssemblies( );
            return EccezioniHelper.QualiDaRimuovere( resp , ammesse , matricola , CodiceEccezioneProvenienteDaTest , extraAssemblies: extraAssemblies );
        }

        private static IEnumerable<string> GetExtraAssemblies ( )
        {
            List<string> extraAssemblies = new List<string>( );
            extraAssemblies.AddRange( Assembly.GetCallingAssembly( ).GetReferencedAssemblies( ).Select( x => x.Name ) );
            extraAssemblies.Add( Assembly.GetCallingAssembly( ).GetName( ).Name );

            return extraAssemblies;
        }

        public static List<L2D_ORARIO> GetOrario ( )
        {
            var db = new myRaiData.digiGappEntities( );
            string sede = UtenteHelper.SedeGapp( DateTime.Now );
            string sedecont = UtenteHelper.EsponiAnagrafica( ).SedeContabile;
            if ( !String.IsNullOrWhiteSpace( sedecont ) )
            {
                sedecont = sedecont.Trim( );
                return db.L2D_ORARIO.Where( x => x.SediContabiliCTD != null && x.SediContabiliCTD.Contains( sedecont ) ).ToList( );
            }
            else
                return new List<L2D_ORARIO>( );
        }


        public static List<string> RimuoviEccezioniPerLimiteGiornaliero ( dayResponse giornata )
        {
            return EccezioniHelper.RimuoviEccezioniPerLimiteGiornaliero( giornata );
        }

        public static Eccezione[] VerificaProposteConMacinatore ( Eccezione[] EccezioniProposte , dayResponse resp )
        {
            var db = new digiGappEntities( );
            try
            {
                List<string> Lcod = EccezioniProposte.Select( x => x.cod ).ToList( );
                List<MyRai_Eccezioni_Ammesse> Lammesse = db.MyRai_Eccezioni_Ammesse.Where( x => Lcod.Contains( x.cod_eccezione ) ).ToList( );
                if ( Lammesse != null && Lammesse.Count > 0 )
                {
                    EsitoCompilatore esito = EccezioniManager.QualiDaRimuovere( resp , Lammesse , CommonManager.GetCurrentUserMatricola( ) );
                    if ( esito != null && esito.EccezioniDaRimuovere != null && esito.EccezioniDaRimuovere.Count > 0 )
                    {
                        List<Eccezione> LE = EccezioniProposte.ToList( );
                        LE.RemoveAll( ecc => esito.EccezioniDaRimuovere.Contains( ecc.cod ) );
                        EccezioniProposte = LE.ToArray( );
                    }
                }
            }
            catch ( Exception ex )
            {
                Logger.LogErrori( new MyRai_LogErrori( )
                {
                    applicativo = "PORTALE" ,
                    data = DateTime.Now ,
                    matricola = CommonManager.GetCurrentUserMatricola( ) ,
                    provenienza = "VerificaProposteConMocinatore" ,
                    error_message = ex.ToString( )
                } );
            }
            return EccezioniProposte;
        }

        public static List<MyRai_Eccezioni_Ammesse> IniettaEccezioni ( List<MyRai_Eccezioni_Ammesse> ammesse ,
            List<string> codiciEccDB , DateTime Date , List<string> rimuoviPerLimiteG )
        {
            return EccezioniHelper.IniettaEccezioni( ammesse , codiciEccDB , Date , rimuoviPerLimiteG );
        }



        public static string[] getammesse ( List<MyRai_Eccezioni_Ammesse> ammesse )
        {
            return ammesse.Select( x => x.cod_eccezione ).ToArray( );
        }

        public static List<MyRai_Eccezioni_Ammesse> GetListaEccezioniPossibili ( string matricola , DateTime data , dayResponse d = null )
        {
            var extraAssemblies = GetExtraAssemblies( );
            return EccezioniHelper.GetListaEccezioniPossibili( matricola , data , d , extraAssemblies );
        }
        public static string GetIntervalloRMTRinsediamento ( dayResponse R , string Orario_Importo = "O" )
        {
            return EccezioniHelper.GetIntervalloRMTRinsediamento( R , Orario_Importo );
        }
        private static bool EccezioniQsuGapp ( dayResponse resp )
        {
            return EccezioniHelper.EccezioniQsuGapp( resp );
        }

        private static HashSet<string> RimuoviSeEscluseDaDB ( List<MyRai_Eccezioni_Ammesse> EccezioniAmmesse , string[] EccezioniOdierne , HashSet<string> CodiciEccezioniDB )
        {
            return EccezioniHelper.RimuoviSeEscluseDaDB( EccezioniAmmesse , EccezioniOdierne , CodiciEccezioniDB );
        }

        public static List<string> RimuoviFittizieGiaInDB ( List<MyRai_Eccezioni_Ammesse> ammesse , DateTime data )
        {
            return EccezioniHelper.RimuoviFittizieGiaInDB( ammesse , data );
        }

        public static Boolean IsEccezione_0_50 ( string codice )
        {
            var db = new digiGappEntities( );
            var ecc = db.L2D_ECCEZIONE.Where( x => x.cod_eccezione.Trim( ).ToUpper( ) == codice.Trim( ).ToUpper( ) ).FirstOrDefault( );
            return ( ecc != null && ( ecc.flag_macroassen == "M" || ecc.flag_macroassen == "P" ) );
        }

        public static Boolean IsEccezioneAQuarti ( string codice )
        {
            return EccezioniHelper.IsEccezioneAQuarti( codice );
        }

        public static Eccezione GetItemEccezione ( MyRai_Eccezioni_Richieste ec )
        {
            var db = new digiGappEntities( );
            if ( ec.azione == "C" )
            {
                ec.numero_documento = ec.numero_documento_riferimento;
            }
            Eccezione e = new Eccezione( )
            {

                dalle = "" ,
                alle = "" ,
                stato_eccezione = "D" ,
                tipo_eccezione = ec.tipo_eccezione ,
                ndoc = ec.numero_documento.ToString( ).PadLeft( 6 , '0' ) ,
                data = ec.data_eccezione.ToString( "ddMMyyyy" ) ,
                cod = ec.cod_eccezione.PadRight( 4 , ' ' ) ,
                matricola = ec.MyRai_Richieste.matricola_richiesta.PadLeft( 7 , '0' ) ,
                descrittiva_lunga = db.MyRai_Eccezioni_Ammesse.Where( x => x.cod_eccezione == ec.cod_eccezione ).Select( x => x.desc_eccezione ).FirstOrDefault( )
            };
            if ( ec.quantita == 1 )
                e.qta = "1";
            if ( ec.quantita == null )
                e.qta = "";
            else
            {
                var eccezionel2d = db.L2D_ECCEZIONE.Where( x => x.cod_eccezione == ec.cod_eccezione ).FirstOrDefault( );
                if ( eccezionel2d != null && eccezionel2d.unita_misura == "H" )
                {
                    e.qta = ( ( decimal ) ec.quantita ).ToString( "F" );
                }

            }
            if ( !IsEccezioneAQuarti( ec.cod_eccezione ) )
            {
                //mod. 05092017: se non è eccezione a quarti (PRQ) converti in minuti

                if ( ec.dalle != null )
                {
                    e.dalle = EccezioniManager.calcolaMinuti((DateTime)ec.dalle).ToString().PadLeft(4, '0');
                }
                if ( ec.alle != null )
                {
                    e.alle = EccezioniManager.calcolaMinuti((DateTime)ec.alle).ToString().PadLeft(4, '0');
                }
            }
            else
            {
                //se è eccezione a quarti, quindi lascia HH:MM
                e.dalle = ( ( DateTime ) ec.dalle ).ToString( "HH:mm" );
                e.alle = ( ( DateTime ) ec.alle ).ToString( "HH:mm" );

            }

            if ( IsEccezioneAQuarti( ec.cod_eccezione ) )
                e.qta = "0.25";
            else
                if ( IsEccezione_0_50( ec.cod_eccezione ) )
            {
                e.qta = "0.50";
            }
            else
                    if ( ec.alle != null && ec.dalle != null )
            {
                TimeSpan di = ( ( DateTime ) ec.alle ).Subtract( ( DateTime ) ec.dalle );

                if ( di.TotalMinutes > 0 )
                    e.qta = EccezioniManager.CalcolaQuantitaOreMinuti(e.dalle, e.alle);
            }

            return e;
        }



        public static Boolean InvioIstantaneo ( string tipoEvento , string matricola )
        {
            var db = new digiGappEntities( );
            var setting = db.MyRai_InvioNotifiche.Where( x => x.Matricola.Contains( matricola ) && x.TipoEvento == tipoEvento ).FirstOrDefault( );

            if ( setting != null )
                return setting.TipoInvio == "I";

            return db.MyRai_InvioNotifiche.Any( x => x.Matricola == "*" && x.TipoEvento == tipoEvento && x.TipoInvio == "I" );
        }

        public static string GetTurnoFromJson ( string json )
        {
            var DD = Newtonsoft.Json.JsonConvert.DeserializeObject<List<KeyValuePair<string , string>>>( json );
            if ( DD == null )
                return null;

            foreach ( var item in DD )
            {
                if ( item.Key == "TURNO" )
                    return item.Value;
            }
            return null;
        }
        public static Boolean MPbefore ( DateTime D )
        {
            var getListaEvidenze = SessionHelper.Get( SessionVariables.ListaEvidenzeScrivania );
            if ( getListaEvidenze != null )
            {
                var sessionData = ( SessionListaEvidenzeModel ) getListaEvidenze;
                return sessionData.ListaEvidenze.data.giornate.Any( x => x.TipoEcc == TipoEccezione.MaggiorPresenza && x.data < D );
            }
            return false;
        }
        public static string CambiaTurno ( MyRai_Eccezioni_Richieste eccez )
        {

            string NuovoTurno = GetTurnoFromJson( eccez.ValoriParamExtraJSON );
            if ( String.IsNullOrWhiteSpace( NuovoTurno ) )
                return "Turno non trovato";

            WSDigigapp serviceWS = new WSDigigapp( )
            {
                Credentials = CommonHelper.GetUtenteServizioCredentials( )
            };

            try
            {
                var resp = serviceWS.getEccezioni( eccez.MyRai_Richieste.matricola_richiesta , eccez.data_eccezione.ToString( "ddMMyyyy" ) , "BU" , 70 );
                string olddata = resp.data.Substring( 142 , 62 ) + resp.giornata.tipoDipendente;

                var response = serviceWS.updateEccezione( eccez.MyRai_Richieste.matricola_richiesta , eccez.data_eccezione.ToString( "ddMMyyyy" ) ,
                    "" , olddata , "" , "" , "" , "" , ' ' , "" , "" , "" , "" , "" , resp.giornata.orarioTeorico.PadRight( 2 , ' ' ) , NuovoTurno.PadRight( 2 , ' ' ) , 80 );

                if ( response.esito == false || response.codErrore != "0000" )
                    return response.descErrore;

                resp = serviceWS.getEccezioni( eccez.MyRai_Richieste.matricola_richiesta , eccez.data_eccezione.ToString( "ddMMyyyy" ) , "BU" , 70 );

                if ( resp.giornata.orarioReale.Trim( ) != NuovoTurno.Trim( ) )
                    return "Impossibile modificare orario";

                Logger.LogAzione( new MyRai_LogAzioni( )
                {
                    applicativo = "PORTALE" ,
                    data = DateTime.Now ,
                    matricola = CommonManager.GetCurrentUserMatricola( ) ,
                    provenienza = "CambiaTurno" ,
                    operazione = "CambioTurno" ,
                    descrizione_operazione = "Nuovo turno in Gapp: " + NuovoTurno + " - matr " + eccez.MyRai_Richieste.matricola_richiesta + " - data " + eccez.data_eccezione.ToString( "ddMMyyyy" )
                } );

                return null;
            }
            catch ( Exception ex )
            {
                Logger.LogErrori( new MyRai_LogErrori( )
                {
                    applicativo = "PORTALE" ,
                    data = DateTime.Now ,
                    error_message = ex.ToString( ) ,
                    matricola = CommonManager.GetCurrentUserMatricola( ) ,
                    provenienza = "CambiaTurno"
                } );
                return ex.Message;
            }
        }
        private static string ValidaRifiutaFittizia ( digiGappEntities db , MyRai_Eccezioni_Richieste eccez , bool valida , string NotaRifiuto_o_Approvazione = null )
        {
            var richPadre = eccez.MyRai_Richieste;
            if ( !valida )
            {
                foreach ( var e in richPadre.MyRai_Eccezioni_Richieste )
                {
                    e.id_stato = ( int ) EnumStatiRichiesta.Rifiutata;
                    e.data_rifiuto_primo_livello = DateTime.Now;
                    e.matricola_primo_livello = CommonManager.GetCurrentUserMatricola( );
                    e.nota_rifiuto_o_approvazione = NotaRifiuto_o_Approvazione;
                    e.nominativo_primo_livello = Utente.Nominativo();
                }
                richPadre.id_stato = ( int ) EnumStatiRichiesta.Rifiutata;

            }
            else
            {
                foreach ( var e in richPadre.MyRai_Eccezioni_Richieste )
                {
                    e.id_stato = ( int ) EnumStatiRichiesta.Approvata;
                    e.data_validazione_primo_livello = DateTime.Now;
                    e.matricola_primo_livello = CommonManager.GetCurrentUserMatricola( );
                    e.nota_rifiuto_o_approvazione = NotaRifiuto_o_Approvazione;
                    e.nominativo_primo_livello = Utente.Nominativo();
                }
                richPadre.id_stato = ( int ) EnumStatiRichiesta.Approvata;


                string EsitoCambio = CambiaTurno( eccez );
                if ( EsitoCambio != null )
                    return EsitoCambio;


                if ( eccez.cod_eccezione.Trim( ) == "CTA" )
                {
                    InserimentoEccezioneModel model = new InserimentoEccezioneModel( )
                    {
                        MatricolaForzataBatch = eccez.MyRai_Richieste.matricola_richiesta ,
                        data_da = eccez.data_eccezione.ToString( "dd/MM/yyyy" ) ,
                        data_a = eccez.data_eccezione.ToString( "dd/MM/yyyy" ) ,
                        cod_eccezione = "MA"
                    };

                    try
                    {
                        AggiungiEccezioneResponse esito = aggiungiEccezione( model , 90 , eccez.MyRai_Richieste.matricola_richiesta );

                        if ( esito.Error != null )
                            return esito.Error;

                        if ( String.IsNullOrWhiteSpace( esito.NumDoc ) || int.Parse( esito.NumDoc ) == 0 )
                            return "Non è stato rilasciato un numero documento valido.";

                        Logger.LogAzione( new MyRai_LogAzioni( )
                        {
                            applicativo = "PORTALE" ,
                            data = DateTime.Now ,
                            matricola = richPadre.matricola_richiesta ,
                            provenienza = "ValidaRifiutaFittizia" ,
                            operazione = "Inserimento MA per Cambio Turno" ,
                            descrizione_operazione = "Ndoc " + esito.NumDoc + " - IdEccez " + richPadre.id_richiesta + " - Matr Dip " + richPadre.matricola_richiesta
                        } );
                    }
                    catch ( Exception ex )
                    {
                        return ex.Message;
                    }
                }
            }

            if ( !DBHelper.Save( db , CommonHelper.GetCurrentUserMatricola( ) ) )
            {
                return "Errore aggiornamento DB";
            }

            if ( valida )
                CeitonManager.AggiornaGiornataCeiton( richPadre.id_richiesta );

            NotificaCambioTurno( eccez , valida );

            myRaiCommonTasks.NotificheDestinateDipendenti.SendNotificheESS_DIP( richPadre.matricola_richiesta );

            return null;
        }

        public static void NotificaCambioTurno ( MyRai_Eccezioni_Richieste eccez , bool Approvata )
        {
            if ( !Approvata )
                NotificheManager.InserisciNotifica(
                                "Richiesta rifiutata" + ": " + eccez.cod_eccezione + " del " + eccez.MyRai_Richieste.periodo_dal.ToString( "dd/MM/yyyy" ) ,
                                EnumCategoriaNotifica.RifiutoEccezione.ToString( ) ,
                                eccez.MyRai_Richieste.matricola_richiesta , "Portale" , eccez.id_richiesta );
            else
                NotificheManager.InserisciNotifica(
                                "Richiesta approvata" + ": " + eccez.cod_eccezione + " del " + eccez.MyRai_Richieste.periodo_dal.ToString( "dd/MM/yyyy" ) ,
                                EnumCategoriaNotifica.ApprovazioneEccezione.ToString( ) ,
                                eccez.MyRai_Richieste.matricola_richiesta , "Portale" , eccez.id_richiesta );
        }

        public static void AggiornaDatiVisto ( int IdEccezioneRichiesta , bool valida , string nota )
        {
            var db = new digiGappEntities( );
            MyRai_Eccezioni_Richieste eccez = db.MyRai_Eccezioni_Richieste.Where( x => x.id_eccezioni_richieste == IdEccezioneRichiesta ).FirstOrDefault( );
            if ( eccez != null )
            {
                string matr = CommonManager.GetCurrentUserMatricola( );
                var richPadre = eccez.MyRai_Richieste;
                foreach ( var ecc in richPadre.MyRai_Eccezioni_Richieste )
                {
                    ecc.matricola_visto = matr;
                    ecc.nominativo_visto = UtenteHelper.Nominativo( );
                    if ( valida )
                        ecc.data_visto_validato = DateTime.Now;
                    else
                        ecc.data_visto_rifiutato = DateTime.Now;
                    ecc.nota_visto = nota;
                }
                db.SaveChanges( );
            }
        }
        public static List<string> PossibileStornare(int IdEccezioneRichiesta)
        {
            List<string> L = new List<string>();
            var db = new digiGappEntities();
            MyRai_Eccezioni_Richieste eccez = db.MyRai_Eccezioni_Richieste.Where(x => x.id_eccezioni_richieste == IdEccezioneRichiesta).FirstOrDefault();

            if (eccez.MyRai_Richieste.matricola_richiesta != CommonManager.GetCurrentUserMatricola())
            {
                return L;
            }
            string[] EccezioniConVerificaStorno = CommonManager.GetParametri<string>(EnumParametriSistema.EccezioniConVerificaStorno);
            if (EccezioniConVerificaStorno == null)
            {
                return L;
            }
            string[] EccezioniDaContr = EccezioniConVerificaStorno[0] . Split(',');
            if (EccezioniDaContr == null || EccezioniDaContr.Length == 0 || 
                ! EccezioniDaContr.Contains(eccez.cod_eccezione.Trim()))
            {
                return L;
            }

            WSDigigapp service = new WSDigigapp();
            service.Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            var resp = service.getEccezioni(eccez.MyRai_Richieste.matricola_richiesta, eccez.data_eccezione.ToString("ddMMyyyy"),
                "BU", 80);

            if (resp.eccezioni == null || resp.eccezioni.Length == 0)
            {
                return L;
            }
            string[] EccezioniPossibiliPresenti = EccezioniConVerificaStorno[1].Split(',');
            var Presenti = resp.eccezioni.Where(x => x.cod.Trim() != "CAR").Select(x => x.cod.Trim()).ToList();
            if ( ! Presenti.Any())
            {
                return L;
            }
            
            foreach (var e in Presenti)
            {
                if ( ! EccezioniPossibiliPresenti.Contains(e))
                {
                    L.Add(e);
                }
            }
            return L;
        }
        public static string Valida_Rifiuta ( int IdEccezioneRichiesta , bool valida , string NotaRifiuto_o_Approvazione = null ,
            Boolean AggiornaStatoDB = true , Boolean InviaEmail = true , Boolean CancellazioneRichiestaDaDipendente = false )
        {

            WSDigigapp datiBack = new WSDigigapp( );
            Boolean PremutoValida = valida;
            String OverrideNotifica = "";

            var db = new digiGappEntities( );
            MyRai_Eccezioni_Richieste eccez = db.MyRai_Eccezioni_Richieste.Where( x => x.id_eccezioni_richieste == IdEccezioneRichiesta ).FirstOrDefault( );
            if ( eccez == null )
                return "Errore DB, impossibile recuperare dati della richiesta";

            if (valida == false)
            {
                List<string> NonValide = PossibileStornare(IdEccezioneRichiesta);
                if (NonValide.Any())
                {
                    string codici = String.Join(",", NonValide.ToArray());
                    if (NonValide.Count==1)
                        return "Eliminare l'eccezione " + codici + " prima di procedere allo storno";
                    else
                        return "Eliminare le eccezioni " + codici + " prima di procedere allo storno";
                }
            }

            if ( EccezioniManager.IsEccezioneFittiziaNoCorrispondenzaGapp( eccez.cod_eccezione ) )
            {
                return ValidaRifiutaFittizia( db , eccez , valida , NotaRifiuto_o_Approvazione );
            }

            //se rifiutiamo uno storno, cambiamo gli stati sul DB senza comunicare niente a CICS
            if ( eccez.azione == "C" && valida == false )
            {
                var richPadre = eccez.MyRai_Richieste;
                foreach ( var e in richPadre.MyRai_Eccezioni_Richieste.Where( x => x.azione == "C" ) )
                {
                    e.id_stato = ( int ) EnumStatiRichiesta.Rifiutata;
                    e.data_rifiuto_primo_livello = DateTime.Now;
                    e.matricola_primo_livello = CommonManager.GetCurrentUserMatricola( );
                    e.nota_rifiuto_o_approvazione = NotaRifiuto_o_Approvazione;
                    e.nominativo_primo_livello = UtenteHelper.Nominativo( );
                }
                if ( AggiornaStatoDB )
                {
                    if ( !DBHelper.Save( db , CommonHelper.GetCurrentUserMatricola( ) ) )
                    {
                        return "Errore aggiornamento DB";
                    }
                }
                richPadre.id_stato = richPadre.MyRai_Eccezioni_Richieste
                      .Where( x => x.numero_documento != null && x.numero_documento != 0 )
                    .Min( x => x.id_stato );

                if ( AggiornaStatoDB )
                {
                    if ( !DBHelper.Save( db , CommonHelper.GetCurrentUserMatricola( ) ) )
                    {
                        return "Errore aggiornamento DB";
                    }
                }
                Logger.LogAzione( new MyRai_LogAzioni( )
                {
                    applicativo = "PORTALE" ,
                    data = DateTime.Now ,
                    matricola = CommonManager.GetCurrentUserMatricola( ) ,
                    provenienza = "EccezioniManager.Valida_Rifiuta" ,
                    descrizione_operazione = "Rifiuto storno ideccezione_rich:" + IdEccezioneRichiesta
                } );
                NotificheManager.InserisciNotifica( "Storno rifiutato: " + eccez.cod_eccezione + " del " + eccez.data_eccezione.ToString( "dd/MM/yyyy" ) , EnumCategoriaNotifica.RifiutoEccezione.ToString( ) ,
                    eccez.MyRai_Richieste.matricola_richiesta , "Portale" , eccez.id_richiesta );

                return null;
            }
            string EventualeStornoPerDescrizioneMail = "";
            //se stiamo validando uno storno, in realta rifiutiamo la eccez a cui si riferisce
            if ( eccez.azione == "C" && valida == true )
            {
                var richPadre = eccez.MyRai_Richieste;
                eccez = db.MyRai_Eccezioni_Richieste.Where(x =>
                x.numero_documento == eccez.numero_documento_riferimento
                    && x.codice_sede_gapp == richPadre.codice_sede_gapp &&
                    x.MyRai_Richieste.matricola_richiesta == richPadre.matricola_richiesta &&
                    x.data_eccezione == eccez.data_eccezione).FirstOrDefault();
                if ( eccez == null )
                    return "Eccezione di riferimento non trovata";
                valida = false;
                EventualeStornoPerDescrizioneMail = " (STORNO) ";
                OverrideNotifica = "Storno approvato ";
            }

            //da qui è una approvazione valida
            var RichiestaPadre = eccez.MyRai_Richieste;
            string descrizione_eccezione = "";

            List<Eccezione> Lecc = new List<Eccezione>( );
            foreach ( var ec in RichiestaPadre.MyRai_Eccezioni_Richieste
                .Where( x => ( x.id_stato == ( int ) EnumStatiRichiesta.InApprovazione || x.id_stato == ( int ) EnumStatiRichiesta.Approvata ) &&
                 x.numero_documento != null && x.numero_documento != 0 ) )
            {
                Eccezione myecc = GetItemEccezione( ec );
                descrizione_eccezione = myecc.descrittiva_lunga;
                Lecc.Add( myecc );
            }

            if ( Lecc.Count == 0 )
            {
                Logger.LogErrori( new MyRai_LogErrori( )
                {
                    error_message = "Nessuna eccezione" ,
                    provenienza = "EccezioniManager.Valida_rifiuta"
                } );
                return "Nessuna eccezione da validare";
            }

            String MatricolaApprovatoreGAPP = CommonManager.GetCurrentUserMatricola( );
            bool BypassApprovazioni = false;
            String MatricolaApprovatoreDB = MatricolaApprovatoreGAPP;

            if ( UtenteHelper.IsAdmin( CommonHelper.GetCurrentUserMatricola( ) ) )
            {
                string[] m = CommonManager.GetParametri<string>( EnumParametriSistema.BypassApprovazioni );

                if ( !String.IsNullOrWhiteSpace( m[0] ) )
                {
                    MatricolaApprovatoreGAPP = m[0];
                    BypassApprovazioni = true;

                    if ( m[1] == "SELF" )
                        MatricolaApprovatoreDB = RichiestaPadre.matricola_richiesta;
                    else
                        MatricolaApprovatoreDB = m[1];
                }
            }

            try
            {
                //SERVIZIO//

                // se è una approvazione, invia richiesta di validazione a CICS
                if ( valida == true )
                {
                    validationResponse resp = new validationResponse( );

                    //resp = datiBack.validaEccezioni(CommonManager.GetCurrentUserMatricola(), "01", e, valida, 75);
                    resp = ServiceWrapper.validaEccezioni( datiBack , MatricolaApprovatoreGAPP , "01" ,
                        Lecc.ToArray( ) ,
                           valida , 75 );

                    string ErroreResponse = HomeManager.ErroreInValidationResponse( resp );

                    if ( ErroreResponse == null )
                    {
                        Logger.LogAzione( new MyRai_LogAzioni( )
                        {
                            operazione = "Validazione eccezione id " + RichiestaPadre.id_richiesta ,
                            provenienza = "ajaxcontroller/Valida_Rifiuta datiBack.validaEccezioni"
                        } );

                        //Aggiorna Servizio Ceiton con giornata dipendente

                        bool ceitonUpdated = CeitonManager.AggiornaGiornataCeiton( RichiestaPadre.id_richiesta );

                        if ( !CancellazioneRichiestaDaDipendente )
                        {
                            if ( RichiestaPadre.periodo_al == RichiestaPadre.periodo_dal )
                            {
                                if ( !BypassApprovazioni )
                                    NotificheManager.InserisciNotifica( "Richiesta approvata: " + eccez.cod_eccezione + " del " +
           RichiestaPadre.periodo_dal.ToString( "dd/MM/yyyy" ) , EnumCategoriaNotifica.ApprovazioneEccezione.ToString( ) ,
           eccez.MyRai_Richieste.matricola_richiesta , "Portale" , eccez.id_richiesta );
                            }
                            else
                            {
                                if ( !BypassApprovazioni )
                                    NotificheManager.InserisciNotifica( "Richiesta approvata: " + eccez.cod_eccezione + " periodo " +
           RichiestaPadre.periodo_dal.ToString( "dd/MM/yyyy" ) + " - " + RichiestaPadre.periodo_al.ToString( "dd/MM/yyyy" ) ,
           EnumCategoriaNotifica.ApprovazioneEccezione.ToString( ) ,
           eccez.MyRai_Richieste.matricola_richiesta , "Portale" , eccez.id_richiesta );
                            }
                        }
                    }
                    else
                    {
                        Logger.LogErrori( new MyRai_LogErrori( )
                        {
                            error_message = ErroreResponse ,
                            provenienza = "ajaxcontroller/Valida_Rifiuta datiBack.validaEccezioni"
                        } );
                        return "Errore servizio: " + ErroreResponse;
                    }
                }
                else // se è un rifiuto/storno, valida eccezione e poi storna
                {
                    //validationResponse resp = datiBack.validaEccezioni(CommonManager.GetCurrentUserMatricola(),
                    //                              "01", e, true, 75);
                    validationResponse resp = ServiceWrapper.validaEccezioni( datiBack , MatricolaApprovatoreGAPP ,
                                                    "01" , Lecc.ToArray( ) , true , 75 );
                    string ErroreResponse = HomeManager.ErroreInValidationResponse( resp );
                    if ( ErroreResponse == null )
                    {
                        Logger.LogAzione( new MyRai_LogAzioni( )
                        {
                            operazione = "Validazione eccezione id " + RichiestaPadre.id_richiesta ,
                            provenienza = "ajaxcontroller/Valida_Rifiuta datiBack.validaEccezioni"
                        } );
                        // non si inserisce notifica perche questa validazione serve solo a cics 
                    }
                    else
                    {
                        Logger.LogErrori( new MyRai_LogErrori( )
                        {
                            error_message = ErroreResponse ,
                            provenienza = "ajaxcontroller/Valida_Rifiuta datiBack.validaEccezioni"
                        } );
                        return "Errore servizio: " + ErroreResponse;
                    }
                    //validationResponse resp2 = datiBack.validaEccezioni(CommonManager.GetCurrentUserMatricola(),
                    //                              "01", e, false, 75);
                    // INVIA STORNO
                    validationResponse resp2 = ServiceWrapper.validaEccezioni( datiBack , MatricolaApprovatoreGAPP ,
                                                "01" , Lecc.ToArray( ) , false , 90 );

                    ErroreResponse = HomeManager.ErroreInValidationResponse( resp2 );
                    if (ErroreResponse == null || ErroreResponse.Contains("MANCANZA DI GIUSTIFICATIVO DA STORNARE AL *"))
                    {
                        if (ErroreResponse != null && ErroreResponse.Contains("MANCANZA DI GIUSTIFICATIVO DA STORNARE AL *"))
                        {
                            Logger.LogAzione(new MyRai_LogAzioni()
                            {
                                operazione = "Validazione storno eccezione id " + RichiestaPadre.id_richiesta,
                                provenienza = "ajaxcontroller/Valida_Rifiuta datiBack.validaEccezioni",
                                descrizione_operazione = "MANCANZA DI GIUSTIFICATIVO DA STORNARE AL * (considerato come storno approvato)"
                            });
                        }
                        bool esito = CeitonManager.AggiornaGiornataCeiton( RichiestaPadre.id_richiesta );

                        Logger.LogAzione( new MyRai_LogAzioni( )
                        {
                            operazione = "Rifiuto eccezione id " + RichiestaPadre.id_richiesta ,
                            provenienza = "ajaxcontroller/Valida_Rifiuta datiBack.validaEccezioni"
                        } );
                        if ( RichiestaPadre.periodo_al == RichiestaPadre.periodo_dal )
                        {
                            if ( String.IsNullOrWhiteSpace( OverrideNotifica ) )
                            {
                                if ( !BypassApprovazioni )
                                    NotificheManager.InserisciNotifica(
       ( CancellazioneRichiestaDaDipendente == true ? "Richiesta cancellata: " : "Richiesta rifiutata: " )
       + eccez.cod_eccezione + " del " + RichiestaPadre.periodo_dal.ToString( "dd/MM/yyyy" ) ,
       EnumCategoriaNotifica.RifiutoEccezione.ToString( ) ,
       eccez.MyRai_Richieste.matricola_richiesta , "Portale" , eccez.id_richiesta );
                            }
                            else
                            {
                                CommonManager.InserisciEccezioneRipianificata( IdEccezioneRichiesta );

                                if ( !BypassApprovazioni )
                                    NotificheManager.InserisciNotifica(
           OverrideNotifica //storno approvato
           + eccez.cod_eccezione + " del " + RichiestaPadre.periodo_dal.ToString( "dd/MM/yyyy" ) ,
          ( PremutoValida ? EnumCategoriaNotifica.ApprovazioneEccezione.ToString( ) : EnumCategoriaNotifica.RifiutoEccezione.ToString( ) ) ,
           eccez.MyRai_Richieste.matricola_richiesta , "Portale" , eccez.id_richiesta );
                            }
                        }
                        else
                        {
                            if ( String.IsNullOrWhiteSpace( OverrideNotifica ) )
                            {
                                if ( !BypassApprovazioni )
                                    NotificheManager.InserisciNotifica(
         ( CancellazioneRichiestaDaDipendente == true ? "Richiesta cancellata: " : "Richiesta rifiutata: " )
         + eccez.cod_eccezione + " periodo " + RichiestaPadre.periodo_dal.ToString( "dd/MM/yyyy" ) + " - "
         + RichiestaPadre.periodo_al.ToString( "dd/MM/yyyy" ) ,
         EnumCategoriaNotifica.RifiutoEccezione.ToString( ) ,
         eccez.MyRai_Richieste.matricola_richiesta , "Portale" , eccez.id_richiesta );
                            }
                            else
                            {
                                if ( !BypassApprovazioni )
                                    NotificheManager.InserisciNotifica(
             OverrideNotifica//storno approvato
             + eccez.cod_eccezione + " periodo " + RichiestaPadre.periodo_dal.ToString( "dd/MM/yyyy" ) + " - "
             + RichiestaPadre.periodo_al.ToString( "dd/MM/yyyy" ) ,
             ( PremutoValida ? EnumCategoriaNotifica.ApprovazioneEccezione.ToString( ) : EnumCategoriaNotifica.RifiutoEccezione.ToString( ) ) ,
             eccez.MyRai_Richieste.matricola_richiesta , "Portale" , eccez.id_richiesta );
                            }

                        }

                        //resetta le assenze ingiustificate(rifuto potrebbe cambiare la risposta ass.ing.)
                        SessionHelper.Set( "FlagEvidenze" , null );
                        UtenteHelper.GiornateAssenteIngiustificato( CommonHelper.GetCurrentUserMatricola( ) , true );
                    }
                    else
                    {
                        Logger.LogErrori( new MyRai_LogErrori( )
                        {
                            error_message = ErroreResponse ,
                            provenienza = "ajaxcontroller/Valida_Rifiuta datiBack.validaEccezioni"
                        } );
                        return "Errore servizio: " + ErroreResponse;
                    }
                }
            }
            catch ( Exception ex )
            {
                return ex.Message + "-" + ex.InnerException;
            }

            //AGGIORNAMENTO DB////////////////////////////////////
            foreach (var ec in RichiestaPadre.MyRai_Eccezioni_Richieste.Where(x =>
            x.id_stato == (int)EnumStatiRichiesta.InApprovazione
            || (PremutoValida == false && x.id_stato == (int)EnumStatiRichiesta.Approvata)) //per chi ci ripensa
            )
            {
                if ( PremutoValida )
                {
                    ec.id_stato = ( int ) EnumStatiRichiesta.Approvata;
                    ec.data_validazione_primo_livello = DateTime.Now;
                }
                else
                {
                    ec.id_stato = ( int ) EnumStatiRichiesta.Rifiutata;
                    ec.data_rifiuto_primo_livello = DateTime.Now;
                }
                ec.matricola_primo_livello = MatricolaApprovatoreDB;
                ec.nota_rifiuto_o_approvazione = NotaRifiuto_o_Approvazione;
                ec.nominativo_primo_livello = BypassApprovazioni == false ? Utente.Nominativo() : MatricolaApprovatoreDB;
            }

            if ( AggiornaStatoDB )
            {
                if ( !DBHelper.Save( db , CommonHelper.GetCurrentUserMatricola( ) ) )
                {
                    return "Errore aggiornamento DB";
                }
            }

            if (!BypassApprovazioni)
                myRaiCommonTasks.NotificheDestinateDipendenti.SendNotificheESS_DIP(RichiestaPadre.matricola_richiesta);

            if ( RichiestaPadre.richiesta_in_resoconto && ( OverrideNotifica == "Storno approvato" || PremutoValida ) )
            {
                RichiestaPadre.richiesta_in_resoconto = ResocontiManager.GiaCopertaDaResoconto(
                RichiestaPadre.codice_sede_gapp ,
                RichiestaPadre.periodo_dal ,
                RichiestaPadre.periodo_al ,
                true );

                // reperimento dei reparti di cui sono responsabile per la sede esaminata
                List<string> SediReparti = RepartiManager.RepartiDiCuiSonoResponsabile( RichiestaPadre.codice_sede_gapp );

                // reperimento dell'elenco dei reparti della sede esaminata
                //	List<string> repartiSede = RepartiManager.GetRepartiAttiviPerSedeGapp( RichiestaPadre.codice_sede_gapp, RichiestaPadre.periodo_dal, RichiestaPadre.periodo_al );

                List<string> repartiMiei = SediReparti.Select( x => x.Substring( 5 ) ).ToList( );

                // TODO 
                // rimozione dei record su [MyRai_PdfReparti] se esistono record
                // per l'intervallo temporale esaminato per le sedi/reparti di mia competenza
                if ( repartiMiei != null && repartiMiei.Any( ) )
                {
                    foreach ( var rep in repartiMiei )
                    {
                        //string str1 = RichiestaPadre.periodo_dal.ToString("dd/MM/yyyy");
                        //string str2 = RichiestaPadre.periodo_al.ToString("dd/MM/yyyy");

                        DateTime da = RichiestaPadre.periodo_dal;
                        DateTime a = RichiestaPadre.periodo_al;

                        var pdfReparti = db.MyRai_PdfReparti.Where( w => w.sedegapp.Equals( RichiestaPadre.codice_sede_gapp ) && w.reparto.Equals( rep ) && w.periodo_dal == da && w.periodo_al == a ).FirstOrDefault( );

                        if ( pdfReparti != null )
                        {
                            // se esiste la riga allora la rimuove
                            db.MyRai_PdfReparti.Remove( pdfReparti );
                        }
                    }
                }
            }

            RichiestaPadre.id_stato = RichiestaPadre.MyRai_Eccezioni_Richieste
                  .Where( x => x.numero_documento != null && x.numero_documento != 0 )
                .Min( x => x.id_stato );

            if ( !String.IsNullOrWhiteSpace( OverrideNotifica ) )//se è uno storno approvato
            {
                var cop = db.MyRai_CoperturaCarenze.Where( x => x.id_richiesta == RichiestaPadre.id_richiesta ).FirstOrDefault( );
                if ( cop != null )
                    db.MyRai_CoperturaCarenze.Remove( cop );
            }
            if ( AggiornaStatoDB )
            {
                if ( !DBHelper.Save( db , CommonHelper.GetCurrentUserMatricola( ) ) )
                {
                    return "Errore aggiornamento DB";
                }
            }

            return null;
        }



        //public static int POHmese()
        //{
        //    var response = Utente.GetAnalisiEccezioni();
        //    if (response == null ||
        //        response.DettagliEccezioni == null ||
        //        response.DettagliEccezioni.Where(x => x.eccezione == "POH").Count() == 0)
        //    {
        //        return 0;
        //    }
        //    DateTime firstMese = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        //    return response.DettagliEccezioni.Where(x => x.eccezione == "POH" && x.data >= firstMese).Count();
        //}
        //public static void LoggaPOHsuperati(int poh, int massimale)
        //    {
        //        Logger.LogAzione(new MyRai_LogAzioni()
        //        {
        //        applicativo = "PORTALE",
        //        data = DateTime.Now,
        //        matricola = CommonManager.GetCurrentUserMatricola(),
        //        provenienza = "LoggaPOHsuperati",
        //        descrizione_operazione = "POH soppresso perche raggiunto massimale (nel mese:" + poh + "/ max:" + massimale + ")"
        //        });
        //    }

        public static Eccezione GetEccezione ( string matricola , string data , string cod , string oldArea , string ndoc ,
                string dalle , string alle , string importo , string note , string uorg , string df , string ms ,
                string new_orario_teorico , string new_orario_reale )
        {
            if ( ndoc == null )
                ndoc = "";
            if ( note == null )
                note = "";
            if ( uorg == null )
                uorg = "";
            if ( df == null )
                df = "";
            if ( ms == null )
                ms = "";
            if ( new_orario_teorico == null )
                new_orario_teorico = "";
            if ( new_orario_reale == null )
                new_orario_reale = "";
            importo = "";

            string tipoEccezione = CommonManager.GetTipoEccezione( cod );

            Eccezione ec = new Eccezione( )
            {
                alle = alle ,
                dalle = dalle ,
                cod = cod ,
                data = data ,
                qta = "1" ,
                matricola = matricola ,
                importo = importo ,
                ndoc = ndoc ,
                tipo_eccezione = tipoEccezione ,
                stato_eccezione = "D" ,
                flg_storno = " " ,
                note = note
            };
            if ( !String.IsNullOrWhiteSpace( alle ) && !String.IsNullOrWhiteSpace( dalle ) )
                ec.qta = CalcolaQuantitaOreMinuti(dalle, alle);

            ///////////////////////////////////////////////
            var db = new digiGappEntities( );
            var edb = db.L2D_ECCEZIONE.Where( x => x.cod_eccezione == cod ).FirstOrDefault( );
            if ( edb != null && edb.unita_misura == "G" && alle == "0000" && dalle == "0000" )
                ec.qta = "1";

            return ec;
        }

        public static validationResponse aggiungiEccezioniArray ( Eccezione[] ArrayEccezioni )
        {
            WSDigigapp serviceWS = new WSDigigapp( )
            {
                Credentials = CommonHelper.GetUtenteServizioCredentials( )
            };

            WSDigigappDataController service = new WSDigigappDataController( );
            service.ClearAll( );

            string codiceEccezione = "";
            foreach ( Eccezione e in ArrayEccezioni )
            {
                codiceEccezione = e.cod;
                string tipoEccezione = CommonManager.GetTipoEccezione( e.cod );
                if ( tipoEccezione != null && tipoEccezione.Trim( ) == "C" )
                {
                    //var resp = service.getEccezioni(CommonManager.GetCurrentUserMatricola(), e.data, "BU", 70);

                    var resp = service.GetEccezioni(CommonManager.GetCurrentUserMatricola(), CommonManager.GetCurrentUserMatricola(), e.data, "BU", 70);

                    if ( resp.eccezioni.Any( x => x.cod != null && x.cod.Trim( ).ToUpper( ) == e.cod.Trim( ).ToUpper( ) ) )
                    {
                        return new validationResponse { errore = "Tipo di inserimento già presente in questa data" };
                    }
                }
            }

            int livUtente = EccezioniManager.InserimentoEccezioneGiaApprovata( codiceEccezione ) ? 82 : 70;

            validationResponse valResponse = ServiceWrapper.aggiornaEccezioni( serviceWS ,
                CommonManager.GetCurrentUserMatricola7chars( ) ,
                ArrayEccezioni , livUtente );

            return valResponse;

        }

        public static string CalcolaStringaOreMinuti ( int minuti )
        {
            int h = ( int ) minuti / 60;
            int min = minuti - ( h * 60 );
            return h.ToString( ).PadLeft( 2 , '0' ) + ":" + min.ToString( ).PadLeft( 2 , '0' );
        }

        public static string CalcolaQuantitaOreMinuti ( string dallemin , string allemin )
        {
            int dalle = Convert.ToInt32( dallemin );
            int alle = Convert.ToInt32( allemin );
            int diff = 0;

            if ( alle >= dalle )
                diff = alle - dalle;
            else
                diff = ( 1440 - dalle ) + alle;

            int h = ( int ) diff / 60;
            int min = diff - ( h * 60 );
            return h.ToString( ).PadLeft( 2 , '0' ) + ":" + min.ToString( ).PadLeft( 2 , '0' );
        }

        public static Boolean InserimentoEccezioneGiaApprovata ( string cod )
        {
            //if (Utente.IsBoss() || Utente.IsBossLiv2())
            var db = new digiGappEntities( );
            var eccezioneAmmessa = db.MyRai_Eccezioni_Ammesse.Where( x => x.cod_eccezione == cod ).FirstOrDefault( );
            if ( eccezioneAmmessa == null )
                return false;

            //if (Utente.IsLivello1PropriaSede() || Utente.IsLivello2PropriaSede())
            //{
            //    return (eccezioneAmmessa.id_workflow == 1);
            //}
            if ( !String.IsNullOrWhiteSpace( eccezioneAmmessa.AutoApprovataTipiDip ) )
            {
                return ( eccezioneAmmessa.AutoApprovataTipiDip == "*" ||
                    eccezioneAmmessa.AutoApprovataTipiDip.Contains(Utente.TipoDipendente()));
            }

            return false;
        }

        public static AggiungiEccezioneResponse aggiungiEccezione ( InserimentoEccezioneModel model , int livUtente , string matricola = null )
        {
            if ( !model.IsFromBatch )
            {
                WSDigigappDataController service = new WSDigigappDataController( );
                service.ClearAll( );
            }

            Boolean IsPQ = IsEccezioneAQuarti( model.cod_eccezione );
            Boolean Is50 = IsEccezione_0_50( model.cod_eccezione );

            DateTime dataDa;
            DateTime.TryParseExact( model.data_da , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out dataDa );
            DateTime dataA;
            DateTime.TryParseExact( model.data_a , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out dataA );

            string cod = model.cod_eccezione.PadRight( 4 , ' ' );

            string dalle = model.dalle == null ? "" : EccezioniManager.calcolaMinuti(model.dalle).ToString().PadLeft(4, '0');

            string alle = "";// model.alle == null ? "" : EccezioniManager.calcolaMinuti(model.alle).ToString().PadLeft(4, '0');

            if ( IsPQ )
            {
                int minutiQ = GetQuartoDiGiornataInMinuti( model.data_da );
                alle = ( Convert.ToInt32( dalle ) + minutiQ ).ToString( ).PadLeft( 4 , '0' );
            }
            else
            {
                if ( model.alle == null )
                    alle = "";
                else
                    alle = EccezioniManager.calcolaMinuti(model.alle).ToString().PadLeft(4, '0');
            }

            string quantita = "1";


            if ( !String.IsNullOrWhiteSpace( dalle ) && !String.IsNullOrWhiteSpace( alle ) )
                quantita = CalcolaQuantitaOreMinuti(dalle, alle);

            var db = new digiGappEntities( );
            var edb = db.L2D_ECCEZIONE.Where( x => x.cod_eccezione.Trim( ) == model.cod_eccezione.Trim( ) ).FirstOrDefault( );
            if ( edb != null &&
                edb.unita_misura == "G" &&
                ( ( dalle == "0000" && alle == "0000" ) || ( String.IsNullOrWhiteSpace( dalle ) && String.IsNullOrWhiteSpace( alle ) ) )
                )
            {
                quantita = "1";
            }

            if ( edb != null &&
                edb.unita_misura == "H" &&
                  ( ( dalle == "0000" && alle == "0000" ) || ( String.IsNullOrWhiteSpace( dalle ) && String.IsNullOrWhiteSpace( alle ) ) )
                )
            {
                quantita = model.quantita;
            }

            if ( IsPQ )
                quantita = "0.25";
            if ( Is50 )
                quantita = "0.50";


            //imposta quantita ore/minuti sebbene dalle/alle non valorizzate per eccezioni contenute in
            //parametro AccettaSoloDurata (SEH,POH,UMH) - altrimenti riporterebbe '1'
            string AccettanoSoloDurata = CommonManager.GetParametro<string>( EnumParametriSistema.AccettaSoloDurata );
            string DigitaSoloQuantita = CommonManager.GetParametro<string>( EnumParametriSistema.EccezioniDigitaSoloQuantita );

            if ( AccettanoSoloDurata.Split( ',' ).Contains( model.cod_eccezione )
                || DigitaSoloQuantita.Split( ',' ).Contains( model.cod_eccezione )
                || model.DontChangeQuantita )//se viene da proposte con dontchangeQUantita forza quantita del model
            {
                quantita = model.quantita;
            }
            //if (model.cod_eccezione == "SEH" || model.cod_eccezione == "POH" || model.cod_eccezione=="UMH") 
            //     quantita = model.quantita;

            if ( matricola == null )
                matricola = CommonManager.GetCurrentUserMatricola( );

            try
            {
                WSDigigapp serviceWS = new WSDigigapp( )
                {
                    Credentials = CommonHelper.GetUtenteServizioCredentials( )
                };

                //ottieni lo status attuale della giornata
                //var resp = service.getEccezioni(matricola, dataDa.ToString("ddMMyyyy"), "BU", 70);

                WSDigigappDataController serv = new WSDigigappDataController( );

                var resp = serv.GetEccezioni(CommonManager.GetCurrentUserMatricola(), matricola, dataDa.ToString("ddMMyyyy"), "BU", 70);
                string oldData = "";
                if (Utente.GiornalistaDelleReti() && resp.giornata.tipoDipendente=="3")
                {
                    oldData = resp.data.Substring(142, 62) + "G";
                }
                else
                {
                    oldData = resp.data.Substring(142, 62) + resp.giornata.tipoDipendente;
                }

                if ( IsPQ )
                {
                    oldData = oldData.Substring( 0 , 27 ) + resp.orario.prevista_presenza.PadLeft( 4 , '0' ) + oldData.Substring( 31 );
                }
                // se eccez di tipo C, possibile una sola al giorno
                string tipoEccezione = CommonManager.GetTipoEccezione( cod );
                if ( tipoEccezione != null && tipoEccezione.Trim( ) == "C" )
                {
                    if ( resp.eccezioni.Any( x => x.cod != null && x.cod.Trim( ).ToUpper( ) == cod.Trim( ).ToUpper( ) ) )
                    {
                        return new AggiungiEccezioneResponse( ) { Error = "Tipo di inserimento già presente in questa data" };
                    }
                }

                string importo = "";

                if ( !String.IsNullOrWhiteSpace( model.importo ) )
                    importo = model.importo.Replace( "," , "" ).Replace( "." , "" );

                updateResponse Response = ServiceWrapper.updateEccezione( serviceWS ,
                    matricola ,
                    dataDa.ToString( "ddMMyyyy" ) ,
                    cod ,            //cod
                    oldData ,        //
                    quantita ,       //formato 01:30
                    dalle ,          //formato int minuti
                    alle ,           //formato int minuti
                    importo ,             //importo
                    ' ' ,            //storno
                    "" ,             //ndoc
                    "" ,             //note
                    "" ,             //uorg
                    "" ,             //df
                    "" ,             //ms
                    "" ,             //orario teorico
                    "" ,             //orario reale
                    livUtente ,     //liv utente
                    resp
                    );
                if ( Response.esito == true && ( Response.codErrore == "0000" || Response.codErrore == "TX01" ) )
                {
                    if ( Response.codErrore == "TX01" )
                    {
                        LogSuperamentoMassimale( cod , matricola , dataDa.ToString( "ddMMyyyy" ) );
                    }
                    //se ok recupera ndoc e rispondi
                    AggiungiEccezioneResponse r = new AggiungiEccezioneResponse( )
                    {
                        Error = null ,
                        response = Response
                    };

                    int[] numdocPosition = CommonManager.GetParametri<int>( EnumParametriSistema.PosizioneNumDoc );

                    if ( !String.IsNullOrWhiteSpace( Response.rawInput )
                            && Response.rawInput.Length > numdocPosition[0] + numdocPosition[1] )
                    {
                        r.NumDoc = Response.rawInput.Substring( numdocPosition[0] , numdocPosition[1] );
                    }
                    int numdoc;
                    bool b = int.TryParse( r.NumDoc , out numdoc );
                    if ( b == false || numdoc == 0 )
                    {
                        Logger.LogErrori( new MyRai_LogErrori( )
                        {
                            applicativo = "PORTALE" ,
                            data = DateTime.Now ,
                            error_message = "Ndoc non presente in risposta " + r.response.rawInput ,
                            matricola = matricola ,
                            provenienza = "EccezioniManager.aggiungiEccezione"
                        } );
                        return new AggiungiEccezioneResponse( ) { Error = "Numero Documento non restituito da Cics" };
                    }
                    else
                        return r;
                }
                else
                    return new AggiungiEccezioneResponse( ) { Error = Response.codErrore + ": " + Response.descErrore };
            }
            catch ( Exception ex )
            {
                return new AggiungiEccezioneResponse( ) { Error = ex.Message + "-" + ex.InnerException };
            }
        }

        private static void LogSuperamentoMassimale ( string cod , string matricola , string data )
        {
            var db = new digiGappEntities( );
            db.MyRai_LogAzioni.Add( new MyRai_LogAzioni( )
            {
                applicativo = "PORTALE" ,
                data = DateTime.Now ,
                matricola = matricola ,
                provenienza = "LogSuperamentoMassimale" ,
                descrizione_operazione = "Superato massimale per " + matricola + " " + cod + " " + data
            } );
            DBHelper.Save( db , CommonHelper.GetCurrentUserMatricola( ) );
        }

        public static int GetWorkflow ( string codiceEccezione )
        {
            digiGappEntities db = new digiGappEntities( );
            return db.MyRai_Eccezioni_Ammesse.Where( x => x.cod_eccezione == codiceEccezione ).Select( x => x.id_workflow ).FirstOrDefault( );
        }

        public static int GetTreQuartiGiornataMinuti ( string pMatr , string matr , string data )
        {
            WSDigigappDataController service = new WSDigigappDataController( );

            dayResponse resp = service.GetEccezioni( pMatr, matr , data.Replace( "/" , "" ) , "BU" , 70 );

            int minutiTotali = 0;
            int.TryParse( resp.orario.prevista_presenza , out minutiTotali );
            return minutiTotali - ( minutiTotali / 4 );
        }

        public static int GetMetaGiornataMinuti ( string pMatr , string matr , string data )
        {
            WSDigigappDataController service = new WSDigigappDataController( );

            dayResponse resp = service.GetEccezioni( pMatr, matr , data.Replace( "/" , "" ) , "BU" , 70 );

            int minutiTotali = 0;
            int.TryParse( resp.orario.prevista_presenza , out minutiTotali );
            return minutiTotali / 2;
        }

        public static int GetQuartoDiGiornataInMinuti (string pMatr, string matr , string data )
        {
            WSDigigappDataController service = new WSDigigappDataController( );

            dayResponse resp = service.GetEccezioni( pMatr , matr , data.Replace( "/" , "" ) , "BU" , 70 );

            int minutiTotali = 0;
            int.TryParse( resp.orario.prevista_presenza , out minutiTotali );
            return minutiTotali / 4;
        }

        public static int GetQuartoDiGiornataInMinuti ( string data )
        {
            WSDigigappDataController service = new WSDigigappDataController( );

            dayResponse resp = service.GetEccezioni( CommonHelper.GetCurrentUserPMatricola( ) , CommonHelper.GetCurrentUserMatricola( ) , data.Replace( "/" , "" ) , "BU" , 70 );

            int minutiTotali = 0;
            int.TryParse( resp.orario.prevista_presenza , out minutiTotali );
            return minutiTotali / 4;
        }

        public static dayResponse GetGiornata ( string data , string matricola )
        {
            return EccezioniHelper.GetGiornata( data , matricola );
        }

        public static dayResponse GetGiornataForBatch ( string data , string matricola )
        {
            WSDigigappDataController service = new WSDigigappDataController( );

            dayResponse resp = service.GetEccezioniForBatch( matricola , data.Replace( "/" , "" ) , "BU" , 70 );

            return resp;
        }

        public static Boolean EccezioneNecessitaAttivitaCeiton ( string cod )
        {
            return new digiGappEntities( ).MyRai_Eccezioni_Ammesse.Where( x => x.cod_eccezione == cod && x.RichiedeAttivitaCeiton ).FirstOrDefault( ) != null;
        }
        public static bool IsEccezioneInGruppo2_3 ( string cod )
        {
            var db = new digiGappEntities( );
            int? gruppo = db.MyRai_Eccezioni_Ammesse.Where( x => x.cod_eccezione.Trim( ) == cod.Trim( ) ).Select( x => x.id_raggruppamento ).FirstOrDefault( );
            return gruppo == 2 || gruppo == 3;
        }
        public static string GetTurnoDescrizione ( string cod )
        {
            var db = new digiGappEntities( );
            return db.L2D_ORARIO.Where( x => x.cod_orario == cod ).Select( x => x.desc_orario ).FirstOrDefault( );
        }
        public static bool IsGiaPresente ( string codiceEccezione , string data )
        {
            if ( String.IsNullOrWhiteSpace( codiceEccezione ) )
                return false;

            WSDigigapp service = new WSDigigapp( )
            {
                Credentials = new System.Net.NetworkCredential(
                  CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[0] ,
                  CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[1] )
            };
            var respge = service.getEccezioni( CommonManager.GetCurrentUserMatricola( ) , data.Replace( "/" , "" ) , "BU" , 80 );
            bool pres = ( respge != null && respge.eccezioni != null && respge.eccezioni.Any( x => x.cod.Trim( ) == codiceEccezione ) );
            return pres;
        }

        public static EsitoInserimentoEccezione Inserimento ( InserimentoEccezioneModel model , int? idElementoBatch = null , int? idStato = null , dayResponse gio = null)
        {
            string matricola7 = null;
            string matricola = null;
            string pMatricola = null;

            DateTime dataInserimentoDB = DateTime.Now;
            DateTime dataInserimentoGAPP = DateTime.Now;

            //Cerca Extra Parametri
            string JsonExtraParams = "";

            Boolean EccezioneFittizia = false;
            String TipoEccezioneFittizia = null;
            if ( model.IsFromBatch )
            {
                matricola = model.MatricolaForzataBatch;
                matricola7 = matricola.PadLeft( 7 , '0' );
                pMatricola = "P" + matricola;
            }
            else
            {
                matricola7 = CommonHelper.GetCurrentUserMatricola7chars( );
                matricola = CommonHelper.GetCurrentUserMatricola( );
                pMatricola = CommonHelper.GetCurrentUserPMatricola( );
            }
            if (model.cod_eccezione == "SWH")
            {
                DateTime D;
                DateTime.TryParseExact(model.data_da, "dd/MM/yyyy", null, DateTimeStyles.None, out D);
                TimbratureCore.CarenzaTimbrature frammenti =
                    TimbratureCore.TimbratureManager.getCarenzaTimbrature(matricola,
                                                                        null, model.data_da.Replace("/", ""),
                                                                        Utente.SedeGapp(D), false);
                if (frammenti != null && frammenti.Intervalli != null)
                {
                    var intervalli = frammenti.Intervalli.Where(x => x.EndMinuti - x.StartMinuti >= 60).ToList();
                    if (!intervalli.Any())
                        return new EsitoInserimentoEccezione() { response = "Non sono stati trovati periodi di carenza uguali o maggiori di 60 minuti", id_richiesta_padre = 0 };

                    int minTot = intervalli.Sum(x => x.MinutiTotali);
                    if (model.quantita.ToMinutes() > minTot)
                        return new EsitoInserimentoEccezione() { response = "La quantità massima ammessa considerando i periodi maggiori di 60 minuti è di " + minTot.ToHHMM(), id_richiesta_padre = 0 };

                }
            }
            if ( !model.IsFromBatch )
            {
                var tQuery = HttpContext.Current.Request.Params.AllKeys
                            .SelectMany( HttpContext.Current.Request.Params.GetValues , ( k , v ) => new { key = k , value = v } )
                            .Where( k => k.key.ToUpper( ).StartsWith( ( "EP-" ) ) )
                            .Select( i => new { key = i.key.ToUpper( ).Replace( "EP-" , "" ) , value = i.value } );

                if ( tQuery != null && tQuery.Count( ) > 0 )
                    JsonExtraParams = new JavaScriptSerializer( ).Serialize( tQuery );

                if ( HttpContext.Current.Request["eccezione-fittizia"] != null )
                {
                    var dayresp = GetGiornata( model.data_da , CommonManager.GetCurrentUserMatricola( ) );
                    string oldOrario = dayresp.orario != null ? dayresp.orario.cod_orario : "-";
                    EccezioneFittizia = true;
                    TipoEccezioneFittizia = HttpContext.Current.Request["eccezione-fittizia"].ToString( );
                    model.nota_richiesta = "Da " + oldOrario + " a " + tQuery.First( ).value + " - " + model.nota_richiesta;
                    //"Nuovo turno: " + tQuery.First().value + " " + GetTurnoDescrizione(tQuery.First().value) + " - " + model.nota_richiesta;
                }
            }
            if (HaLimitazioneOrario(model.cod_eccezione.Trim()) && IsOraria(model.cod_eccezione.Trim()) && !String.IsNullOrWhiteSpace(model.quantita))
            {
                var dayresp = GetGiornata(model.data_da, CommonManager.GetCurrentUserMatricola());
                if (dayresp != null && dayresp.orario != null && !String.IsNullOrWhiteSpace(dayresp.orario.prevista_presenza))
                {
                    int minutiPresenza = 0;
                    if (Int32.TryParse(dayresp.orario.prevista_presenza, out minutiPresenza))
                    {
                        var minutiEcc = model.quantita.ToMinutes();
                        if (minutiEcc >= minutiPresenza)
                        {
                            return new EsitoInserimentoEccezione() { response = "Intervallo eccezione troppo esteso ", id_richiesta_padre = 0 };
                        }
                    }
                }
            }
            if ( !EccezioneFittizia && EccezioneOrariaSovrappostaOrarie( model , matricola ) )
            {
                return new EsitoInserimentoEccezione( ) { response = "Errore di sovrapposizione orari" , id_richiesta_padre = 0 };
            }

            if ( String.IsNullOrWhiteSpace( model.cod_eccezione ) )
            {
                return new EsitoInserimentoEccezione( ) { response = "Errore nel codice dell'eccezione" , id_richiesta_padre = 0 };
            }

            if ( !model.IsFromBatch && !EccezioneFittizia && TimbratureCore.TimbratureManager.DurataTroppoEstesa(
                model.cod_eccezione ,
                model.dalle ,
                model.alle ,
                model.quantita ,
                UtenteHelper.TipoDipendente( ) ,
                matricola ,
                model.data_da

                ) )
            {
                return new EsitoInserimentoEccezione( ) { response = "Durata troppo estesa per " + model.cod_eccezione , id_richiesta_padre = 0 };
            }

            if ( model.cod_eccezione != null && model.cod_eccezione.Trim( ) == "UME" )
            {
                int inizio = model.dalle.ToMinutes( );
                //if (inizio < 12 * 60 || inizio > (15 * 60) + 15)
                //{
                //    return new EsitoInserimentoEccezione() { response = "UME deve avere un orario di inizio compreso tra le 12:00 e le 15:15", id_richiesta_padre = 0 };
                //}
                if ( !( inizio >= 12 * 60 && inizio <= 15 * 60 + 15 ) && !( inizio >= 19 * 60 && inizio <= 21 * 60 ) )
                {
                    return new EsitoInserimentoEccezione( ) { response = "UME deve avere un orario di inizio compreso tra le 12:00 e le 15:15 o tra le 19:00 e 21:15" , id_richiesta_padre = 0 };
                }
            }


            int IdWorkflow = GetWorkflow( model.cod_eccezione );
            Boolean IsPQ = IsEccezioneAQuarti( model.cod_eccezione );
            Boolean Is50 = IsEccezione_0_50( model.cod_eccezione );

            //Prepara parametri base
            DateTime dataDa;
            bool d1 = DateTime.TryParseExact( model.data_da , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out dataDa );
            DateTime dataA;
            bool d2 = DateTime.TryParseExact( model.data_a , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out dataA );
            if ( d1 == false || d2 == false )
                return new EsitoInserimentoEccezione( ) { response = "Data non valida" , id_richiesta_padre = 0 };

            if ( model.cod_eccezione != null && ( model.cod_eccezione.StartsWith( "FE" ) || model.cod_eccezione.StartsWith( "RR" ) || model.cod_eccezione.StartsWith( "RF" ) ) )
            {
                bool controlloDisabilitato = CommonManager.GetParametro<string>(EnumParametriSistema.DisabilitaControlloEccezioniPianoFerie) == "TRUE";

                if (!controlloDisabilitato && !IsPFapprovato(CommonManager.GetCurrentUserMatricola()))
                {
                    var db = new digiGappEntities( );
                    int AnnoEccezione = dataDa.Year;
                    var def = db.MyRai_PianoFerieDefinizioni.Where( x => x.anno == AnnoEccezione ).FirstOrDefault( );
                    DateTime DataLimite = def.data_chiusura.AddDays(-4);

                    if ( def != null )
                    {
                        if ( ( dataDa >= DataLimite || dataA >= DataLimite ) && !model.IsFromBatch )
                        {
                            return new EsitoInserimentoEccezione( ) { response = "L'eccezione va inserita dal pannello piano ferie." , id_richiesta_padre = 0 };
                        }
                    }
                }
            }


            if ( model.cod_eccezione == "RVIV" )
                model.importo = CommonHelper.GetParametro<string>( EnumParametriSistema.ImportoRVIV );
            if ( model.cod_eccezione == "RPAF" )
                model.importo = CommonHelper.GetParametro<string>( EnumParametriSistema.ImportoRPAF );

            if ( !EccezioneFittizia && !model.IsFromBatch )
            {
                string esito = CheckGiaPresenteSuDB( dataDa , dataA , model , matricola );
                if ( esito != null )
                    return new EsitoInserimentoEccezione( ) { response = esito , id_richiesta_padre = 0 };
            }

            DayActivity dayActivity = null;

            bool IsSMAP_1Min = ( model.cod_eccezione == "SMAP" && model.quantita.ToMinutes( ) == 1 );

            if (!IsSMAP_1Min && !model.IsFromBatch && !EccezioneFittizia && Utente.GestitoSirio() && (EccezioneNecessitaAttivitaCeiton(model.cod_eccezione) || Utente.SelezioneAttivitaCeitonObbligatoria() || (CeitonManager.ActivityAvailableToday(dataDa) && IsEccezioneInGruppo2_3(model.cod_eccezione))))
            {
                var query = HttpContext.Current.Request.Params.AllKeys
                    .SelectMany( HttpContext.Current.Request.Params.GetValues , ( k , v ) => new { key = k , value = v } )
                    .Where( k => k.key.ToUpper( ).StartsWith( ( "EP-" ) ) )
                    .Select( i => new { key = i.key.ToUpper( ).Replace( "EP-" , "" ) , value = i.value } );

                if ( query != null && query.Count( ) > 0 )
                    JsonExtraParams = new JavaScriptSerializer( ).Serialize( query );

                Boolean CeitonObbligatorioPerSede = HomeManager.GetCeitonAttivitaObbligatoriaPerSede( ) && EccezioneNecessitaAttivitaCeiton( model.cod_eccezione );

                if ( String.IsNullOrWhiteSpace( model.idAttivitaCeitonDaPropAutomatiche ) )
                {
                    var AttivitaCeiton = query.Where( x => x.key == "IDATTIVITA" ).FirstOrDefault( );
                    if ( CeitonObbligatorioPerSede && ( AttivitaCeiton == null || String.IsNullOrWhiteSpace( AttivitaCeiton.value ) ) )
                        return new EsitoInserimentoEccezione( ) { response = "Attività Ceiton non indicata" , id_richiesta_padre = 0 };
                    else
                    {
                        if ( AttivitaCeiton != null && !String.IsNullOrWhiteSpace( AttivitaCeiton.value ) )
                        {
                            dayActivity = CeitonHelper.GetCeitonWeekPlan( pMatricola , matricola )
                                            .Days
                                            .SelectMany( x => x.Activities )
                                            .Where( x => x.idAttivita == AttivitaCeiton.value )
                                            .FirstOrDefault( );
                            if ( dayActivity == null )
                            {
                                return new EsitoInserimentoEccezione( ) { response = "Attività Ceiton non trovata per id:" + AttivitaCeiton.value , id_richiesta_padre = 0 };
                            }
                        }
                    }
                }
                else
                {
                    dayActivity = CeitonHelper.GetCeitonWeekPlan( pMatricola , matricola )
                                   .Days
                                   .SelectMany( x => x.Activities )
                                   .Where( x => x.idAttivita == model.idAttivitaCeitonDaPropAutomatiche )
                                   .FirstOrDefault( );
                    if ( dayActivity == null )
                    {
                        return new EsitoInserimentoEccezione( ) { response = "Attività Ceiton non trovata per id-" + model.idAttivitaCeitonDaPropAutomatiche , id_richiesta_padre = 0 };
                    }

                }
                if ( dayActivity == null && CeitonObbligatorioPerSede )
                {
                    return new EsitoInserimentoEccezione( ) { response = "Attività Ceiton non trovata" , id_richiesta_padre = 0 };
                }
            }

            string cod = model.cod_eccezione.PadRight( 4 , ' ' );
            string quantita = "1";

            string dalleHHMM = model.dalle;
            string alleHHMM = model.alle;

            //Prepara array eccezioni         
            List<Eccezione> ListEcc = new List<Eccezione>( );
            //dayResponse gio = null;
            //for ( DateTime Dcurr = dataDa ; Dcurr <= dataA ; Dcurr = Dcurr.AddDays( 1 ) )
            //{
            //    gio = new dayResponse();
            //    int min_dalle = 0;
            //    if ( !String.IsNullOrWhiteSpace( model.dalle ) )
            //        min_dalle = EccezioniManager.calcolaMinuti(model.dalle);

            //    string min_alle = "";
            //    if ( IsPQ )
            //    {
            //        int minutiQ = GetQuartoDiGiornataInMinuti( model.data_da );
            //        min_alle = ( min_dalle + minutiQ ).ToString( ).PadLeft( 4 , '0' );
            //    }
            //    else
            //    {
            //        if ( model.alle == null )
            //            min_alle = "";
            //        else
            //            min_alle = EccezioniManager.calcolaMinuti(model.alle).ToString().PadLeft(4, '0');
            //    }
            //    Eccezione e = EccezioniManager.GetEccezione( matricola7 , Dcurr.ToString( "ddMMyyyy" ) , cod , "" , "" ,
            //        model.dalle == null ? "" : min_dalle.ToString( ).PadLeft( 4 , '0' ) ,
            //        min_alle ,
            //        "" , model.nota_richiesta , "" , "" , "" , " " , " " );

            //    e.ApprovatoreSelezionato = model.MatricolaApprovatoreProduzione;                

            //    if ( !model.IsFromBatch )
            //    {
            //        gio = GetGiornata( Dcurr.ToString( "ddMMyyyy" ) , matricola );
            //    }
            //    else
            //    {
            //        gio = GetGiornataForBatch( Dcurr.ToString( "ddMMyyyy" ) , matricola );
            //    }

            //    if ( gio != null && gio.giornata != null )
            //    {
            //        e.CodiceTurno = gio.giornata.orarioReale;
            //    }

            //    ListEcc.Add( e );
            //}

            int min_dalle = 0;
            if (!String.IsNullOrWhiteSpace(model.dalle))
                min_dalle = EccezioniManager.calcolaMinuti(model.dalle);

            string min_alle = "";
            if (IsPQ)
            {
                int minutiQ = GetQuartoDiGiornataInMinuti(model.data_da);
                min_alle = (min_dalle + minutiQ).ToString().PadLeft(4, '0');
            }
            else
            {
                if (model.alle == null)
                    min_alle = "";
                else
                    min_alle = EccezioniManager.calcolaMinuti(model.alle).ToString().PadLeft(4, '0');
            }
            Eccezione eccezioneGiorno = EccezioniManager.GetEccezione(matricola7, dataDa.ToString("ddMMyyyy"), cod, "", "",
                model.dalle == null ? "" : min_dalle.ToString().PadLeft(4, '0'),
                min_alle,
                "", model.nota_richiesta, "", "", "", " ", " ");

            eccezioneGiorno.ApprovatoreSelezionato = model.MatricolaApprovatoreProduzione;

            if (!model.IsFromBatch)
            {
                gio = GetGiornata(dataDa.ToString("ddMMyyyy"), matricola);
            }
            else
            {
                if (gio == null)
                    gio = GetGiornataForBatch(dataDa.ToString("ddMMyyyy"), matricola);
            }

            if (gio != null && gio.giornata != null)
            {
                eccezioneGiorno.CodiceTurno = gio.giornata.orarioReale;
            }

            ListEcc.Add(eccezioneGiorno);

            //verifica se inserimento possibile rispetto ai massimali per giorno
            if ( dataA == dataDa )
            {
                //if ( !model.IsFromBatch )
                //{
                //    gio = GetGiornata( model.data_da , matricola );
                //}
                //else
                //{
                //    gio = GetGiornataForBatch( model.data_da , matricola );
                //}

                List<string> DaRimGap = TimbratureCore.TimbratureManager.RimuoviPerchePresentiSuGAPP( gio );
                if ( DaRimGap.Contains( model.cod_eccezione ) )
                    return new EsitoInserimentoEccezione( ) { response = "Superato massimale eccezione per giorno (Gapp)" , id_richiesta_padre = 0 };
            }

            Tuple<string , int> EsitoSalvaRichiesta;

            //Inserisce nel DB la richiesta padre ottenendo ID
            if ( !model.IsFromBatch )
            {
                EsitoSalvaRichiesta = EccezioniManager.SalvaRichiestaPreInserimento( cod ,
                                       model.nota_richiesta , dataDa , dataA , IdWorkflow , model.DaProposteAuto , model.MatricolaApprovatoreProduzione );
            }
            else
            {
                try
                {
                    EsitoSalvaRichiesta = EccezioniManager.SalvaRichiestaPreInserimentoForBatch( cod ,
                                           model.nota_richiesta , dataDa , dataA , IdWorkflow , matricola , model.Provenienza , idStato );
                }
                catch ( Exception ex )
                {
                    EsitoSalvaRichiesta = new Tuple<string , int>( ex.Message , 0 );
                    using ( digiGappEntities db = new digiGappEntities( ) )
                    {
                        MyRai_LogErrori err = new MyRai_LogErrori( );
                        err.applicativo = "InserimentoPianoFerie";
                        err.data = DateTime.Now;
                        err.error_message = ex.Message + " stackTrace: " + ex.StackTrace;
                        err.matricola = matricola;
                        err.provenienza = "EccezioniManager - SalvaRichiestaPreInserimentoForBatch ";
                        db.MyRai_LogErrori.Add( err );
                        try
                        {
                            db.SaveChanges( );
                        }
                        catch ( Exception inEx )
                        {

                        }
                    }
                }
            }

            if ( EsitoSalvaRichiesta.Item1 != null )
            {
                return new EsitoInserimentoEccezione( ) { response = EsitoSalvaRichiesta.Item1 , id_richiesta_padre = 0 };
            }

            int IdRichiestaPadre = EsitoSalvaRichiesta.Item2;
            int livUtente = 70; // in approvazione

            if ( InserimentoEccezioneGiaApprovata( cod ) || model.IsFromBatch )
                livUtente = 82; // approvata

            if ( idStato.HasValue && idStato.GetValueOrDefault( ) > 0 )
            {
                // in approvazione
                if ( idStato.GetValueOrDefault( ) == 10 )
                {
                    livUtente = 70;
                }

                // approvata
                if ( idStato.GetValueOrDefault( ) == 20 )
                {
                    livUtente = 82;
                }
            }

            if ( !String.IsNullOrEmpty( model.Provenienza ) )
            {
                List<string> splitted = model.Provenienza.Split( '-' ).ToList( );
                if ( splitted != null && splitted.Any( ) )
                {
                    if ( splitted.Contains( "InApprovazione" ) )
                    {
                        livUtente = 70;
                    }
                }
            }

            //Inserisce singola eccezione in gapp
            if ( dataA == dataDa )
            {
                decimal? QuantitaDB = null;
                if ( !String.IsNullOrWhiteSpace( quantita ) )
                    QuantitaDB = Convert.ToDecimal( quantita );
                if ( IsPQ )
                    QuantitaDB = ( decimal ) 0.25;
                if ( Is50 )
                    QuantitaDB = ( decimal ) 0.50;

                //Salva eccezioni nel DB , se errore cancella tutto
                string esitoSave = null;
                if ( ListEcc == null || ListEcc.Count == 0 )
                {
                    EccezioniManager.EliminaRichiestaPadre( IdRichiestaPadre , model.IsFromBatch );
                    return new EsitoInserimentoEccezione( ) { response = "La lista eccezioni figlie ha zero elementi" , id_richiesta_padre = 0 };
                }
                try
                {
                    if ( !model.IsFromBatch )
                    {
                        esitoSave = EccezioniManager.SalvaEccezioniArray( IdRichiestaPadre ,
                                ListEcc.ToArray( ) , model.nota_richiesta , JsonExtraParams , QuantitaDB , model , dataDa , dataA , IdWorkflow , EccezioneFittizia );
                    }
                    else
                    {
                        try
                        {
                            esitoSave = EccezioniManager.SalvaEccezioniArrayForBatch( IdRichiestaPadre ,
                                                                            ListEcc.ToArray( ) ,
                                                                            model.nota_richiesta ,
                                                                            JsonExtraParams ,
                                                                            QuantitaDB ,
                                                                            model , dataDa , dataA , IdWorkflow , matricola , idStato );
                        }
                        catch ( Exception ex )
                        {
                            using ( digiGappEntities db = new digiGappEntities( ) )
                            {
                                MyRai_LogErrori err = new MyRai_LogErrori( );
                                err.applicativo = "InserimentoPianoFerie";
                                err.data = DateTime.Now;
                                err.error_message = ex.Message + " stackTrace: " + ex.StackTrace;
                                err.matricola = matricola;
                                err.provenienza = "EccezioniManager - SalvaEccezioniArrayForBatch ";
                                db.MyRai_LogErrori.Add( err );
                                try
                                {
                                    db.SaveChanges( );
                                }
                                catch ( Exception inEx )
                                {

                                }
                            }
                            throw new Exception( ex.Message , ex );
                        }
                    }
                }
                catch ( Exception ex )
                {
                    Logger.LogErrori( new MyRai_LogErrori( )
                    {
                        applicativo = "PORTALE" ,
                        data = DateTime.Now ,
                        error_message = ex.ToString( ) ,
                        matricola = model.IsFromBatch ? matricola : CommonManager.GetCurrentUserMatricola(),
                        provenienza = "Inserimento SalvaEccezioni"
                    } );

                    EccezioniManager.EliminaRichiestaPadre( IdRichiestaPadre , model.IsFromBatch );
                    CommonManager.InviaMailDebug(" Salvaeccezioni db: " + ex.ToString(), model);
                    return new EsitoInserimentoEccezione( ) { response = ex.Message , id_richiesta_padre = 0 };
                }

                if ( esitoSave != null )
                {
                    if ( !model.IsFromBatch )
                    {
                        CommonManager.InviaMailDebug(" Salvaeccezioni db: " + esitoSave, model);
                    }
                    EccezioniManager.EliminaRichiestaPadre( IdRichiestaPadre , model.IsFromBatch );
                    return new EsitoInserimentoEccezione( ) { response = esitoSave , id_richiesta_padre = 0 };
                }

                dataInserimentoDB = DateTime.Now;

                //se selfService, scrivi nel Gapp
                if ( IdWorkflow == ( int ) EnumWorkflows.SelfService && !EccezioneFittizia )
                {
                    AggiungiEccezioneResponse aggResponse = new AggiungiEccezioneResponse( );

                    try
                    {
                        if ( !model.IsFromBatch )
                        {
                            aggResponse = EccezioniManager.aggiungiEccezione( model , livUtente );
                        }
                        else
                        {
                            try
                            {
                                aggResponse = EccezioniManager.aggiungiEccezioneForBatch( model , livUtente , matricola );
                            }
                            catch ( Exception ex )
                            {
                                using ( digiGappEntities db = new digiGappEntities( ) )
                                {
                                    MyRai_LogErrori err = new MyRai_LogErrori( );
                                    err.applicativo = "InserimentoPianoFerie";
                                    err.data = DateTime.Now;
                                    err.error_message = ex.Message + " stackTrace: " + ex.StackTrace;
                                    err.matricola = matricola;
                                    err.provenienza = "EccezioniManager - aggiungiEccezioneForBatch ";
                                    db.MyRai_LogErrori.Add( err );
                                    try
                                    {
                                        db.SaveChanges( );
                                    }
                                    catch ( Exception inEx )
                                    {

                                    }
                                }
                                throw new Exception( ex.Message , ex );
                            }
                        }
                    }
                    catch ( Exception ex )
                    {
                        EccezioniManager.EliminaRichiestaPadre( IdRichiestaPadre , model.IsFromBatch );

                        if ( !model.IsFromBatch )
                            CommonManager.InviaMailDebug(" Aggiungi eccezione gapp:" + ex.ToString(), model);

                        return new EsitoInserimentoEccezione( ) { response = ex.Message , id_richiesta_padre = 0 };
                    }

                    if ( aggResponse.Error != null )
                    {
                        EccezioniManager.EliminaRichiestaPadre( IdRichiestaPadre , model.IsFromBatch );

                        if ( !model.IsFromBatch )
                            CommonManager.InviaMailDebug(" Aggiungi eccezione gapp:" + aggResponse.Error, model);

                        return new EsitoInserimentoEccezione( ) { response = aggResponse.Error , id_richiesta_padre = 0 };
                    }
                    dataInserimentoGAPP = DateTime.Now;

                    //procedi ad aggiornare numero doc su DB per l'unica figlia
                    AggiornaNdoc( IdRichiestaPadre , aggResponse.NumDoc , model.IsFromBatch );

                    if ( model.IsFromBatch && idElementoBatch.GetValueOrDefault( ) > 0 )
                    {
                        AggiornaMyRaiPianoFerie( idElementoBatch.Value , dataInserimentoDB , dataInserimentoGAPP , IdRichiestaPadre , aggResponse.NumDoc );
                    }

                    //associa attivita ceiton se è il caso
                    if ( dayActivity != null )
                    {
                        CeitonManager.AggiungiAttivitaCeitonDB( dayActivity , IdRichiestaPadre );
                    }
                    if ( livUtente == 82 )
                    {
                        bool esito = CeitonManager.AggiornaGiornataCeiton( IdRichiestaPadre );
                    }
                }

                if ( !EccezioneFittizia && !model.IsFromBatch )
                {
                    //refresh sessione
                    Utente.GiornateAssenteIngiustificato(CommonManager.GetCurrentUserMatricola(), true);
                }

                if ( !EccezioneFittizia && !model.IsFromBatch )
                    SessionHelper.Set( "FlagEvidenze" , null );

                if ( esitoSave == null )
                {
                    if ( EccezioneFittizia )
                    {
                        if ( !model.IsFromBatch && livUtente == 70 )
                        {
                            NotificheManager.NotificaPerInserimentoRichiesta_o_Storno( IdRichiestaPadre , EnumCategoriaNotifica.InsRichiesta );
                        }
                        return new EsitoInserimentoEccezione( ) { response = "OK" , id_richiesta_padre = IdRichiestaPadre };
                    }
                    else
                    {
                        RimuoviVisionato( matricola7.Substring( 1 ) , dataDa , model.IsFromBatch );

                        if ( !model.IsFromBatch )
                        {
                            if ( livUtente == 70 )
                            {
                                NotificheManager.NotificaPerInserimentoRichiesta_o_Storno( IdRichiestaPadre , EnumCategoriaNotifica.InsRichiesta );
                            }

                            // Azzera i dati di sessione per il DataController
                            WSDigigappDataController service = new WSDigigappDataController( );
                            service.ClearAll( );
                            AggiornaDatiDiSessione( );
                        }

                        return new EsitoInserimentoEccezione( ) { response = "OK" , id_richiesta_padre = IdRichiestaPadre };
                    }

                }
                else
                    return new EsitoInserimentoEccezione( ) { response = esitoSave , id_richiesta_padre = 0 };
            }
            else //Inserisce array di eccezioni
            {
                decimal? QuantitaDB = null;
                if ( !String.IsNullOrWhiteSpace( quantita ) )
                    QuantitaDB = Convert.ToDecimal( quantita );

                //Salva eccezioni nel DB
                string esitoSave = null;

                try
                {

                    if ( !model.IsFromBatch )
                    {
                        esitoSave = EccezioniManager.SalvaEccezioniArray( IdRichiestaPadre ,
                          ListEcc.ToArray( ) , model.nota_richiesta , JsonExtraParams , QuantitaDB , model , dataDa , dataA , IdWorkflow , EccezioneFittizia );
                    }
                    else
                    {
                        esitoSave = EccezioniManager.SalvaEccezioniArrayForBatch( IdRichiestaPadre ,
                          ListEcc.ToArray( ) , model.nota_richiesta , JsonExtraParams , QuantitaDB , model , dataDa , dataA , IdWorkflow , matricola , idStato );

                    }
                }
                catch ( Exception ex )
                {
                    Logger.LogErrori( new MyRai_LogErrori( )
                    {
                        applicativo = "PORTALE" ,
                        data = DateTime.Now ,
                        error_message = ex.ToString( ) ,
                        matricola = !String.IsNullOrEmpty(matricola) ? matricola : CommonManager.GetCurrentUserMatricola(),
                        provenienza = "Inserimento SalvaEccezioni multi"
                    } );
                    EccezioniManager.EliminaRichiestaPadre( IdRichiestaPadre , model.IsFromBatch );

                    if ( !model.IsFromBatch )
                        CommonManager.InviaMailDebug(" Salvaeccezioni DB:" + ex.ToString(), model);

                    return new EsitoInserimentoEccezione( ) { response = ex.Message , id_richiesta_padre = 0 };
                }
                if ( esitoSave != null )
                {
                    if ( !model.IsFromBatch )
                        CommonManager.InviaMailDebug(" Salvaeccezioni DB: " + esitoSave, model);

                    EccezioniManager.EliminaRichiestaPadre( IdRichiestaPadre , model.IsFromBatch );
                    return new EsitoInserimentoEccezione( ) { response = esitoSave , id_richiesta_padre = 0 };
                }
                else
                {
                    if ( EccezioneFittizia )
                    {
                        if ( livUtente == 70 )
                        {
                            if ( !model.IsFromBatch )
                                NotificheManager.NotificaPerInserimentoRichiesta_o_Storno( IdRichiestaPadre , EnumCategoriaNotifica.InsRichiesta );
                        }
                        return new EsitoInserimentoEccezione( ) { response = "OK" , id_richiesta_padre = IdRichiestaPadre };
                    }
                }

                validationResponse valResponse = new validationResponse( );
                try
                {
                    valResponse = EccezioniManager.aggiungiEccezioniArray( ListEcc.ToArray( ) );
                }
                catch ( Exception ex )
                {
                    EccezioniManager.EliminaRichiestaPadre( IdRichiestaPadre , model.IsFromBatch );
                    if ( !model.IsFromBatch )
                        CommonManager.InviaMailDebug("Aggiungi eccezioni gapp:" + ex.ToString(), model);
                    return new EsitoInserimentoEccezione( ) { response = ex.Message , id_richiesta_padre = 0 };
                }
                if ( valResponse.esito != true ||
                    !String.IsNullOrWhiteSpace( valResponse.errore ) ||
                    valResponse.eccezioni.All( x => x.errore.codice != "0000" )
                    )
                {
                    EccezioniManager.EliminaRichiestaPadre( IdRichiestaPadre , model.IsFromBatch );
                    if ( !model.IsFromBatch )
                        CommonManager.InviaMailDebug("Aggiungi eccezioni gapp:" + valResponse.errore, model);
                    return new EsitoInserimentoEccezione( ) { response = valResponse.errore , id_richiesta_padre = 0 };
                }

                List<string> ErroriDate = new List<string>( );

                foreach ( Eccezione e in valResponse.eccezioni )
                {
                    ListEcc.Where( x => x.data == e.data ).First( ).ndoc = e.ndoc;
                    ListEcc.Where( x => x.data == e.data ).First( ).errore = e.errore;

                    if ( e.errore.codice != "0000" )
                        ErroriDate.Add( e.data + ":    " + e.errore.codice + "/" + e.errore.descrizione );
                    else
                    {
                        AggiornaNdocData( IdRichiestaPadre , e.ndoc , e.data );
                    }
                }

                if ( !model.IsFromBatch )
                {
                    //refresh sessione
                    UtenteHelper.GiornateAssenteIngiustificato( CommonHelper.GetCurrentUserMatricola( ) , true );
                }

                if ( !model.IsFromBatch )
                    SessionHelper.Set( "FlagEvidenze" , null );
                if ( esitoSave == null && ErroriDate.Count == 0 )
                {
                    RimuoviVisionato( matricola7.Substring( 1 ) , dataDa );

                    if ( livUtente == 70 )
                    {
                        if ( !model.IsFromBatch )
                            NotificheManager.NotificaPerInserimentoRichiesta_o_Storno( IdRichiestaPadre , EnumCategoriaNotifica.InsRichiesta );
                    }

                    if ( !model.IsFromBatch )
                    {
                        // Azzera i dati di sessione per il DataController
                        WSDigigappDataController service = new WSDigigappDataController( );
                        service.ClearAll( );
                        AggiornaDatiDiSessione( );
                    }

                    return new EsitoInserimentoEccezione( ) { response = "OK" , id_richiesta_padre = IdRichiestaPadre };
                }
                else
                {
                    if ( esitoSave != null )
                    {
                        return new EsitoInserimentoEccezione( ) { response = esitoSave , id_richiesta_padre = IdRichiestaPadre };
                    }
                    else
                    {
                        String ErroriInterni = "";
                        if ( ErroriDate.Count > 0 )
                        {
                            if ( ErroriDate.Count >= ListEcc.Count )
                            {
                                ErroriInterni = "<b>Nel periodo " + model.data_da + "-" + model.data_a + ", tutti gli inserimenti non sono andati a buon fine:</b><br /> "
                                    + String.Join( "<br />" , ErroriDate.ToArray( ) ) + "<br/>";
                            }
                            else
                            {
                                ErroriInterni = "<b>Nel periodo " + model.data_da + "-" + model.data_a + ", gli inserimenti riguardanti le date seguenti non sono andati a buon fine:</b><br /> "
                                    + String.Join( "<br />" , ErroriDate.ToArray( ) ) + "<br/><b>Le altre date sono state inserite correttamente.</b>";
                            }
                        }
                        return new EsitoInserimentoEccezione( ) { response = ErroriInterni , id_richiesta_padre = IdRichiestaPadre };
                    }
                }


            }
        }

        private static bool IsPFapprovato ( string matr )
        {
            var db = new digiGappEntities( );
            int annocorrente = DateTime.Now.Year;
            var pf = db.MyRai_PianoFerie.Where(x => x.matricola == matr && x.anno == annocorrente).FirstOrDefault();
            return ( pf != null && pf.data_approvato != null );
        }

        private static string CheckGiaPresenteSuDB ( DateTime dataDa , DateTime dataA , InserimentoEccezioneModel model , string matricola )
        {
            var db = new digiGappEntities( );
            var ecc = db.MyRai_Eccezioni_Ammesse.Where( x => x.cod_eccezione == model.cod_eccezione.Trim( ) ).FirstOrDefault( );
            if ( ecc == null )
                return "Eccezione non valida";

            var list = db.MyRai_Richieste.Where( x =>
                                     x.matricola_richiesta == matricola &&

                                     x.periodo_dal <= dataA &&
                                     dataDa <= x.periodo_al &&

                                    ( x.id_stato == 10 || x.id_stato == 20 ) &&
                                     x.MyRai_Eccezioni_Richieste.Any( a => a.cod_eccezione == model.cod_eccezione ) &&
                                     !x.MyRai_Eccezioni_Richieste.Any( z => z.azione == "C" && z.id_stato == 20 ) )
                                    .ToList( );

            if ( list.Count >= ecc.MaxPerGiorno )
                return "Richiesta in sovrapposizione";

            return null;
        }

        public static void AggiornaDatiDiSessione()
        {
            // aggiorna i dati di sessione
            WSDigigapp datiBack_ws1 = new WSDigigapp( );
            datiBack_ws1.Credentials = CommonHelper.GetUtenteServizioCredentials( );
            string dateBack = UtenteHelper.GetDateBackPerEvidenze( );

            monthResponseEccezione listaEvidenzeResponse = myRaiHelper.ServiceWrapper.ListaEvidenzWrapper( datiBack_ws1 , CommonHelper.GetCurrentUserMatricola( ) , dateBack , DateTime.Now.AddDays( -1 ).ToString( "ddMMyyyy" ) , 70 );
			ServiceWrapper.ModificaFittizio(listaEvidenzeResponse);
            SessionListaEvidenzeModel sessionModel = new SessionListaEvidenzeModel( )
            {
                UltimaScrittura = DateTime.Now ,
                ListaEvidenze = listaEvidenzeResponse
            };

            // set della sessione per impostare i dati della chiamata ListaEvidenze
            SessionHelper.Set( SessionVariables.ListaEvidenzeScrivania , sessionModel );
        }

        private static bool EccezioneOrariaSovrappostaOrarie ( InserimentoEccezioneModel model , string matricola )
        {
            if ( String.IsNullOrWhiteSpace( model.dalle ) || String.IsNullOrWhiteSpace( model.alle ) )
                return false;

            if ( model.dalle.Trim( ) == "00:00" && model.alle.Trim( ) == "00:00" )
                return false;

            if ( model.cod_eccezione == "URH" || model.cod_eccezione == "UMH" )
                return false;

            if ( !IsOraria( model.cod_eccezione ) )
                return false;
            DateTime D1model;
            bool b1 = DateTime.TryParseExact( model.data_da + " " + model.dalle , "dd/MM/yyyy HH:mm" , null , DateTimeStyles.None , out D1model );

            DateTime D2model;
            bool b2 = DateTime.TryParseExact( model.data_da + " " + model.alle , "dd/MM/yyyy HH:mm" , null , DateTimeStyles.None , out D2model );

            if ( !b1 || !b2 )
                return false;

            WSDigigapp serviceWS = new WSDigigapp( )
            {
                Credentials = CommonHelper.GetUtenteServizioCredentials( )
            };
            dayResponse resp = serviceWS.getEccezioni( matricola , model.data_da.Replace( "/" , "" ) , "BU" , 80 );

            if ( resp != null && resp.eccezioni != null )
            {
                if (TimbratureCore.TimbratureManager.IsTipoTinMNS(Utente.TipoDipendente(), resp) && model.cod_eccezione == "STRF")
                    return false;

                foreach ( var ecc in resp.eccezioni )
                {
                    if ( ecc.cod != null && ecc.cod.Trim( ) == "SMAP" )
                        continue;

                    if ( String.IsNullOrWhiteSpace( ecc.dalle ) || String.IsNullOrWhiteSpace( ecc.alle ) )
                        continue;
                    if ( !IsOraria( ecc.cod ) )
                        continue;

                    DateTime D1ecc;
                    bool e1 = DateTime.TryParseExact( ecc.data + " " + ecc.dalle.Trim( ) , "ddMMyyyy HH:mm" , null , DateTimeStyles.None , out D1ecc );

                    DateTime D2ecc;
                    bool e2 = DateTime.TryParseExact( ecc.data + " " + ecc.alle.Trim( ) , "ddMMyyyy HH:mm" , null , DateTimeStyles.None , out D2ecc );

                    if ( !e1 || !e2 )
                        continue;

                    if ( ( D1model == D1ecc && D2model == D2ecc )
                        || ( D1model > D1ecc && D1model < D2ecc )
                        || ( D2model > D1ecc && D2model < D2ecc )
                        || ( D2ecc > D1model && D2ecc < D2model )
                        || ( D1ecc > D1model && D1ecc < D2model )
                        )
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static bool HaLimitazioneOrario(string cod)
        {
            string codici = CommonManager.GetParametro<string>(EnumParametriSistema.EccezioniLimitateOrario);
            if (String.IsNullOrWhiteSpace(codici))
                return false;
            else
                return codici.Split(',').Contains(cod.Trim());
        }
        public static bool IsOraria ( string cod )
        {
            var db = new digiGappEntities( );
            var ecc = db.L2D_ECCEZIONE.Where( x => x.cod_eccezione.Trim( ) == cod.Trim( ) ).FirstOrDefault( );
            if ( ecc == null )
                return false;

            return ecc.unita_misura.Trim( ) == "H";
        }

        public static Tuple<string , int> SalvaRichiestaPreInserimento ( string cod , string nota , DateTime data_da , DateTime data_a , int idworkflow , bool DaPropAutomatiche = false , string approvatoreSelezionato = null )
        {
            var db = new digiGappEntities( );
            string tipoEccezione = CommonManager.GetTipoEccezione(cod);

            int StatoIniziale = ( int ) EnumStatiRichiesta.InApprovazione;
            if ( EccezioniManager.InserimentoEccezioneGiaApprovata( cod ) )
                StatoIniziale = ( int ) EnumStatiRichiesta.Approvata;

            MyRai_Richieste rich = new MyRai_Richieste( );
            rich.da_proposta_auto = DaPropAutomatiche;
            rich.nominativo = Utente.Nominativo();
            rich.motivo_richiesta = nota;
            rich.codice_sede_gapp = Utente.SedeGapp(data_da);
            rich.data_richiesta = DateTime.Now;
            rich.periodo_dal = data_da;
            rich.periodo_al = data_a;
            rich.ApprovatoreSelezionato = approvatoreSelezionato;
            rich.matricola_richiesta = CommonManager.GetCurrentUserMatricola();
            if ( idworkflow == ( int ) EnumWorkflows.SelfService )
                rich.id_stato = StatoIniziale;
            if ( idworkflow == ( int ) EnumWorkflows.Segreteria )
                rich.id_stato = ( int ) EnumStatiRichiesta.InseritoSegreteria;
            if ( idworkflow == ( int ) EnumWorkflows.UfficioPersonale )
                rich.id_stato = ( int ) EnumStatiRichiesta.InseritoUffPersonale;

            //rich.id_stato = idworkflow == (int)EnumWorkflows.SelfService ? StatoIniziale : (int)EnumStatiRichiesta.InseritoSegreteria;
            rich.id_tipo_richiesta = 1;
            rich.TipoQuadratura = (Utente.GetQuadratura() == Quadratura.Giornaliera ? 0 : 1);

            rich.urgente = DateTime.Now < rich.periodo_dal &&
                          rich.periodo_dal > DateTime.Now &&
                          rich.periodo_dal < DateTime.Now.AddHours( 48 );

            rich.reparto = Utente.Reparto();
            rich.gestito_sirio = Utente.GestitoSirio();
            rich.richiedente_L1 = CommonManager.L1propriaSede();
            rich.richiedente_L2 = CommonManager.L2propriaSede();
            rich.richiesta_in_resoconto = (Utente.GetQuadratura() == Quadratura.Settimanale);


            db.MyRai_Richieste.Add( rich );

            if ( DBHelper.Save( db , CommonHelper.GetCurrentUserMatricola( ) ) )
                return new Tuple<string , int>( null , rich.id_richiesta );
            else
                return new Tuple<string , int>( "Salvataggio dati non riuscito, impossibile procedere." , 0 );

        }

        public static string SalvaEccezioniArray ( int IdRichiestaPadre , Eccezione[] Eccezioni , string nota , string JSONparam ,
            decimal? quantita , InserimentoEccezioneModel model , DateTime data_da , DateTime data_a , int idworkflow , bool EccezioneFittizia )
        {
            var db = new digiGappEntities( );
            string tipoEccezione = EccezioneFittizia ? "C" : CommonManager.GetTipoEccezione(Eccezioni[0].cod);

            int StatoIniziale = ( int ) EnumStatiRichiesta.InApprovazione;
            if ( EccezioniManager.InserimentoEccezioneGiaApprovata( Eccezioni[0].cod ) )
                StatoIniziale = ( int ) EnumStatiRichiesta.Approvata;

            MyRai_Richieste rich = db.MyRai_Richieste.Where( x => x.id_richiesta == IdRichiestaPadre ).FirstOrDefault( );

            List<MyRai_Eccezioni_Richieste> Lbackup = new List<MyRai_Eccezioni_Richieste>( );
            foreach ( Eccezione ec in Eccezioni )
            {
                if ( ec.errore != null && ec.errore.codice != "0000" )
                {
                    continue;
                }
                MyRai_Eccezioni_Richieste eccez = new MyRai_Eccezioni_Richieste( );
                rich.MyRai_Eccezioni_Richieste.Add( eccez );

                eccez.numero_documento = !String.IsNullOrWhiteSpace( ec.ndoc ) ? Convert.ToInt32( ec.ndoc ) : 0;
                eccez.tipo_eccezione = tipoEccezione;
                eccez.azione = "I";
                eccez.cod_eccezione = ec.cod;

                if ( !model.IsFromBatch )
                {
                    eccez.codice_sede_gapp = Utente.SedeGapp(data_da);
                }
                else
                {

                }

                eccez.data_creazione = DateTime.Now;
                eccez.data_eccezione = CommonManager.ConvertToDate(ec.data);
                eccez.id_stato = idworkflow == ( int ) EnumWorkflows.SelfService
                                                ? StatoIniziale
                                                : ( int ) EnumStatiRichiesta.InseritoSegreteria;
                eccez.turno = ec.CodiceTurno;

                if ( EccezioniManager.InserimentoEccezioneGiaApprovata( ec.cod ) )
                {
                    eccez.data_validazione_primo_livello = DateTime.Now;
                    eccez.matricola_primo_livello = CommonManager.GetCurrentUserMatricola();
                    eccez.nominativo_primo_livello = Utente.Nominativo();
                }

                eccez.df = model.df;
                eccez.uorg = model.uorg;
                eccez.matricola_spettacolo = model.matrspett;

                eccez.ValoriParamExtraJSON = JSONparam;
                eccez.quantita = quantita;
                eccez.motivo_richiesta = nota;

                if ( !String.IsNullOrWhiteSpace( model.importo ) )
                {
                    eccez.importo = Convert.ToDecimal( model.importo.Replace( "." , "," ) );
                }
                if ( !String.IsNullOrWhiteSpace( model.quantita ) )
                {
                    eccez.quantita = Convert.ToDecimal( model.quantita.Replace( ":" , "," ) );
                }
                if ( !String.IsNullOrWhiteSpace( model.dalle ) )
                {
                    int h = Convert.ToInt32( model.dalle.Split( ':' )[0] );
                    int m = Convert.ToInt32( model.dalle.Split( ':' )[1] );
                    //  eccez.dalle = new DateTime(eccez.data_eccezione.Year, eccez.data_eccezione.Month, eccez.data_eccezione.Day, h, m, 0);
                    if ( h > 23 )
                    {
                        h = h - 24;
                        DateTime De = new DateTime( eccez.data_eccezione.Year , eccez.data_eccezione.Month , eccez.data_eccezione.Day );
                        De = De.AddDays( 1 );
                        eccez.dalle = new DateTime( De.Year , De.Month , De.Day , h , m , 0 );
                    }
                    else
                        eccez.dalle = new DateTime( eccez.data_eccezione.Year , eccez.data_eccezione.Month , eccez.data_eccezione.Day , h , m , 0 );
                }
                if ( !String.IsNullOrWhiteSpace( model.alle ) )
                {
                    int h = Convert.ToInt32( model.alle.Split( ':' )[0] );
                    int m = Convert.ToInt32( model.alle.Split( ':' )[1] );
                    if ( h > 23 )
                    {
                        h = h - 24;
                        DateTime De = new DateTime( eccez.data_eccezione.Year , eccez.data_eccezione.Month , eccez.data_eccezione.Day );
                        De = De.AddDays( 1 );
                        eccez.alle = new DateTime( De.Year , De.Month , De.Day , h , m , 0 );
                    }
                    else
                        eccez.alle = new DateTime( eccez.data_eccezione.Year , eccez.data_eccezione.Month , eccez.data_eccezione.Day , h , m , 0 );
                }
                if (IsEccezioneAQuarti(eccez.cod_eccezione))//se PRQ serve anche alle per successivo storno
                {
                    string oremin = CalcolaQuantitaOreMinuti("0", ec.alle);
                    int h = Convert.ToInt32( oremin.Split( ':' )[0] );
                    int m = Convert.ToInt32( oremin.Split( ':' )[1] );
                    if ( h > 23 )
                    {
                        h = h - 24;
                        DateTime De = new DateTime( eccez.data_eccezione.Year , eccez.data_eccezione.Month , eccez.data_eccezione.Day );
                        De = De.AddDays( 1 );
                        eccez.alle = new DateTime( De.Year , De.Month , De.Day , h , m , 0 );
                    }
                    else
                        eccez.alle = new DateTime( eccez.data_eccezione.Year , eccez.data_eccezione.Month , eccez.data_eccezione.Day , h , m , 0 );
                }

                /////////////////////////////////////////////////////// QUANTITA per H 

                var ecc = db.L2D_ECCEZIONE.Where( x => x.cod_eccezione == eccez.cod_eccezione ).FirstOrDefault( );
                if ( ecc != null && ecc.unita_misura == "H" && eccez.dalle != null && eccez.alle != null )
                {
                    TimeSpan ts = ( ( DateTime ) eccez.alle - ( DateTime ) eccez.dalle );
                    decimal qua = Convert.ToDecimal( ts.Hours + "," + ( ts.Minutes < 10 ? "0" + ts.Minutes.ToString( ) : ts.Minutes.ToString( ) ) );

                    if ( qua > 0 )
                        eccez.quantita = qua;
                }
                ///////////////////////////////////////////////////////////////////////

                Lbackup.Add( eccez );
            }
            if ( rich.MyRai_Eccezioni_Richieste.Count > 0 )
            {
                string cod_ecc = Lbackup[0].cod_eccezione;
                var ammessa = db.MyRai_Eccezioni_Ammesse.Where( x => x.cod_eccezione == cod_ecc ).FirstOrDefault( );
                if ( ammessa != null && ammessa.RichiedeDocumento == true )
                {
                    var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<List<KeyValuePair<string , string>>>( JSONparam );
                    if ( obj != null && obj.Count > 0 )
                    {
                        int idDocDipendente = Convert.ToInt32( obj[0].Value );
                        MyRai_Associazione_Richiesta_Doc ar = new MyRai_Associazione_Richiesta_Doc( );
                        ar.id_richiesta = IdRichiestaPadre;
                        ar.id_documento = idDocDipendente;
                        db.MyRai_Associazione_Richiesta_Doc.Add( ar );
                    }
                }
                if ( DBHelper.Save( db , CommonHelper.GetCurrentUserMatricola( ) ) )
                    return null;
                else
                {
                    BackupManager.SaveBackupData( IdRichiestaPadre , Lbackup );
                    return "*Errore durante il salvataggio dei dati.";
                }
            }
            else
                return null;
        }

        public static void AggiornaNdocData ( int idpadre , string ndoc , string data )
        {
            var db = new digiGappEntities( );
            try
            {
                var rich = db.MyRai_Richieste.Where( x => x.id_richiesta == idpadre ).FirstOrDefault( );
                DateTime D;
                bool datavalida = DateTime.TryParseExact( data , "ddMMyyyy" , null , DateTimeStyles.None , out D );
                int numdoc = 0;
                bool ndocvalido = int.TryParse( ndoc , out numdoc );

                if ( rich != null && datavalida && ndocvalido )
                {
                    var er = rich.MyRai_Eccezioni_Richieste.Where( x => x.data_eccezione == D ).FirstOrDefault( );
                    if ( er != null )
                    {
                        er.numero_documento = numdoc;
                        db.SaveChanges( );
                    }
                }
            }
            catch ( Exception ex )
            {

                Logger.LogErrori( new MyRai_LogErrori( )
                {
                    error_message = ex.ToString( ) ,
                    provenienza = "AggiornaNdocData"
                } );
                CommonManager.InviaMailDebug("Errore agg Ndoc per idrich:" + idpadre + " - " + ex.ToString(), null);
            }
        }

        public static void AggiornaNdoc ( int idpadre , string ndoc , bool isBatch = false )
        {
            var db = new digiGappEntities( );
            try
            {
                var rich = db.MyRai_Richieste.Where( x => x.id_richiesta == idpadre ).FirstOrDefault( );
                if ( rich != null )
                {
                    if ( rich.MyRai_Eccezioni_Richieste != null && rich.MyRai_Eccezioni_Richieste.Count > 0 )
                    {
                        int numdoc = 0;
                        int.TryParse( ndoc , out numdoc );
                        foreach ( var child in rich.MyRai_Eccezioni_Richieste )
                        {
                            child.numero_documento = numdoc;
                        }

                        if ( !isBatch )
                        {
                            DBHelper.Save( db , CommonHelper.GetCurrentUserMatricola( ) );
                        }
                        else
                        {
                            DBHelper.SaveNoSession( db , rich.matricola_richiesta );
                        }
                    }
                    try
                    {
                        DateTime dataRichiesta = rich.data_richiesta;
                        string matricola = rich.matricola_richiesta;

                        DateTime start = new DateTime( dataRichiesta.Year , dataRichiesta.Month , dataRichiesta.Day , 0 , 0 , 0 );
                        DateTime stop = new DateTime( dataRichiesta.Year , dataRichiesta.Month , dataRichiesta.Day , 23 , 59 , 59 );

                        var records = db.MyRai_Visualizzazione_Giornate_Da_Segreteria.Where( v => v.Matricola.Equals( matricola ) && v.DataRichiesta >= start && v.DataRichiesta <= stop ).ToList( );

                        if ( records != null && records.Any( ) )
                        {
                            records.ForEach( r =>
                            {
                                r.Visualizzato = false;
                            } );

                            if ( !isBatch )
                            {
                                DBHelper.Save( db , CommonHelper.GetCurrentUserMatricola( ) );
                            }
                            else
                            {
                                DBHelper.SaveNoSession( db , rich.matricola_richiesta );
                            }

                        }
                    }
                    catch ( Exception ex )
                    {
                    }

                }
            }
            catch ( Exception ex )
            {
                if ( !isBatch )
                {
                    CommonManager.InviaMailDebug("Errore agg Ndoc per idrichiesta:" + idpadre + " - " + ex.ToString(), null);

                    Logger.LogErrori( new MyRai_LogErrori( )
                    {
                        error_message = ex.ToString( ) ,
                        provenienza = "AggiornaNdoc"
                    } );
                }

            }
        }

        /// <summary>
        /// Rimuove il visionato se presente, nella giornata passata
        /// </summary>
        /// <param name="matricolaUtente"></param>
        /// <param name="dataRichiesta"></param>
        public static void RimuoviVisionato ( string matricolaUtente , DateTime dataRichiesta , bool isBatch = false )
        {
            var db = new digiGappEntities( );
            try
            {
                try
                {
                    DateTime start = new DateTime( dataRichiesta.Year , dataRichiesta.Month , dataRichiesta.Day , 0 , 0 , 0 );
                    DateTime stop = new DateTime( dataRichiesta.Year , dataRichiesta.Month , dataRichiesta.Day , 23 , 59 , 59 );

                    var records = db.MyRai_Visualizzazione_Giornate_Da_Segreteria.Where( v => v.Matricola.Equals( matricolaUtente ) && v.DataRichiesta >= start && v.DataRichiesta <= stop ).ToList( );

                    if ( records != null && records.Any( ) )
                    {
                        records.ForEach( r =>
                        {
                            r.Visualizzato = false;
                        } );

                        if ( isBatch )
                        {
                            DBHelper.SaveNoSession( db , matricolaUtente );
                        }
                        else
                        {
                            DBHelper.Save( db , CommonHelper.GetCurrentUserMatricola( ) );
                        }
                    }
                }
                catch ( Exception ex )
                {
                }

            }
            catch ( Exception ex )
            {
                CommonManager.InviaMailDebug(String.Format("Errore in RimuoviVisionato per l'utente: {0} nella data: {1}. \nErrore:{2}", matricolaUtente, dataRichiesta.ToString("dd/MM/yyyy"), ex.ToString()), null);

                Logger.LogErrori( new MyRai_LogErrori( )
                {
                    error_message = ex.ToString( ) ,
                    provenienza = "RimuoviVisionato"
                } );
            }
        }

        public static void EliminaRichiestaPadre ( int id , bool isBatch = false )
        {
            var db = new digiGappEntities( );
            try
            {
                var rich = db.MyRai_Richieste.Where( x => x.id_richiesta == id ).FirstOrDefault( );
                if ( rich != null )
                {
                    if ( rich.MyRai_Eccezioni_Richieste != null && rich.MyRai_Eccezioni_Richieste.Count > 0 )
                    {
                        List<int> lid = new List<int>( );

                        foreach ( var child in rich.MyRai_Eccezioni_Richieste )
                        {
                            lid.Add( child.id_eccezioni_richieste );
                        }
                        foreach ( int idsingolo in lid )
                        {
                            var ch = db.MyRai_Eccezioni_Richieste.Where( x => x.id_eccezioni_richieste == idsingolo ).FirstOrDefault( );
                            if ( ch != null )
                                db.MyRai_Eccezioni_Richieste.Remove( ch );
                        }
                    }

                    db.MyRai_Richieste.Remove( rich );

                    if ( !isBatch )
                    {
                        DBHelper.Save( db , CommonHelper.GetCurrentUserMatricola( ) );
                        AggiornaDatiDiSessione( );
                    }
                    else
                    {
                        DBHelper.SaveNoSession( db , rich.matricola_richiesta );
                    }
                }
            }
            catch ( Exception ex )
            {
                if ( !isBatch )
                {
                    Logger.LogErrori( new MyRai_LogErrori( )
                    {
                        error_message = ex.ToString( ) ,
                        provenienza = "EliminaRichiestaPadre"
                    } );
                }
            }
        }

        public static Timbrature GetTimbraturaPiuVicina ( dayResponse giornata , Eccezione ecc )
        {
            if ( ecc == null || String.IsNullOrWhiteSpace( ecc.alle ) )
                return null;
            Timbrature response = null;

            int minalle = calcolaMinuti(ecc.alle);
            int differenza = 10000;

            foreach ( Timbrature t in giornata.timbrature )
            {
                int min = calcolaMinuti(t.uscita.orario);
                if ( min >= minalle && min < differenza )
                {
                    differenza = min;
                    response = t;
                }
            }
            return response;
        }

        public static int calcolaMinuti ( DateTime D )
        {
            return CommonHelper.calcolaMinuti(D);
        }

        public static int calcolaMinuti ( string orarioHHMM )
        {
            return CommonHelper.calcolaMinuti(orarioHHMM);
        }
        public static Boolean IsEccezioneFittiziaNoCorrispondenzaGapp ( String cod )
        {
            var db = new digiGappEntities( );
            return db.MyRai_Eccezioni_Ammesse.Any( x => x.cod_eccezione == cod.Trim( ) && x.no_corrispondenza_gapp );
        }
        public static string decodificaStatoEccezione ( Eccezione eccezione )
        {
            string tooltipText = "";
            switch ( eccezione.stato_eccezione )
            {

                case "D":
                case " ":
                    {
                        tooltipText = "Inserito";
                        if ( eccezione.dataInserimento != "00000000" )
                            tooltipText += " il: " + DateTime.ParseExact( eccezione.dataInserimento , "ddMMyyyy" , CultureInfo.InvariantCulture ).ToString( "dd/MM/yyyy" , new CultureInfo( "IT-it" ) );//.ToShortDateString();
                        tooltipText += eccezione.stato_eccezione == "D" ? " (Da Validare)" : " (Validato)";
                    }
                    break;
                case "S":
                    {
                        tooltipText = "Stampato";
                        if ( eccezione.dataStampa != "00000000" )
                            tooltipText += " il: " + DateTime.ParseExact( eccezione.dataStampa , "ddMMyyyy" , CultureInfo.InvariantCulture ).ToString( "dd/MM/yyyy" , new CultureInfo( "IT-it" ) );//.ToShortDateString();
                    }
                    break;

                case "C":
                    {
                        tooltipText = "Convalidato";
                        if ( eccezione.dataConvalida != "00000000" )
                            tooltipText += " il: " + DateTime.ParseExact( eccezione.dataConvalida , "ddMMyyyy" , CultureInfo.InvariantCulture ).ToString( "dd/MM/yyyy" , new CultureInfo( "IT-it" ) );//.ToShortDateString();
                    }
                    break;

                case "P":
                    {
                        if ( eccezione.tipo_eccezione == "A" )
                            tooltipText = "Pagato";
                        else
                            tooltipText = "Contabilizzato";
                        if ( eccezione.bustaPaga != "      01" )
                            tooltipText += " nel mese di " + DateTime.ParseExact( eccezione.bustaPaga , "yyyyMMdd" , CultureInfo.InvariantCulture ).ToString( "MMMM yyyy" , new CultureInfo( "IT-it" ) );
                    }
                    break;

                case "R":
                    {
                        tooltipText = "RIFIUTATO";
                    }
                    break;
            }
            return tooltipText;
        }

        public static string[] GetIntervalliMensa ( DateTime date )
        {
            string sede = Utente.SedeGapp(date);
            var db = new digiGappEntities( );
            var sedeg = db.L2D_SEDE_GAPP.Where( x => x.cod_sede_gapp.Trim( ) == sede.Trim( )
                     && date >= x.data_inizio_validita
                     && date <= x.data_fine_validita
                ).FirstOrDefault( );
            if ( sedeg != null )
            {
                return new string[] { sedeg.intervallo_variabile_mensa( date ) , sedeg.intervallo_variabile_mensa_serale( date ) };
            }
            else
                return null;
        }

        public static string GetMaxDurataURH ( DateTime data )
        {
            string sede = Utente.SedeGapp(data);
            var db = new digiGappEntities( );
            var sedeg = db.L2D_SEDE_GAPP.Where( x => x.cod_sede_gapp.Trim( ) == sede.Trim( )
                 && x.data_inizio_validita <= data
                 && x.data_fine_validita >= data
                ).FirstOrDefault( );
            if ( sedeg != null && !String.IsNullOrWhiteSpace( sedeg.periodo_mensa ) )
                return sedeg.periodo_mensa;
            else
                return "0";
        }


        public static Tuple<string , int> SalvaRichiestaPreInserimentoForBatch ( string cod , string nota , DateTime data_da , DateTime data_a , int idworkflow , string matricola , string provenienza , int? idStato = null )
        {
            try
            {
                var db = new digiGappEntities( );
                string tipoEccezione = CommonManager.GetTipoEccezione(cod);
                int StatoIniziale = ( int ) EnumStatiRichiesta.Approvata;

                if ( idStato.HasValue && idStato.GetValueOrDefault( ) > 0 )
                {
                    StatoIniziale = idStato.GetValueOrDefault( );
                }

                if ( !String.IsNullOrEmpty( provenienza ) )
                {
                    List<string> splitted = provenienza.Split( '-' ).ToList( );
                    if ( splitted != null && splitted.Any( ) )
                    {
                        if ( splitted.Contains( "InApprovazione" ) )
                        {
                            StatoIniziale = ( int ) EnumStatiRichiesta.InApprovazione;
                        }
                    }
                }

                MyRaiServiceInterface.MyRaiServiceReference1.Utente utente = BatchManager.GetUserData( matricola );
                utente.matricola = matricola;

                MyRai_Richieste rich = new MyRai_Richieste( );
                rich.nominativo = utente.nominativo;
                rich.motivo_richiesta = nota;
                rich.da_pianoferie = true;

                if ( !string.IsNullOrEmpty( provenienza ) && provenienza.Equals( "Batch Inserimento SW" ) )
                {
                    rich.da_pianoferie = false;
                }

                if (!string.IsNullOrEmpty(provenienza) && provenienza.Equals("Storno SW-InApprovazione"))
                {
                    rich.da_pianoferie = false;
                }

                if (provenienza.Contains("HRIS/Congedi"))
                {
                    rich.da_pianoferie = false;
                }

                if (!string.IsNullOrEmpty(provenienza) && provenienza.Contains("Ripianificato da") && provenienza.Contains("InApprovazione") && cod.Trim().ToUpper().Equals("SW"))
                {
                    rich.da_pianoferie = false;
                }

                if (!string.IsNullOrEmpty(provenienza) && provenienza.Contains("Ripianificato da") && provenienza.Contains("InApprovazione") && !cod.Trim().ToUpper().Equals("FE"))
                {
                    rich.da_pianoferie = false;
                }

                if (!string.IsNullOrEmpty(provenienza) && provenienza.Contains("DA_PIANOFERIE=FALSE"))
                {
                    rich.da_pianoferie = false;
                }
                else if (!string.IsNullOrEmpty(provenienza) && provenienza.Contains("DA_PIANOFERIE=TRUE"))
                {
                    rich.da_pianoferie = true;
                }

                rich.codice_sede_gapp = BatchManager.SedeGapp( utente );
                rich.data_richiesta = DateTime.Now;
                rich.periodo_dal = data_da;
                rich.periodo_al = data_a;
                rich.matricola_richiesta = matricola;
                rich.id_stato = StatoIniziale;
                rich.id_tipo_richiesta = 1;

                rich.urgente = DateTime.Now < rich.periodo_dal &&
                              rich.periodo_dal > DateTime.Now &&
                              rich.periodo_dal < DateTime.Now.AddHours( 48 );

                rich.reparto = utente.CodiceReparto;

                rich.gestito_sirio = utente.gestito_SIRIO;
                string sediGestiteSirio = CommonManager.GetParametro<string>(EnumParametriSistema.SediEffettivamenteGestiteSirio);
                if ( !String.IsNullOrWhiteSpace( sediGestiteSirio ) && !String.IsNullOrWhiteSpace( rich.codice_sede_gapp ) )
                {
                    if ( sediGestiteSirio.ToUpper( ).Split( ',' ).Contains( rich.codice_sede_gapp.ToUpper( ) ) )
                    {
                        rich.gestito_sirio = true;
                    }
                }

                rich.richiedente_L1 = BatchManager.L1propriaSede( utente );
                rich.richiedente_L2 = BatchManager.L2propriaSede( utente );
                rich.richiesta_in_resoconto = ( BatchManager.GetQuadratura( utente ) == BatchManager.Quadratura.Settimanale );
                rich.TipoQuadratura = ( BatchManager.GetQuadratura( utente ) == BatchManager.Quadratura.Giornaliera ? 0 : 1 );
                db.MyRai_Richieste.Add( rich );

                if ( DBHelper.SaveNoSession( db , matricola ) )
                    return new Tuple<string , int>( null , rich.id_richiesta );
                else
                    return new Tuple<string , int>( "Salvataggio dati non riuscito, impossibile procedere." , 0 );
            }
            catch ( Exception ex )
            {
                return new Tuple<string , int>( "Salvataggio dati non riuscito, impossibile procedere.\n" + ex.Message + "\n" + ex.StackTrace + "\n" + ex.InnerException , 0 );
            }
        }


        public static string SalvaEccezioniArrayForBatch ( int IdRichiestaPadre , Eccezione[] Eccezioni , string nota , string JSONparam ,
                                                        decimal? quantita , InserimentoEccezioneModel model , DateTime data_da ,
                                                        DateTime data_a , int idworkflow , string matricola , int? idStato = null )
        {
            var db = new digiGappEntities( );
            string tipoEccezione = CommonManager.GetTipoEccezione(Eccezioni[0].cod);

            int StatoIniziale = ( int ) EnumStatiRichiesta.Approvata;

            if ( idStato.HasValue && idStato.GetValueOrDefault( ) > 0 )
            {
                StatoIniziale = idStato.GetValueOrDefault( );
            }

            if ( !String.IsNullOrEmpty( model.Provenienza ) )
            {
                List<string> splitted = model.Provenienza.Split( '-' ).ToList( );
                if ( splitted != null && splitted.Any( ) )
                {
                    if ( splitted.Contains( "InApprovazione" ) )
                    {
                        StatoIniziale = ( int ) EnumStatiRichiesta.InApprovazione;
                    }
                }
            }

            MyRai_Richieste rich = db.MyRai_Richieste.Where( x => x.id_richiesta == IdRichiestaPadre ).FirstOrDefault( );

            List<MyRai_Eccezioni_Richieste> Lbackup = new List<MyRai_Eccezioni_Richieste>( );

            MyRaiServiceInterface.MyRaiServiceReference1.Utente utente = BatchManager.GetUserData( matricola );
            utente.matricola = matricola;

            foreach ( Eccezione ec in Eccezioni )
            {
                if ( ec.errore != null && ec.errore.codice != "0000" )
                {
                    continue;
                }
                MyRai_Eccezioni_Richieste eccez = new MyRai_Eccezioni_Richieste( );
                rich.MyRai_Eccezioni_Richieste.Add( eccez );

                eccez.numero_documento = !String.IsNullOrWhiteSpace( ec.ndoc ) ? Convert.ToInt32( ec.ndoc ) : 0;
                eccez.tipo_eccezione = tipoEccezione;
                eccez.azione = "I";
                eccez.cod_eccezione = ec.cod;
                eccez.codice_sede_gapp = BatchManager.SedeGapp( utente );
                eccez.data_creazione = DateTime.Now;
                eccez.data_eccezione = CommonManager.ConvertToDate(ec.data);
                eccez.id_stato = StatoIniziale;
                eccez.turno = ec.CodiceTurno;

                string matricolaApprovatore = "";
                DateTime? dataApprovazione = null;

                // il campo provenienza va splittato per '-'
                // se viene splittato in 5 parti, allora nella parte 4
                // è contenuto l'approvatore
                // Es.
                // se la stringa fosse di questo tipo:
                // PianoFerie 2019-Ripianificato da 29/04/2019-2002428-119295-24/04/2019 10:29
                // allora il 119295 è il numero matricola dell'approvatore
                // se invece la lunghezza della lista splittata è minore di 5 allora è una stringa
                // nel vecchio formato. ES:
                // PianoFerie 2019-Ripianificato da 30/04/2019

                if (!String.IsNullOrEmpty(model.MatricolaApprovatoreCalcolata))
                {
                    if (model.DataApprovazione.HasValue)
                    {
                        dataApprovazione = model.DataApprovazione.GetValueOrDefault();
                    }
                    else
                    {
                        return "*Errore durante il calcolo della data creazione del record";
                    }

                    matricolaApprovatore = model.MatricolaApprovatoreCalcolata;
                }
                else if (!String.IsNullOrEmpty(model.Provenienza) && model.Provenienza.Contains("-MATRICOLAAPPROVATORE="))
                {
                    if (model.DataApprovazione.HasValue)
                    {
                        dataApprovazione = model.DataApprovazione.GetValueOrDefault();
                    }
                    else
                    {
                        return "*Errore durante il calcolo della data creazione del record";
                    }

                    int pos = model.Provenienza.IndexOf("-MATRICOLAAPPROVATORE=");
                    if (pos > -1)
                    {
                        string sub = model.Provenienza.Substring(pos);
                        sub = sub.Replace("-MATRICOLAAPPROVATORE=", "");
                        sub = sub.Substring(0, 6);
                        int checkMatricola = 0;
                        bool verifica = int.TryParse(sub, out checkMatricola);
                        if (!verifica)
                        {
                            return "*Errore durante il calcolo dell'approvatore";
                        }
                        matricolaApprovatore = sub;
                    }
                    else
                    {
                        return "*Errore durante il calcolo dell'approvatore";
                    }
                }
                else if ( model.Provenienza == "SW" )
                {
                    dataApprovazione = DateTime.Now;
                    matricolaApprovatore = matricola;
                }
                else
                {
                    if ( !String.IsNullOrEmpty( model.Provenienza ) )
                    {
                        List<string> splitted = model.Provenienza.Split( '-' ).ToList( );
                        if ( splitted != null && splitted.Count >= 5 )
                        {
                            try
                            {
                                if ( splitted.Contains( "InApprovazione" ) )
                                {
                                    matricolaApprovatore = "";
                                    string dateTemp = splitted[4];
                                    DateTime myDate = DateTime.ParseExact( dateTemp , "dd/MM/yyyy HH:mm" , CultureInfo.InvariantCulture );
                                    dataApprovazione = null;
                                }
                                else
                                {
                                    matricolaApprovatore = splitted[3];
                                    string dateTemp = splitted[4];
                                    DateTime myDate = DateTime.ParseExact( dateTemp , "dd/MM/yyyy HH:mm" , CultureInfo.InvariantCulture );
                                    dataApprovazione = myDate;
                                }
                            }
                            catch ( Exception ex )
                            {
                                return "*Errore durante il reperimento dei dati dell'approvatore.";
                            }
                        }
                        else
                        {
                            string rep = utente.CodiceReparto;
                            if ( !String.IsNullOrEmpty( rep ) )
                            {
                                rep = rep.Trim( );
                            }
                            else
                            {
                                rep = "";
                            }

                            if ( rep == "00" )
                            {
                                rep = "";
                            }

                            if ( String.IsNullOrEmpty( utente.sede_gapp ) )
                            {
                                utente.sede_gapp = "";
                            }

                            string searchSede = utente.sede_gapp + rep;
                            searchSede = searchSede.Trim( );

                            // se è true allora la richiesta viene approvata da UFFPERS
                            if ( model.MatricolaUFFPERS )
                            {
                                matricolaApprovatore = "UFFPERS";
                                dataApprovazione = DateTime.Now;
                            }
                            else
                            {
                                if ( splitted.Contains( "InApprovazione" ) )
                                {
                                    matricolaApprovatore = "";
                                    dataApprovazione = null;
                                }
                                else
                                {
                                    var approvatore = db.MyRai_PianoFerie.Where( w => w.matricola.Equals( matricola ) && w.anno.Equals( DateTime.Now.Year ) ).FirstOrDefault( );
                                    if ( approvatore == null )
                                    {
                                        return "*Errore durante il reperimento dei dati dell'approvatore.";
                                    }
                                    matricolaApprovatore = approvatore.approvatore;
                                    dataApprovazione = approvatore.data_approvato;
                                }
                            }
                        }
                    }
                    else
                    {
                        // se è true allora la richiesta viene approvata da UFFPERS
                        if ( model.MatricolaUFFPERS )
                        {
                            matricolaApprovatore = "UFFPERS";
                            dataApprovazione = DateTime.Now;
                        }
                        else
                        {
                            var approvatore = db.MyRai_PianoFerie.Where( w => w.matricola.Equals( matricola ) && w.anno.Equals( DateTime.Now.Year ) ).FirstOrDefault( );
                            if ( approvatore == null )
                            {
                                return "*Errore durante il reperimento dei dati dell'approvatore.";
                            }

                            matricolaApprovatore = approvatore.approvatore;
                            dataApprovazione = approvatore.data_approvato;
                        }
                    }
                }

                eccez.data_validazione_primo_livello = dataApprovazione;
                eccez.matricola_primo_livello = matricolaApprovatore;

                // se è true allora la richiesta viene approvata da UFFPERS
                if ( model.MatricolaUFFPERS )
                {
                    eccez.nominativo_primo_livello = "UFFPERS";
                }
                else
                {
                    if ( String.IsNullOrEmpty( matricolaApprovatore ) )
                    {
                        eccez.nominativo_primo_livello = null;
                    }
                    else
                    {
                        MyRaiServiceInterface.MyRaiServiceReference1.Utente utenteApprovatore = BatchManager.GetUserData( matricolaApprovatore );
                        eccez.nominativo_primo_livello = utenteApprovatore.nominativo;
                    }
                }

                eccez.df = model.df;
                eccez.uorg = model.uorg;
                eccez.matricola_spettacolo = model.matrspett;

                eccez.ValoriParamExtraJSON = JSONparam;
                eccez.quantita = quantita;
                eccez.motivo_richiesta = nota;

                if ( !String.IsNullOrWhiteSpace( model.importo ) )
                {
                    eccez.importo = Convert.ToDecimal( model.importo.Replace( "." , "," ) );
                }
                if ( !String.IsNullOrWhiteSpace( model.quantita ) )
                {
                    eccez.quantita = Convert.ToDecimal( model.quantita.Replace( ":" , "," ) );
                }
                if ( !String.IsNullOrWhiteSpace( model.dalle ) )
                {
                    int h = Convert.ToInt32( model.dalle.Split( ':' )[0] );
                    int m = Convert.ToInt32( model.dalle.Split( ':' )[1] );
                    if ( h > 23 )
                    {
                        h = h - 24;
                        DateTime De = new DateTime( eccez.data_eccezione.Year , eccez.data_eccezione.Month , eccez.data_eccezione.Day );
                        De = De.AddDays( 1 );
                        eccez.dalle = new DateTime( De.Year , De.Month , De.Day , h , m , 0 );
                    }
                    else
                    {
                        eccez.dalle = new DateTime( eccez.data_eccezione.Year , eccez.data_eccezione.Month , eccez.data_eccezione.Day , h , m , 0 );
                    }
                }
                if ( !String.IsNullOrWhiteSpace( model.alle ) )
                {
                    int h = Convert.ToInt32( model.alle.Split( ':' )[0] );
                    int m = Convert.ToInt32( model.alle.Split( ':' )[1] );
                    if ( h > 23 )
                    {
                        h = h - 24;
                        DateTime De = new DateTime( eccez.data_eccezione.Year , eccez.data_eccezione.Month , eccez.data_eccezione.Day );
                        De = De.AddDays( 1 );
                        eccez.alle = new DateTime( De.Year , De.Month , De.Day , h , m , 0 );
                    }
                    else
                        eccez.alle = new DateTime( eccez.data_eccezione.Year , eccez.data_eccezione.Month , eccez.data_eccezione.Day , h , m , 0 );
                }
                if (IsEccezioneAQuarti(eccez.cod_eccezione))//se PRQ serve anche alle per successivo storno
                {
                    string oremin = CalcolaQuantitaOreMinuti("0", ec.alle);
                    int h = Convert.ToInt32( oremin.Split( ':' )[0] );
                    int m = Convert.ToInt32( oremin.Split( ':' )[1] );
                    eccez.alle = new DateTime( eccez.data_eccezione.Year , eccez.data_eccezione.Month , eccez.data_eccezione.Day , h , m , 0 );
                }

                /////////////////////////////////////////////////////// QUANTITA per H 

                var ecc = db.L2D_ECCEZIONE.Where( x => x.cod_eccezione == eccez.cod_eccezione ).FirstOrDefault( );
                if ( ecc != null && ecc.unita_misura == "H" && eccez.dalle != null && eccez.alle != null )
                {
                    TimeSpan ts = ( ( DateTime ) eccez.alle - ( DateTime ) eccez.dalle );
                    decimal qua = Convert.ToDecimal( ts.Hours + "," + ( ts.Minutes < 10 ? "0" + ts.Minutes.ToString( ) : ts.Minutes.ToString( ) ) );

                    if ( qua > 0 )
                        eccez.quantita = qua;
                }
                ///////////////////////////////////////////////////////////////////////

                Lbackup.Add( eccez );
            }
            if ( rich.MyRai_Eccezioni_Richieste.Count > 0 )
            {
                string cod_ecc = Lbackup[0].cod_eccezione;
                var ammessa = db.MyRai_Eccezioni_Ammesse.Where( x => x.cod_eccezione == cod_ecc ).FirstOrDefault( );
                if ( ammessa != null && ammessa.RichiedeDocumento == true )
                {
                    var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<List<KeyValuePair<string , string>>>( JSONparam );
                    if ( obj != null && obj.Count > 0 )
                    {
                        int idDocDipendente = Convert.ToInt32( obj[0].Value );
                        MyRai_Associazione_Richiesta_Doc ar = new MyRai_Associazione_Richiesta_Doc( );
                        ar.id_richiesta = IdRichiestaPadre;
                        ar.id_documento = idDocDipendente;
                        db.MyRai_Associazione_Richiesta_Doc.Add( ar );
                    }
                }
                if ( DBHelper.SaveNoSession( db , matricola ) )
                    return null;
                else
                {
                    BackupManager.SaveBackupData( IdRichiestaPadre , Lbackup , matricola );
                    return "*Errore durante il salvataggio dei dati.";
                }
            }
            else
                return null;
        }

        public static AggiungiEccezioneResponse aggiungiEccezioneForBatch ( InserimentoEccezioneModel model , int livUtente , string matricola )
        {
            Boolean IsPQ = IsEccezioneAQuarti(model.cod_eccezione);
            Boolean Is50 = IsEccezione_0_50( model.cod_eccezione );

            DateTime dataDa;
            DateTime.TryParseExact( model.data_da , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out dataDa );
            DateTime dataA;
            DateTime.TryParseExact( model.data_a , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out dataA );

            string cod = model.cod_eccezione.PadRight( 4 , ' ' );

            string dalle = model.dalle == null ? "" : EccezioniManager.calcolaMinuti(model.dalle).ToString().PadLeft(4, '0');

            string alle = "";// model.alle == null ? "" : EccezioniManager.calcolaMinuti(model.alle).ToString().PadLeft(4, '0');

            if ( IsPQ )
            {
                int minutiQ = GetQuartoDiGiornataInMinuti( model.data_da );
                alle = ( Convert.ToInt32( dalle ) + minutiQ ).ToString( ).PadLeft( 4 , '0' );
            }
            else
            {
                if ( model.alle == null )
                    alle = "";
                else
                    alle = EccezioniManager.calcolaMinuti(model.alle).ToString().PadLeft(4, '0');
            }

            string quantita = "1";


            if ( !String.IsNullOrWhiteSpace( dalle ) && !String.IsNullOrWhiteSpace( alle ) )
                quantita = CalcolaQuantitaOreMinuti(dalle, alle);

            var db = new digiGappEntities( );
            var edb = db.L2D_ECCEZIONE.Where( x => x.cod_eccezione.Trim( ) == model.cod_eccezione.Trim( ) ).FirstOrDefault( );
            if ( edb != null &&
                edb.unita_misura == "G" &&
                ( ( dalle == "0000" && alle == "0000" ) || ( String.IsNullOrWhiteSpace( dalle ) && String.IsNullOrWhiteSpace( alle ) ) )
                )
            {
                quantita = "1";
            }

            if ( edb != null &&
                edb.unita_misura == "H" &&
                  ( ( dalle == "0000" && alle == "0000" ) || ( String.IsNullOrWhiteSpace( dalle ) && String.IsNullOrWhiteSpace( alle ) ) )
                )
            {
                quantita = model.quantita;
            }

            if ( IsPQ )
                quantita = "0.25";
            if ( Is50 )
                quantita = "0.50";

            //imposta quantita ore/minuti sebbene dalle/alle non valorizzate per eccezioni contenute in
            //parametro AccettaSoloDurata (SEH,POH,UMH) - altrimenti riporterebbe '1'
            string AccettanoSoloDurata = CommonManager.GetParametro<string>(EnumParametriSistema.AccettaSoloDurata);
            string DigitaSoloQuantita = CommonManager.GetParametro<string>(EnumParametriSistema.EccezioniDigitaSoloQuantita);

            if ( AccettanoSoloDurata.Split( ',' ).Contains( model.cod_eccezione )
                || DigitaSoloQuantita.Split( ',' ).Contains( model.cod_eccezione )
                || model.DontChangeQuantita )//se viene da proposte con dontchangeQUantita forza quantita del model
            {
                quantita = model.quantita;
            }

            try
            {
                WSDigigapp serviceWS = new WSDigigapp( )
                {
                    Credentials = CommonHelper.GetUtenteServizioCredentials( )
                };

                //ottieni lo status attuale della giornata
                //var resp = service.getEccezioni(matricola, dataDa.ToString("ddMMyyyy"), "BU", 70);

                WSDigigappDataController serv = new WSDigigappDataController( );

                var resp = serv.GetEccezioniForBatch( matricola , dataDa.ToString( "ddMMyyyy" ) , "BU" , 70 );

                string oldData = resp.data.Substring( 142 , 62 ) + resp.giornata.tipoDipendente;
                if ( IsPQ )
                {
                    oldData = oldData.Substring( 0 , 27 ) + resp.orario.prevista_presenza.PadLeft( 4 , '0' ) + oldData.Substring( 31 );
                }
                // se eccez di tipo C, possibile una sola al giorno
                string tipoEccezione = CommonManager.GetTipoEccezione(cod);
                if ( tipoEccezione != null && tipoEccezione.Trim( ) == "C" )
                {
                    if ( resp.eccezioni.Any( x => x.cod != null && x.cod.Trim( ).ToUpper( ) == cod.Trim( ).ToUpper( ) ) )
                    {
                        return new AggiungiEccezioneResponse( ) { Error = "Tipo di inserimento già presente in questa data" };
                    }
                }

                string importo = "";

                if ( !String.IsNullOrWhiteSpace( model.importo ) )
                    importo = model.importo.Replace( "," , "" ).Replace( "." , "" );

                updateResponse Response = ServiceWrapper.updateEccezioneForBatch( serviceWS ,
                    matricola ,
                    dataDa.ToString( "ddMMyyyy" ) ,
                    cod ,            //cod
                    oldData ,        //
                    quantita ,       //formato 01:30
                    dalle ,          //formato int minuti
                    alle ,           //formato int minuti
                    importo ,             //importo
                    ' ' ,            //storno
                    "" ,             //ndoc
                    "" ,             //note
                    "" ,             //uorg
                    "" ,             //df
                    "" ,             //ms
                    "" ,             //orario teorico
                    "" ,             //orario reale
                    livUtente );     //liv utente

                if ( Response.esito == true && ( Response.codErrore == "0000" || Response.codErrore == "TX01" ) )
                {
                    if ( Response.codErrore == "TX01" )
                    {
                        LogSuperamentoMassimale( cod , matricola , dataDa.ToString( "ddMMyyyy" ) );
                    }
                    //se ok recupera ndoc e rispondi
                    AggiungiEccezioneResponse r = new AggiungiEccezioneResponse( )
                    {
                        Error = null ,
                        response = Response
                    };

                    int[] numdocPosition = CommonManager.GetParametri<int>(EnumParametriSistema.PosizioneNumDoc);

                    if ( !String.IsNullOrWhiteSpace( Response.rawInput )
                            && Response.rawInput.Length > numdocPosition[0] + numdocPosition[1] )
                    {
                        r.NumDoc = Response.rawInput.Substring( numdocPosition[0] , numdocPosition[1] );
                    }
                    int numdoc;
                    bool b = int.TryParse( r.NumDoc , out numdoc );
                    if ( b == false || numdoc == 0 )
                    {
                        Logger.LogErrori( new MyRai_LogErrori( )
                        {
                            applicativo = "PORTALE" ,
                            data = DateTime.Now ,
                            error_message = "Ndoc non presente in risposta " + r.response.rawInput ,
                            matricola = matricola ,
                            provenienza = "EccezioniManager.aggiungiEccezione"
                        } );
                        return new AggiungiEccezioneResponse( ) { Error = "Numero Documento non restituito da Cics" };
                    }
                    else
                        return r;
                }
                else
                    return new AggiungiEccezioneResponse( ) { Error = Response.codErrore + ": " + Response.descErrore };
            }
            catch ( Exception ex )
            {
                return new AggiungiEccezioneResponse( ) { Error = ex.Message + "-" + ex.InnerException };
            }
        }



        private static void AggiornaMyRaiPianoFerie ( int idElemento , DateTime inserimentoDB , DateTime inserimentoGAPP , int idPadre , string nDoc )
        {
            try
            {
                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    var item = db.MyRai_PianoFerieBatch.Where( w => w.id.Equals( idElemento ) ).FirstOrDefault( );

                    if ( item != null )
                    {
                        item.data_inserimento_db = inserimentoDB;
                        item.data_inserimento_gapp = inserimentoGAPP;
                        item.id_richiesta_db = idPadre;
                        int doc = int.Parse( nDoc );
                        item.ndoc_gapp = doc;
                        item.data_ultimo_tentativo = DateTime.Now;

                        db.SaveChanges( );
                    }
                }
            }
            catch ( Exception ex )
            {
                try
                {
                    using ( digiGappEntities db = new digiGappEntities( ) )
                    {
                        var item = db.MyRai_PianoFerieBatch.Where( w => w.id.Equals( idElemento ) ).FirstOrDefault( );

                        if ( item != null )
                        {
                            item.error = ex.StackTrace;
                            db.SaveChanges( );
                        }
                    }
                    Logger.LogErrori(new MyRai_LogErrori()
                    {
                        error_message = ex.ToString(),
                        provenienza = "AggiornaMyRaiPianoFerie - "+ idElemento
                    });
                }
                catch ( Exception innerEx )
                {
                    Logger.LogErrori(new MyRai_LogErrori()
                    {
                        error_message = innerEx.Message,
                        provenienza = "AggiornaMyRaiPianoFerie - " + idElemento
                    });
                    throw new Exception( innerEx.Message );
                }

                Logger.LogErrori(new MyRai_LogErrori()
                {
                    error_message = ex.Message,
                    provenienza = "AggiornaMyRaiPianoFerie - " + idElemento
                });

                throw new Exception( ex.Message );
            }
        }

        public static string Valida_RifiutaForBatch ( int IdEccezioneRichiesta , bool valida ,
                                                    string NotaRifiuto_o_Approvazione = null ,
                                                    Boolean AggiornaStatoDB = true , Boolean InviaEmail = true ,
                                                    Boolean CancellazioneRichiestaDaDipendente = false , string matricola = null )
        {
            WSDigigapp datiBack = new WSDigigapp( );
            Boolean PremutoValida = valida;
            String OverrideNotifica = "";

            var db = new digiGappEntities( );
            MyRai_Eccezioni_Richieste eccez = db.MyRai_Eccezioni_Richieste.Where( x => x.id_eccezioni_richieste == IdEccezioneRichiesta ).FirstOrDefault( );
            if ( eccez == null )
                return "Errore DB, impossibile recuperare dati della richiesta";

            //se rifiutiamo uno storno, cambiamo gli stati sul DB senza comunicare niente a CICS
            if ( eccez.azione == "C" && valida == false )
            {
                var richPadre = eccez.MyRai_Richieste;
                foreach ( var e in richPadre.MyRai_Eccezioni_Richieste.Where( x => x.azione == "C" ) )
                {
                    e.id_stato = ( int ) EnumStatiRichiesta.Rifiutata;
                    e.data_rifiuto_primo_livello = DateTime.Now;
                    e.matricola_primo_livello = matricola;
                    e.nota_rifiuto_o_approvazione = NotaRifiuto_o_Approvazione;
                    e.nominativo_primo_livello = Utente.Nominativo();
                }
                if ( AggiornaStatoDB )
                {
                    if ( !DBHelper.SaveNoSession( db , matricola ) )
                    {
                        return "Errore aggiornamento DB";
                    }
                }
                richPadre.id_stato = richPadre.MyRai_Eccezioni_Richieste
                      .Where( x => x.numero_documento != null && x.numero_documento != 0 )
                    .Min( x => x.id_stato );

                if ( AggiornaStatoDB )
                {
                    if ( !DBHelper.SaveNoSession( db , matricola ) )
                    {
                        return "Errore aggiornamento DB";
                    }
                }
                Logger.LogAzione( new MyRai_LogAzioni( )
                {
                    applicativo = "Batch" ,
                    data = DateTime.Now ,
                    matricola = matricola ,
                    provenienza = "EccezioniManager.Valida_Rifiuta" ,
                    descrizione_operazione = "Rifiuto storno ideccezione_rich:" + IdEccezioneRichiesta
                } , matricola );
                NotificheManager.InserisciNotifica( "Storno rifiutato: " + eccez.cod_eccezione + " del " + eccez.data_eccezione.ToString( "dd/MM/yyyy" ) , EnumCategoriaNotifica.RifiutoEccezione.ToString( ) ,
                    eccez.MyRai_Richieste.matricola_richiesta , "Portale" , eccez.id_richiesta , matricola );

                return null;
            }

            string EventualeStornoPerDescrizioneMail = "";
            //se stiamo validando uno storno, in realta rifiutiamo la eccez a cui si riferisce
            if ( eccez.azione == "C" && valida == true )
            {
                eccez = db.MyRai_Eccezioni_Richieste.Where( x => x.numero_documento == eccez.numero_documento_riferimento
                     && x.codice_sede_gapp == eccez.codice_sede_gapp ).FirstOrDefault( );
                if ( eccez == null )
                    return "Eccezione di riferimento non trovata";
                valida = false;
                EventualeStornoPerDescrizioneMail = " (STORNO) ";
                OverrideNotifica = "Storno approvato ";
            }

            //da qui è una approvazione valida
            var RichiestaPadre = eccez.MyRai_Richieste;
            string descrizione_eccezione = "";

            List<Eccezione> Lecc = new List<Eccezione>( );
            foreach ( var ec in RichiestaPadre.MyRai_Eccezioni_Richieste
                .Where( x => ( x.id_stato == ( int ) EnumStatiRichiesta.InApprovazione || x.id_stato == ( int ) EnumStatiRichiesta.Approvata ) &&
                 x.numero_documento != null && x.numero_documento != 0 ) )
            {
                Eccezione myecc = GetItemEccezione( ec );
                descrizione_eccezione = myecc.descrittiva_lunga;
                Lecc.Add( myecc );
            }

            if ( Lecc.Count == 0 )
            {
                Logger.LogErrori( new MyRai_LogErrori( )
                {
                    error_message = "Nessuna eccezione" ,
                    provenienza = "EccezioniManager.Valida_rifiuta"
                } , matricola );
                return "Nessuna eccezione da validare";
            }

            try
            {
                //SERVIZIO//

                // se è una approvazione, invia richiesta di validazione a CICS
                if ( valida == true )
                {
                    validationResponse resp = new validationResponse( );

                    resp = ServiceWrapper.validaEccezioniForBatch( datiBack , matricola , "01" ,
                        Lecc.ToArray( ) ,
                           valida , 75 );

                    string ErroreResponse = HomeManager.ErroreInValidationResponse( resp );

                    if ( ErroreResponse == null )
                    {
                        Logger.LogAzione( new MyRai_LogAzioni( )
                        {
                            operazione = "Validazione eccezione id " + RichiestaPadre.id_richiesta ,
                            provenienza = "InserimentoPianoFerie/VerificaErrori"
                        } , matricola );

                        //Aggiorna Servizio Ceiton con giornata dipendente
                        bool ceitonUpdated = CeitonManager.AggiornaGiornataCeitonForBatch( RichiestaPadre.id_richiesta , matricola );

                        if ( !CancellazioneRichiestaDaDipendente )
                        {
                            if ( RichiestaPadre.periodo_al == RichiestaPadre.periodo_dal )
                            {
                                NotificheManager.InserisciNotifica( "Richiesta approvata: " + eccez.cod_eccezione + " del " +
                                RichiestaPadre.periodo_dal.ToString( "dd/MM/yyyy" ) , EnumCategoriaNotifica.ApprovazioneEccezione.ToString( ) ,
                                eccez.MyRai_Richieste.matricola_richiesta , "Portale" , eccez.id_richiesta , matricola );
                            }
                            else
                            {
                                NotificheManager.InserisciNotifica( "Richiesta approvata: " + eccez.cod_eccezione + " periodo " +
                                RichiestaPadre.periodo_dal.ToString( "dd/MM/yyyy" ) + " - " + RichiestaPadre.periodo_al.ToString( "dd/MM/yyyy" ) ,
                                EnumCategoriaNotifica.ApprovazioneEccezione.ToString( ) ,
                                eccez.MyRai_Richieste.matricola_richiesta , "Portale" , eccez.id_richiesta , matricola );
                            }
                        }
                    }
                    else
                    {
                        Logger.LogErrori( new MyRai_LogErrori( )
                        {
                            error_message = ErroreResponse ,
                            provenienza = "InserimentoPianoFerie/VerificaErrori"
                        } , matricola );
                        return "Errore servizio: " + ErroreResponse;
                    }
                }
                else // se è un rifiuto/storno, valida eccezione e poi storna
                {
                    validationResponse resp = ServiceWrapper.validaEccezioniForBatch( datiBack , matricola , "01" , Lecc.ToArray( ) , true , 75 );

                    string ErroreResponse = HomeManager.ErroreInValidationResponse( resp );
                    if ( ErroreResponse == null )
                    {
                        Logger.LogAzione( new MyRai_LogAzioni( )
                        {
                            operazione = "Validazione eccezione id " + RichiestaPadre.id_richiesta ,
                            provenienza = "InserimentoPianoFerie/VerificaErrori"
                        } , matricola );
                        // non si inserisce notifica perche questa validazione serve solo a cics 
                    }
                    else
                    {
                        Logger.LogErrori( new MyRai_LogErrori( )
                        {
                            error_message = ErroreResponse ,
                            provenienza = "InserimentoPianoFerie/VerificaErrori"
                        } , matricola );
                        return "Errore servizio: " + ErroreResponse;
                    }

                    // INVIA STORNO
                    validationResponse resp2 = ServiceWrapper.validaEccezioni( datiBack , matricola , "01" , Lecc.ToArray( ) , false , 90 );
                    ErroreResponse = HomeManager.ErroreInValidationResponse( resp2 );

                    if ( ErroreResponse == null )
                    {
                        bool esito = CeitonManager.AggiornaGiornataCeitonForBatch( RichiestaPadre.id_richiesta , matricola );

                        Logger.LogAzione( new MyRai_LogAzioni( )
                        {
                            operazione = "Rifiuto eccezione id " + RichiestaPadre.id_richiesta ,
                            provenienza = "InserimentoPianoFerie/VerificaErrori"
                        } , matricola );

                        if ( RichiestaPadre.periodo_al == RichiestaPadre.periodo_dal )
                        {
                            if ( String.IsNullOrWhiteSpace( OverrideNotifica ) )
                            {
                                NotificheManager.InserisciNotifica(
                            ( CancellazioneRichiestaDaDipendente == true ? "Richiesta cancellata: " : "Richiesta rifiutata: " )
                            + eccez.cod_eccezione + " del " + RichiestaPadre.periodo_dal.ToString( "dd/MM/yyyy" ) ,
                            EnumCategoriaNotifica.RifiutoEccezione.ToString( ) ,
                            eccez.MyRai_Richieste.matricola_richiesta , "Portale" , eccez.id_richiesta , matricola );
                            }
                            else
                            {
                                CommonManager.InserisciEccezioneRipianificata(IdEccezioneRichiesta, matricola);

                                NotificheManager.InserisciNotifica(
                                OverrideNotifica //storno approvato
                                + eccez.cod_eccezione + " del " + RichiestaPadre.periodo_dal.ToString( "dd/MM/yyyy" ) ,
                                EnumCategoriaNotifica.RifiutoEccezione.ToString( ) ,
                                eccez.MyRai_Richieste.matricola_richiesta , "Portale" , eccez.id_richiesta , matricola );
                            }
                        }
                        else
                        {
                            if ( String.IsNullOrWhiteSpace( OverrideNotifica ) )
                            {
                                NotificheManager.InserisciNotifica(
                              ( CancellazioneRichiestaDaDipendente == true ? "Richiesta cancellata: " : "Richiesta rifiutata: " )
                              + eccez.cod_eccezione + " periodo " + RichiestaPadre.periodo_dal.ToString( "dd/MM/yyyy" ) + " - "
                              + RichiestaPadre.periodo_al.ToString( "dd/MM/yyyy" ) ,
                              EnumCategoriaNotifica.RifiutoEccezione.ToString( ) ,
                              eccez.MyRai_Richieste.matricola_richiesta , "Portale" , eccez.id_richiesta , matricola );
                            }
                            else
                            {
                                NotificheManager.InserisciNotifica(
                                  OverrideNotifica//storno approvato
                                  + eccez.cod_eccezione + " periodo " + RichiestaPadre.periodo_dal.ToString( "dd/MM/yyyy" ) + " - "
                                  + RichiestaPadre.periodo_al.ToString( "dd/MM/yyyy" ) ,
                                  EnumCategoriaNotifica.RifiutoEccezione.ToString( ) ,
                                  eccez.MyRai_Richieste.matricola_richiesta , "Portale" , eccez.id_richiesta , matricola );
                            }
                        }
                    }
                    else
                    {
                        Logger.LogErrori( new MyRai_LogErrori( )
                        {
                            error_message = ErroreResponse ,
                            provenienza = "InserimentoPianoFerie/VerificaErrori"
                        } , matricola );
                        return "Errore servizio: " + ErroreResponse;
                    }
                }
            }
            catch ( Exception ex )
            {
                return ex.Message + "-" + ex.InnerException;
            }

            //AGGIORNAMENTO DB////////////////////////////////////
            foreach ( var ec in RichiestaPadre.MyRai_Eccezioni_Richieste.Where( x => x.id_stato == ( int ) EnumStatiRichiesta.InApprovazione ) )
            {
                if ( PremutoValida )
                {
                    ec.id_stato = ( int ) EnumStatiRichiesta.Approvata;
                    ec.data_validazione_primo_livello = DateTime.Now;
                }
                else
                {
                    ec.id_stato = ( int ) EnumStatiRichiesta.Rifiutata;
                    ec.data_rifiuto_primo_livello = DateTime.Now;
                }


                MyRaiServiceInterface.MyRaiServiceReference1.Utente utente = BatchManager.GetUserData( matricola );
                utente.matricola = matricola;


                ec.matricola_primo_livello = matricola;
                ec.nota_rifiuto_o_approvazione = NotaRifiuto_o_Approvazione;
                ec.nominativo_primo_livello = utente.nominativo;
            }

            if ( AggiornaStatoDB )
            {
                if ( !DBHelper.SaveNoSession( db , matricola ) )
                {
                    return "Errore aggiornamento DB";
                }
            }

            if ( RichiestaPadre.richiesta_in_resoconto && ( OverrideNotifica == "Storno approvato" || PremutoValida ) )
            {
                RichiestaPadre.richiesta_in_resoconto = ResocontiManager.GiaCopertaDaResoconto(
                RichiestaPadre.codice_sede_gapp ,
                RichiestaPadre.periodo_dal ,
                RichiestaPadre.periodo_al ,
                true );

                // reperimento dei reparti di cui sono responsabile per la sede esaminata
                List<string> SediReparti = RepartiManager.RepartiDiCuiSonoResponsabileForBatch( RichiestaPadre.codice_sede_gapp , matricola );

                // reperimento dell'elenco dei reparti della sede esaminata
                //	List<string> repartiSede = RepartiManager.GetRepartiAttiviPerSedeGapp( RichiestaPadre.codice_sede_gapp, RichiestaPadre.periodo_dal, RichiestaPadre.periodo_al );

                List<string> repartiMiei = SediReparti.Select( x => x.Substring( 5 ) ).ToList( );

                // TODO 
                // rimozione dei record su [MyRai_PdfReparti] se esistono record
                // per l'intervallo temporale esaminato per le sedi/reparti di mia competenza
                if ( repartiMiei != null && repartiMiei.Any( ) )
                {
                    foreach ( var rep in repartiMiei )
                    {
                        //string str1 = RichiestaPadre.periodo_dal.ToString("dd/MM/yyyy");
                        //string str2 = RichiestaPadre.periodo_al.ToString("dd/MM/yyyy");

                        DateTime da = RichiestaPadre.periodo_dal;
                        DateTime a = RichiestaPadre.periodo_al;

                        var pdfReparti = db.MyRai_PdfReparti.Where( w => w.sedegapp.Equals( RichiestaPadre.codice_sede_gapp ) && w.reparto.Equals( rep ) && w.periodo_dal == da && w.periodo_al == a ).FirstOrDefault( );

                        if ( pdfReparti != null )
                        {
                            // se esiste la riga allora la rimuove
                            db.MyRai_PdfReparti.Remove( pdfReparti );
                        }
                    }
                }
            }

            RichiestaPadre.id_stato = RichiestaPadre.MyRai_Eccezioni_Richieste
                  .Where( x => x.numero_documento != null && x.numero_documento != 0 )
                .Min( x => x.id_stato );

            if ( !String.IsNullOrWhiteSpace( OverrideNotifica ) )//se è uno storno approvato
            {
                var cop = db.MyRai_CoperturaCarenze.Where( x => x.id_richiesta == RichiestaPadre.id_richiesta ).FirstOrDefault( );
                if ( cop != null )
                    db.MyRai_CoperturaCarenze.Remove( cop );
            }
            if ( AggiornaStatoDB )
            {
                if ( !DBHelper.SaveNoSession( db , matricola ) )
                {
                    return "Errore aggiornamento DB";
                }
            }

            return null;
        }



        public static EsitoInserimentoEccezione InserimentoElementoiaPresenteGAPP ( InserimentoEccezioneModel model , int nDocumentoGAPP , int? idElementoBatch = null )
        {
            string matricola7 = null;
            string matricola = null;
            string pMatricola = null;

            DateTime dataInserimentoDB = DateTime.Now;
            DateTime dataInserimentoGAPP = DateTime.Now;

            if ( model.IsFromBatch )
            {
                matricola = model.MatricolaForzataBatch;
                matricola7 = matricola.PadLeft( 7 , '0' );
                pMatricola = "P" + matricola;
            }
            else
            {
                matricola7 = CommonHelper.GetCurrentUserMatricola7chars( );
                matricola = CommonHelper.GetCurrentUserMatricola( );
                pMatricola = CommonHelper.GetCurrentUserPMatricola( );
            }

            if ( EccezioneOrariaSovrappostaOrarie( model , matricola ) )
            {
                return new EsitoInserimentoEccezione( ) { response = "Errore di sovrapposizione orari" , id_richiesta_padre = 0 };
            }

            if ( String.IsNullOrWhiteSpace( model.cod_eccezione ) )
            {    //return "Errore nel codice dell'eccezione";
                return new EsitoInserimentoEccezione( ) { response = "Errore nel codice dell'eccezione" , id_richiesta_padre = 0 };
            }

            if ( TimbratureCore.TimbratureManager.DurataTroppoEstesa(
                model.cod_eccezione ,
                model.dalle ,
                model.alle ,
                model.quantita , "" , matricola , model.data_da ) )
            {
                return new EsitoInserimentoEccezione( ) { response = "Durata troppo estesa per " + model.cod_eccezione , id_richiesta_padre = 0 };
            }

            int IdWorkflow = GetWorkflow( model.cod_eccezione );
            Boolean IsPQ = IsEccezioneAQuarti( model.cod_eccezione );
            Boolean Is50 = IsEccezione_0_50( model.cod_eccezione );

            //Cerca Extra Parametri
            string JsonExtraParams = "";

            //Prepara parametri base
            DateTime dataDa;
            bool d1 = DateTime.TryParseExact( model.data_da , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out dataDa );
            DateTime dataA;
            bool d2 = DateTime.TryParseExact( model.data_a , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out dataA );
            if ( d1 == false || d2 == false )
                return new EsitoInserimentoEccezione( ) { response = "Data non valida" , id_richiesta_padre = 0 };

            DayActivity dayActivity = null;

            if ( !model.IsFromBatch )
            {
                var tQuery = HttpContext.Current.Request.Params.AllKeys
                            .SelectMany( HttpContext.Current.Request.Params.GetValues , ( k , v ) => new { key = k , value = v } )
                            .Where( k => k.key.ToUpper( ).StartsWith( ( "EP-" ) ) )
                            .Select( i => new { key = i.key.ToUpper( ).Replace( "EP-" , "" ) , value = i.value } );

                if ( tQuery != null && tQuery.Count( ) > 0 )
                    JsonExtraParams = new JavaScriptSerializer( ).Serialize( tQuery );
            }

            if (!model.IsFromBatch && Utente.GestitoSirio() && (EccezioneNecessitaAttivitaCeiton(model.cod_eccezione) || (CeitonManager.ActivityAvailableToday(dataDa) && IsEccezioneInGruppo2_3(model.cod_eccezione))))
            {
                var query = HttpContext.Current.Request.Params.AllKeys
                    .SelectMany( HttpContext.Current.Request.Params.GetValues , ( k , v ) => new { key = k , value = v } )
                    .Where( k => k.key.ToUpper( ).StartsWith( ( "EP-" ) ) )
                    .Select( i => new { key = i.key.ToUpper( ).Replace( "EP-" , "" ) , value = i.value } );

                if ( query != null && query.Count( ) > 0 )
                    JsonExtraParams = new JavaScriptSerializer( ).Serialize( query );

                Boolean CeitonObbligatorioPerSede = HomeManager.GetCeitonAttivitaObbligatoriaPerSede( ) && EccezioneNecessitaAttivitaCeiton( model.cod_eccezione );

                if ( String.IsNullOrWhiteSpace( model.idAttivitaCeitonDaPropAutomatiche ) )
                {
                    var AttivitaCeiton = query.Where( x => x.key == "IDATTIVITA" ).FirstOrDefault( );
                    if ( CeitonObbligatorioPerSede && ( AttivitaCeiton == null || String.IsNullOrWhiteSpace( AttivitaCeiton.value ) ) )
                        return new EsitoInserimentoEccezione( ) { response = "Attività Ceiton non indicata" , id_richiesta_padre = 0 };
                    else
                    {
                        if ( AttivitaCeiton != null && !String.IsNullOrWhiteSpace( AttivitaCeiton.value ) )
                        {
                            dayActivity = CeitonHelper.GetCeitonWeekPlan( pMatricola , matricola )
                                            .Days
                                            .SelectMany( x => x.Activities )
                                            .Where( x => x.idAttivita == AttivitaCeiton.value )
                                            .FirstOrDefault( );
                            if ( dayActivity == null )
                            {
                                return new EsitoInserimentoEccezione( ) { response = "Attività Ceiton non trovata per id:" + AttivitaCeiton.value , id_richiesta_padre = 0 };
                            }
                        }
                    }
                }
                else
                {
                    dayActivity = CeitonHelper.GetCeitonWeekPlan( pMatricola , matricola )
                                   .Days
                                   .SelectMany( x => x.Activities )
                                   .Where( x => x.idAttivita == model.idAttivitaCeitonDaPropAutomatiche )
                                   .FirstOrDefault( );
                    if ( dayActivity == null )
                    {
                        return new EsitoInserimentoEccezione( ) { response = "Attività Ceiton non trovata per id-" + model.idAttivitaCeitonDaPropAutomatiche , id_richiesta_padre = 0 };
                    }

                }
                if ( dayActivity == null && CeitonObbligatorioPerSede )
                {
                    return new EsitoInserimentoEccezione( ) { response = "Attività Ceiton non trovata" , id_richiesta_padre = 0 };
                }
            }

            string cod = model.cod_eccezione.PadRight( 4 , ' ' );
            string quantita = "1";

            string dalleHHMM = model.dalle;
            string alleHHMM = model.alle;

            //Prepara array eccezioni         
            List<Eccezione> ListEcc = new List<Eccezione>( );
            for ( DateTime Dcurr = dataDa ; Dcurr <= dataA ; Dcurr = Dcurr.AddDays( 1 ) )
            {
                int min_dalle = 0;
                if ( !String.IsNullOrWhiteSpace( model.dalle ) )
                    min_dalle = EccezioniManager.calcolaMinuti(model.dalle);

                string min_alle = "";
                if ( IsPQ )
                {
                    int minutiQ = GetQuartoDiGiornataInMinuti( model.data_da );
                    min_alle = ( min_dalle + minutiQ ).ToString( ).PadLeft( 4 , '0' );
                }
                else
                {
                    if ( model.alle == null )
                        min_alle = "";
                    else
                        min_alle = EccezioniManager.calcolaMinuti(model.alle).ToString().PadLeft(4, '0');
                }
                Eccezione e = EccezioniManager.GetEccezione( matricola7 , Dcurr.ToString( "ddMMyyyy" ) , cod , "" , "" ,
                    model.dalle == null ? "" : min_dalle.ToString( ).PadLeft( 4 , '0' ) ,
                    min_alle ,
                    "" , model.nota_richiesta , "" , "" , "" , " " , " " );

                dayResponse gio;

                if ( !model.IsFromBatch )
                {
                    gio = GetGiornata( Dcurr.ToString( "ddMMyyyy" ) , matricola );
                }
                else
                {
                    gio = GetGiornataForBatch( Dcurr.ToString( "ddMMyyyy" ) , matricola );
                }

                if ( gio != null && gio.giornata != null )
                {
                    e.CodiceTurno = gio.giornata.orarioReale;
                }

                ListEcc.Add( e );
            }

            ////verifica se inserimento possibile rispetto ai massimali per giorno
            //if ( dataA == dataDa )
            //{
            //    dayResponse gio;

            //    if ( !model.IsFromBatch )
            //    {
            //        gio = GetGiornata ( model.data_da , matricola );
            //    }
            //    else
            //    {
            //        gio = GetGiornataForBatch ( model.data_da , matricola );
            //    }

            //    List<string> DaRimGap = TimbratureCore.TimbratureManager.RimuoviPerchePresentiSuGAPP ( gio );
            //    if ( DaRimGap.Contains ( model.cod_eccezione ) )
            //        return new EsitoInserimentoEccezione ( ) { response = "Superato massimale eccezione per giorno (Gapp)" , id_richiesta_padre = 0 };
            //}

            Tuple<string , int> EsitoSalvaRichiesta;

            //Inserisce nel DB la richiesta padre ottenendo ID
            if ( !model.IsFromBatch )
            {
                EsitoSalvaRichiesta = EccezioniManager.SalvaRichiestaPreInserimento( cod ,
                                       model.nota_richiesta , dataDa , dataA , IdWorkflow );
            }
            else
            {
                EsitoSalvaRichiesta = EccezioniManager.SalvaRichiestaPreInserimentoForBatch( cod ,
                                       model.nota_richiesta , dataDa , dataA , IdWorkflow , matricola , model.Provenienza );
            }

            if ( EsitoSalvaRichiesta.Item1 != null )
            {
                return new EsitoInserimentoEccezione( ) { response = EsitoSalvaRichiesta.Item1 , id_richiesta_padre = 0 };
            }

            int IdRichiestaPadre = EsitoSalvaRichiesta.Item2;
            int livUtente = 70; // in approvazione

            if ( InserimentoEccezioneGiaApprovata( cod ) || model.IsFromBatch )
                livUtente = 82; // approvata

            if ( !String.IsNullOrEmpty( model.Provenienza ) )
            {
                List<string> splitted = model.Provenienza.Split( '-' ).ToList( );
                if ( splitted != null && splitted.Any( ) )
                {
                    if ( splitted.Contains( "InApprovazione" ) )
                    {
                        livUtente = 70;
                    }
                }
            }

            //Inserisce singola eccezione in gapp
            if ( dataA == dataDa )
            {
                decimal? QuantitaDB = null;
                if ( !String.IsNullOrWhiteSpace( quantita ) )
                    QuantitaDB = Convert.ToDecimal( quantita );
                if ( IsPQ )
                    QuantitaDB = ( decimal ) 0.25;
                if ( Is50 )
                    QuantitaDB = ( decimal ) 0.50;

                //Salva eccezioni nel DB , se errore cancella tutto
                string esitoSave = null;
                if ( ListEcc == null || ListEcc.Count == 0 )
                {
                    EccezioniManager.EliminaRichiestaPadre( IdRichiestaPadre , model.IsFromBatch );
                    return new EsitoInserimentoEccezione( ) { response = "La lista eccezioni figlie ha zero elementi" , id_richiesta_padre = 0 };
                }
                try
                {
                    if ( !model.IsFromBatch )
                    {
                        esitoSave = EccezioniManager.SalvaEccezioniArray( IdRichiestaPadre ,
                                ListEcc.ToArray( ) , model.nota_richiesta , JsonExtraParams , QuantitaDB , model , dataDa , dataA , IdWorkflow , false );
                    }
                    else
                    {
                        esitoSave = EccezioniManager.SalvaEccezioniArrayForBatch( IdRichiestaPadre ,
                                                                        ListEcc.ToArray( ) ,
                                                                        model.nota_richiesta ,
                                                                        JsonExtraParams ,
                                                                        QuantitaDB ,
                                                                        model , dataDa , dataA , IdWorkflow , matricola );
                    }
                }
                catch ( Exception ex )
                {
                    Logger.LogErrori( new MyRai_LogErrori( )
                    {
                        applicativo = "PORTALE" ,
                        data = DateTime.Now ,
                        error_message = ex.ToString( ) ,
                        matricola = model.IsFromBatch ? matricola : CommonManager.GetCurrentUserMatricola(),
                        provenienza = "Inserimento SalvaEccezioni"
                    } );

                    EccezioniManager.EliminaRichiestaPadre( IdRichiestaPadre , model.IsFromBatch );
                    CommonManager.InviaMailDebug(" Salvaeccezioni db: " + ex.ToString(), model);
                    return new EsitoInserimentoEccezione( ) { response = ex.Message , id_richiesta_padre = 0 };
                }

                if ( esitoSave != null )
                {
                    if ( !model.IsFromBatch )
                    {
                        CommonManager.InviaMailDebug(" Salvaeccezioni db: " + esitoSave, model);
                    }
                    EccezioniManager.EliminaRichiestaPadre( IdRichiestaPadre , model.IsFromBatch );
                    return new EsitoInserimentoEccezione( ) { response = esitoSave , id_richiesta_padre = 0 };
                }

                dataInserimentoDB = DateTime.Now;

                // poichè già esiste su GAPP
                // va solo aggiornato il numero documento

                //procedi ad aggiornare numero doc su DB per l'unica figlia
                AggiornaNdoc( IdRichiestaPadre , nDocumentoGAPP.ToString( ) , model.IsFromBatch );

                if ( model.IsFromBatch && idElementoBatch.GetValueOrDefault( ) > 0 )
                {
                    AggiornaMyRaiPianoFerie( idElementoBatch.Value , dataInserimentoDB , dataInserimentoGAPP , IdRichiestaPadre , nDocumentoGAPP.ToString( ) );
                }

                //associa attivita ceiton se è il caso
                if ( dayActivity != null )
                {
                    CeitonManager.AggiungiAttivitaCeitonDB( dayActivity , IdRichiestaPadre );
                }
                if ( livUtente == 82 )
                {
                    bool esito = CeitonManager.AggiornaGiornataCeiton( IdRichiestaPadre );
                }

                if ( !model.IsFromBatch )
                {
                    //refresh sessione
                    Utente.GiornateAssenteIngiustificato(CommonManager.GetCurrentUserMatricola(), true);
                }

                if ( !model.IsFromBatch )
                    SessionHelper.Set( "FlagEvidenze" , null );

                if ( esitoSave == null )
                {
                    RimuoviVisionato( matricola7.Substring( 1 ) , dataDa , model.IsFromBatch );

                    if ( !model.IsFromBatch )
                    {
                        if ( livUtente == 70 )
                        {
                            NotificheManager.NotificaPerInserimentoRichiesta_o_Storno( IdRichiestaPadre , EnumCategoriaNotifica.InsRichiesta );
                        }

                        // Azzera i dati di sessione per il DataController
                        WSDigigappDataController service = new WSDigigappDataController( );
                        service.ClearAll( );
                    }

                    return new EsitoInserimentoEccezione( ) { response = "OK" , id_richiesta_padre = IdRichiestaPadre };
                }
                else
                    return new EsitoInserimentoEccezione( ) { response = esitoSave , id_richiesta_padre = 0 };
            }
            else //Inserisce array di eccezioni
            {

                return new EsitoInserimentoEccezione( ) { response = "KO - Non gestito" , id_richiesta_padre = 0 };
            }
        }

    }
}
