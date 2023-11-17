using System;
using System.Collections.Generic;
using System.Linq;

using myRai.Models;
using myRaiCommonManager;
using myRaiData;
using myRaiHelper;
using MyRaiServiceInterface.it.rai.servizi;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using MyRaiServiceReference1 = MyRaiServiceInterface.MyRaiServiceReference1;

namespace myRai.Business
{
    public class FiltroTest
    {
        public List<string> EccezioniDaRimuovere(dayResponse R, List<string> Eccezioni)
        {
            if (Eccezioni == null || Eccezioni.Count == 0) return null;
            List<string> Rimuovi = new List<string>( );

            foreach ( var ecc in Eccezioni )
            {
                if ( ecc == "AR20" )
                {
                    List<DateTime> usufruite = new List<DateTime>( );

                    List<DateTime> LD = new List<DateTime>( );
                    LD.Add( new DateTime( 2019 , 08 , 16 ) );
                    LD.Add( new DateTime( 2019 , 12 , 24 ) );
                    LD.Add( new DateTime( 2019 , 12 , 31 ) );

                    var db = new myRaiData.digiGappEntities( );
                    string sede = Utente.SedeGapp(DateTime.Now);
                    DateTime D = DateTime.Now;

                    int anno = DateTime.Now.Year;

                    if (
                        Utente.TipoDipendente() == "G"
                         &&
                         LD.Contains( R.giornata.data )
                         && !Utente.GiornalistaDelleReti()
                         && R.giornata.orarioReale != "90" && R.giornata.orarioReale != "95" && R.giornata.orarioReale != "96"
                         &&
                             ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) || x.cod.Trim( ).StartsWith( "IS" ) )
                               ||
                             ( R.timbrature != null && R.timbrature.Length > 0 ) )
                        )
                    {
                        DateTime fineanno = new DateTime( DateTime.Now.Year , 12 , 31 , 0 , 0 , 0 );

                        if ( R.giornata.data.Date.Equals( fineanno ) )
                        {
                            MyRaiServiceReference1.MyRaiService1Client wcf1 = new MyRaiServiceReference1.MyRaiService1Client( );
                            wcf1.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

                            var response = wcf1.GetAnalisiEccezioni(CommonManager.GetCurrentUserMatricola(),
                                                                        new DateTime( ( int ) anno , 8 , 16 ) ,
                                                                        new DateTime( ( int ) anno , 12 , 31 ) ,
                                                                        "AR20" ,
                                                                        null ,
                                                                        null
                                                                        );

                            if ( response != null && response.DettagliEccezioni != null )
                            {
                                foreach ( var d in response.DettagliEccezioni )
                                {
                                    d.data = new DateTime( ( int ) anno , d.data.Month , d.data.Day );
                                }

                                usufruite.AddRange( response.DettagliEccezioni.Where( x => x.eccezione == "AR20" ).Select( x => x.data ).ToList( ) );
                            }
                            else
                            {
                                usufruite = new List<DateTime>( );
                            }

                            if ( usufruite.Count( ) >= 2 )
                            {
                                Rimuovi.Add( "AR20" );
                            }
                        }
                    }
                    else
                        Rimuovi.Add( "AR20" );
                }
                if ( ecc == "AT30" )
                {
                    if (
                          R.giornata != null
                          && R.eccezioni != null
                          &&
                             ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) )
                               ||
                             ( R.timbrature != null && R.timbrature.Length > 0 ) )
                          )
                    {
                        WSDigigapp service = new WSDigigapp( ) { Credentials = System.Net.CredentialCache.DefaultCredentials };
                        var resp = service.getEccezioni(Utente.Matricola(), R.giornata.data.AddDays(-1).ToString("ddMMyyyy"), "BU", 70);
                        if ( resp.orario != null && resp.orario.cod_orario != null && resp.orario.cod_orario == "96" )
                        {
                            resp = service.getEccezioni(Utente.Matricola(), R.giornata.data.AddDays(-2).ToString("ddMMyyyy"), "BU", 70);
                        }
                        int MinutiIeri = 1440 - Convert.ToInt32(resp.orario.uscita_iniziale);
                        if ((resp.timbrature == null || resp.timbrature.Count() == 0) && !resp.eccezioni.Any(x => x.cod.Trim() == "FS" || x.cod.Trim() == "SE"))
                        {
                            Rimuovi.Add("AT30");
                        }
                        else
                        {
                            if (resp.timbrature != null && resp.timbrature.Any() && resp.timbrature.Last().uscita != null && !String.IsNullOrWhiteSpace(resp.timbrature.Last().uscita.orario))
                            {
                                MinutiIeri = 1440 - resp.timbrature.Last().uscita.orario.ToMinutes();
                        }
                        }

                        //throw new Exception(resp.orario.uscita_iniziale);
                        int MinutiOggi = Convert.ToInt32( R.orario.entrata_iniziale );
                        if ( MinutiIeri + MinutiOggi > 660 )
                            Rimuovi.Add( "AT30" );
                    }
                    else
                        Rimuovi.Add( "AT30" );

                }
                if ( ecc == "CTA" )
                {
                    if ( myRai.Business.CommonManager.GetCurrentUserMatricola( ) == "103650" )
                    {
                        //ti spetta
                    }
                    else
                        Rimuovi.Add( "CTA" );
                }
                if ( ecc == "CTD" )
                {
                    if (
                        myRai.Business.CommonManager.HasInfoDipendente(9, true) &&
                        (R.eccezioni == null || !R.eccezioni.Any() || (R.eccezioni.Count() == 1 && R.eccezioni.Any(z => z.cod.Trim() == "CAR" || z.cod.Trim() == "FS"))) &&
                        ("8VD10,8VD50,8VD00,8VD20,8VD30,8VD40,8VD60,8VD70,8VD80,8VDA0,8VE40,8VE50,8VE90,8VD01,8VD0A,9LD10,9LDD0,9LDH0,9LDK0,9FE20,9FE30,9FEA0,9FEA1,9FEA2,9FEA3,9FEA4,9FEA9,9FEB0,9FEB1,9FEB2,9FEB3,9FEC0,9FEE0,9FEH0,9FEF0,9FI00,9FI10,9FI20,9FI30,9FI40,9FI50,9FI60,9FI70,9FI80,9FL00,9FL40,9FL50,9FL60,96L70,9FL80,9FL90".Split(',').Contains(Utente.SedeGapp()) || myRai.Business.CommonManager.GetCurrentUserMatricola() == "103650")
                        )
                    {
                        //ti spetta
                    }
                    else
                        Rimuovi.Add( "CTD" );
                }
                if ( ecc == "DG55" )
                {
                    List<DateTime> LD = new List<DateTime>();
                    LD.Add(new DateTime(2019, 11, 3));
                    if (Utente.TipoDipendente() == "G"
                        && !Utente.GiornalistaDelleReti()

                        && R.giornata != null
                        && R.eccezioni != null
                        && R.giornata.tipoGiornata != "P"
                        //&& R.giornata.tipoGiornata != "R"
                        //&& R.giornata.tipoGiornata != "S"
                        && R.giornata.siglaGiornata == "DO"
                        && !R.eccezioni.Any( m => m.cod == "LF36" )
                        && R.giornata.orarioReale != "90" && R.giornata.orarioReale != "95" && R.giornata.orarioReale != "96"
                        &&
    ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) || x.cod.Trim( ).StartsWith( "IS" ) )
                             ||
                           ( R.timbrature != null && R.timbrature.Length > 0 ) )
                        )
                    {
                        //bravo ti spetta
                    }
                    else Rimuovi.Add("DG55");
                }
                if ( ecc == "DH40" )
                {

                    if (myRai.Business.CommonManager.HasInfoDipendente(9, true)
                        && R.giornata != null
                        && R.eccezioni != null
                        && R.giornata.siglaGiornata == "DO"
                        && R.giornata.tipoGiornata != "P"
                        && R.giornata.tipoGiornata != "R"
                        && R.giornata.tipoGiornata != "S"
                        && ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) || x.cod.Trim( ).StartsWith( "IS" ) )
                            ||
                            ( R.timbrature != null && R.timbrature.Length > 0 ) )
                    )
                    {
                        //bravo ti spetta
                        if ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "SRH" ) ) )
                        {
                            Rimuovi.Add( "DH40" );
                        }

                    }
                    else
                        Rimuovi.Add( "DH40" );
                }
                if ( ecc == "DH60" )
                {

                    if (R.giornata != null
                        && R.eccezioni != null
                        && R.giornata.siglaGiornata == "DO"
                        && R.giornata.tipoGiornata != "P"
                        && R.giornata.tipoGiornata != "R"
                        && R.giornata.tipoGiornata != "S"
                        &&
                        ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) || x.cod.Trim( ).StartsWith( "IS" ) )
                            ||
                            ( R.timbrature != null && R.timbrature.Length > 0 ) )
                    )
                    {
                        //bravo ti spetta
                        if ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "SRH" ) ) )
                        {
                            Rimuovi.Add( "DH60" );
                        }
                    }
                    else
                        Rimuovi.Add( "DH60" );
                }

                if ( ecc == "FEM" )
                {
                    if (R.eccezioni.Any(x => x.cod.Trim() == "SNP") || Utente.FormaContratto() == "8" || CommonManager.IsDateInControlloSNM_SMP(R.giornata.data))
                    {
                        //bravo ti spetta
                    }
                    else Rimuovi.Add("FEM");
                }
                if ( ecc == "FEP" )
                {
                    if (R.eccezioni.Any(x => x.cod.Trim() == "SNM") || Utente.FormaContratto() == "8" || CommonManager.IsDateInControlloSNM_SMP(R.giornata.data))
                    {
                        //bravo ti spetta
                    }
                    else Rimuovi.Add("FEP");
                }
                if ( ecc == "FFH" )
                {

                    if ( R.eccezioni.Any( x => ( x.cod.Trim( ) == "FE" || x.cod.Trim( ) == "FO" || x.cod.Trim( ) == "SE" ) || x.cod.Trim( ) == "FS" || x.cod.Trim( ) == "FF" ) )
                    {

                        //bravo ti spetta
                        Rimuovi.Add( "FFH" );
                    }

                }
                if ( ecc == "FFM" )
                {

                    if ( R.eccezioni.Any( x => x.cod.Trim( ) == "CAR" ) == false )
                        Rimuovi.Add( "FFM" );
                }


                if ( ecc == "FFP" )
                {

                    if ( R.eccezioni.Any( x => x.cod.Trim( ) == "CAR" ) == false )
                        Rimuovi.Add( "FFP" );
                }
                if ( ecc == "FMH" )
                {

                    if (
                          ( !R.giornata.orarioReale.StartsWith( "9" ) && R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "MF" ) ) )
                          ||
                           ( !R.giornata.orarioReale.StartsWith( "9" ) && R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "MN" ) ) )
                          ||
                           ( !R.giornata.orarioReale.StartsWith( "9" ) && R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "MR" ) ) )
                          )
                    {
                        //ti spetta
                    }
                    else
                        Rimuovi.Add( "FMH" );

                }

                if ( ecc == "FO" )
                {

                    if ( R.eccezioni.Any( x => ( x.cod.StartsWith( "FE" ) || x.cod.StartsWith( "FS" ) || x.cod.StartsWith( "SE" ) ) ) )
                    {

                        //bravo ti spetta
                        Rimuovi.Add( "FO" );
                    }

                }
                if ( ecc == "FOH" )
                {

                    if ( R.eccezioni.Any( x => ( x.cod.Trim( ) == "FE" || x.cod.Trim( ) == "FS" || x.cod.Trim( ) == "SE" ) ) )
                    {

                        //bravo ti spetta
                        Rimuovi.Add( "FOH" );
                    }

                }
                if ( ecc == "FOM" )
                {

                    if ( R.eccezioni.Any( x => ( x.cod.Trim( ) == "FEM" || x.cod.Trim( ) == "FE" ) ) )
                    {

                        //bravo ti spetta
                        Rimuovi.Add( "FOM" );
                    }
                    if ( R.eccezioni.Any( x => ( x.cod.Trim( ) == "FSM" || x.cod.Trim( ) == "FS" ) ) )
                    {

                        //bravo ti spetta
                        Rimuovi.Add( "FOM" );
                    }
                    if ( R.eccezioni.Any( x => ( x.cod.Trim( ) == "SEM" || x.cod.Trim( ) == "SE" ) ) )
                    {

                        //bravo ti spetta
                        Rimuovi.Add( "FOM" );
                    }
                    if ( R.eccezioni.Any( x => x.cod.Trim( ) == "FO" ) )
                    {

                        //bravo ti spetta
                        Rimuovi.Add( "FOM" );
                    }
                }
                if ( ecc == "FOP" )
                {

                    if ( R.eccezioni.Any( x => ( x.cod.Trim( ) == "FEP" || x.cod.Trim( ) == "FE" ) ) )
                    {

                        //bravo ti spetta
                        Rimuovi.Add( "FOP" );
                    }
                    if ( R.eccezioni.Any( x => ( x.cod.Trim( ) == "FSP" || x.cod.Trim( ) == "FS" ) ) )
                    {

                        //bravo ti spetta
                        Rimuovi.Add( "FOP" );
                    }
                    if ( R.eccezioni.Any( x => ( x.cod.Trim( ) == "SEP" || x.cod.Trim( ) == "SE" ) ) )
                    {

                        //bravo ti spetta
                        Rimuovi.Add( "FOP" );
                    }
                    if ( R.eccezioni.Any( x => x.cod.Trim( ) == "FO" ) )
                    {

                        //bravo ti spetta
                        Rimuovi.Add( "FOP" );
                    }
                }
                if (ecc == "FPH")
                {
                    string sedec = myRai.Models.Utente.EsponiAnagrafica().SedeContabile;

                    if (sedec != null && "750,410,540,680,770".Split(',').Contains(sedec) &&
                        myRai.Business.CommonManager.HasInfoDipendente(9, true) &&
                        R.giornata != null
                        && R.giornata.orarioReale != "90" && R.giornata.orarioReale != "95" && R.giornata.orarioReale != "96"
                        && ((R.eccezioni != null && R.eccezioni.Any(x => x.cod.Trim().StartsWith("FS")) || (R.timbrature != null && R.timbrature.Length > 0))))
                    {
                        MyRaiServiceReference1.MyRaiService1Client wcf1 = new MyRaiServiceReference1.MyRaiService1Client();
                        wcf1.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

                        var response = wcf1.getOrario(R.giornata.orarioReale, R.data.Replace("/", ""), CommonManager.GetCurrentUserMatricola(), "BU", 75);
                        if (response != null
                            && !String.IsNullOrWhiteSpace(response.OrarioUscitaFinale)
                            && EccezioniManager.calcolaMinuti(response.OrarioUscitaFinale) < 21 * 60)
                        {
                            //ok ti spetta
                        }
                        else
                            Rimuovi.Add("FPH");
                    }
                    else
                    {
                        Rimuovi.Add("FPH");
                    }
                }
                if ( ecc == "FSH" )
                {

                    if ( R.eccezioni.Any( x => ( x.cod.Trim( ) == "FE" || x.cod.Trim( ) == "FO" || x.cod.Trim( ) == "SE" ) || x.cod.Trim( ) == "FS" ) )
                    {

                        //bravo ti spetta
                        Rimuovi.Add( "FSH" );
                    }

                }
                if ( ecc == "FSM" )
                {

                    if ( R.eccezioni.Any( x => ( x.cod.Trim( ) == "FEM" || x.cod.Trim( ) == "FE" ) ) )
                    {

                        //bravo ti spetta
                        Rimuovi.Add( "FSM" );
                    }
                    if ( R.eccezioni.Any( x => ( x.cod.Trim( ) == "FOM" || x.cod.Trim( ) == "FO" ) ) )
                    {

                        //bravo ti spetta
                        Rimuovi.Add( "FSM" );
                    }
                    if ( R.eccezioni.Any( x => ( x.cod.Trim( ) == "SEM" || x.cod.Trim( ) == "SE" ) ) )
                    {

                        //bravo ti spetta
                        Rimuovi.Add( "FSM" );
                    }
                    if ( R.eccezioni.Any( x => x.cod.Trim( ) == "FS" ) )
                    {

                        //bravo ti spetta
                        Rimuovi.Add( "FSM" );
                    }
                }
                if ( ecc == "FSP" )
                {

                    if ( R.eccezioni.Any( x => ( x.cod.Trim( ) == "FEP" || x.cod.Trim( ) == "FE" ) ) )
                    {

                        //bravo ti spetta
                        Rimuovi.Add( "FSP" );
                    }
                    if ( R.eccezioni.Any( x => ( x.cod.Trim( ) == "FOP" || x.cod.Trim( ) == "FO" ) ) )
                    {

                        //bravo ti spetta
                        Rimuovi.Add( "FSP" );
                    }
                    if ( R.eccezioni.Any( x => ( x.cod.Trim( ) == "SEP" || x.cod.Trim( ) == "SE" ) ) )
                    {

                        //bravo ti spetta
                        Rimuovi.Add( "FSP" );
                    }
                    if ( R.eccezioni.Any( x => x.cod.Trim( ) == "FS" ) )
                    {

                        //bravo ti spetta
                        Rimuovi.Add( "FSP" );
                    }
                }
                if ( ecc == "GAPC" )
                {


                    if (
                                         R.giornata != null
                                           && R.eccezioni != null
                                           &&
                                              ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) )
                                                )
                                           )
                    {
                        bool showVoto = false;
                        string matricola = myRai.Business.CommonManager.GetCurrentUserMatricola( );

                        if (myRai.Business.CommonManager.HasInfoDipendente(10))
                        {
                            var infoDipendente = myRai.Business.CommonManager.GetInfoDipendente( 10 );

                            if ( infoDipendente != null )
                            {
                                DateTime min = infoDipendente.data_inizio;
                                DateTime max = infoDipendente.data_fine.HasValue ? infoDipendente.data_fine.GetValueOrDefault( ) : DateTime.MaxValue;

                                DateTime currentDate = DateTime.Now;

                                if ( ( min <= currentDate ) && ( currentDate <= max ) )
                                {
                                    showVoto = true;
                                }
                            }
                        }

                        if ( !showVoto )
                        {
                            Rimuovi.Add( "GAPC" );
                        }

                    }
                    else
                        Rimuovi.Add( "GAPC" );




                }
                if ( ecc == "GAVE" )
                {
                    if ( R.giornata != null && R.eccezioni != null &&
                                              ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "FF" ) ) )
                                           )
                    {
                        var infoDipendentePatB = myRai.Business.CommonManager.GetInfoDipendente( 14 );
                        var infoDipendentePatC = myRai.Business.CommonManager.GetInfoDipendente( 10 );
                        DateTime currentDate = DateTime.Now;

                        bool showGAVE = (
                            ( infoDipendentePatB != null && currentDate > infoDipendentePatB.data_inizio && currentDate < infoDipendentePatB.data_fine )
                            ||
                            ( infoDipendentePatC != null && currentDate > infoDipendentePatC.data_inizio && currentDate < infoDipendentePatC.data_fine )
                            );

                        if ( !showGAVE )
                        {
                            Rimuovi.Add( "GAVE" );
                        }

                    }
                    else
                        Rimuovi.Add( "GAVE" );
                }
                if ( ecc == "GAVU" )
                {
                    if ( R.giornata != null && R.eccezioni != null &&
                                              ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "SE" ) ) )
                                           )
                    {
                        var infoDipendentePatB = myRai.Business.CommonManager.GetInfoDipendente( 14 );
                        var infoDipendentePatC = myRai.Business.CommonManager.GetInfoDipendente( 10 );
                        DateTime currentDate = DateTime.Now;

                        bool showGAVU = (
                            ( infoDipendentePatB != null && currentDate > infoDipendentePatB.data_inizio && currentDate < infoDipendentePatB.data_fine )
                            ||
                            ( infoDipendentePatC != null && currentDate > infoDipendentePatC.data_inizio && currentDate < infoDipendentePatC.data_fine )
                            );

                        if ( !showGAVU )
                        {
                            Rimuovi.Add( "GAVU" );
                        }

                    }
                    else
                        Rimuovi.Add( "GAVU" );
                }
                if (ecc == "IS")
                {
                    if ((R.timbrature == null || !R.timbrature.Any()) && myRai.Models.Utente.IsSedeGappTerritoriale())
                    {
                        //ti spetta
                    }
                    else
                        Rimuovi.Add("IS");
                }
                if ( ecc == "LEXF" )
                {
                    List<DateTime> LD = new List<DateTime>( );
                    LD.Add(new DateTime(2020, 03, 19));
                    LD.Add(new DateTime(2020, 05, 10));
                    LD.Add(new DateTime(2020, 05, 31));
                    LD.Add(new DateTime(2020, 06, 29));
                    LD.Add(new DateTime(2020, 06, 20));
                    LD.Add(new DateTime(2020, 11, 4));

                    var db = new myRaiData.digiGappEntities( );
                    string sede = Utente.SedeGapp(DateTime.Now);
                    DateTime D = DateTime.Now;
                    string cal = db.L2D_SEDE_GAPP.Where( x => x.cod_sede_gapp == sede && x.data_inizio_validita <= D && x.data_fine_validita >= D ).Select( x => x.CalendarioDiSede ).FirstOrDefault( );

                    if ( cal != null && cal.Trim( ).ToUpper( ) == "ROMA" )
                        LD.Add(new DateTime(2020, 06, 24));


                    if (Utente.TipoDipendente() == "G"
                         && LD.Contains( R.giornata.data )
                         && !Utente.GiornalistaDelleReti()
                         //&& (R.giornata.tipoGiornata == "R" || R.giornata.tipoGiornata == "S")
                         && !R.eccezioni.Any( m => m.cod == "LF36" )
                         && !R.eccezioni.Any( m => m.cod == "LF80" )
                         && R.giornata.orarioReale != "90" && R.giornata.orarioReale != "95" && R.giornata.orarioReale != "96"
                         &&
                             ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) || x.cod.Trim( ).StartsWith( "IS" ) )
                               ||
                             ( R.timbrature != null && R.timbrature.Length > 0 ) )

                        )
                    {
                        //bravo ti spetta
                    }
                    else
                        Rimuovi.Add( "LEXF" );
                }
                if ( ecc == "LF36" )
                {
                    if (Utente.TipoDipendente() == "G"
                          && !Utente.GiornalistaDelleReti()
                          && R.giornata != null
                          && R.eccezioni != null
                          && R.giornata.tipoGiornata == "P"
                          && R.giornata.orarioReale != "90" && R.giornata.orarioReale != "95" && R.giornata.orarioReale != "96"
                          &&
                             ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) || x.cod.Trim( ).StartsWith( "IS" ) )
                               ||
                             ( R.timbrature != null && R.timbrature.Length > 0 ) )
                          )
                    {
                        int q = myRai.Business.CommonManager.QuanteEccezioniAnno("LF36", CommonManager.GetCurrentUserMatricola());
                        if (q >= 2) Rimuovi.Add("LF36");
                        //bravo ti spetta
                    }
                    else
                        Rimuovi.Add( "LF36" );
                }
                if ( ecc == "LF80" )
                {
                    List<DateTime> LD = new List<DateTime>( );
                    LD.Add(new DateTime(2019, 03, 19));
                    LD.Add(new DateTime(2019, 05, 10));
                    LD.Add(new DateTime(2019, 05, 31));
                    LD.Add(new DateTime(2019, 06, 29));
                    LD.Add(new DateTime(2019, 11, 4));
                    LD.Add(new DateTime(2019, 11, 3));

                    if (Utente.TipoDipendente() == "G"
            && !LD.Contains( R.giornata.data )
                          && !Utente.GiornalistaDelleReti()
                          && R.giornata != null
                          && R.eccezioni != null
                          && ( R.giornata.tipoGiornata == "R" || R.giornata.tipoGiornata == "S" )
                          && !R.eccezioni.Any( m => m.cod == "LF36" )
                          && R.giornata.orarioReale != "90" && R.giornata.orarioReale != "95" && R.giornata.orarioReale != "96"
                          &&
                             ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) || x.cod.Trim( ).StartsWith( "IS" ) )
                               ||
                             ( R.timbrature != null && R.timbrature.Length > 0 ) )
                          )
                    {
                        //bravo ti spetta
                    }
                    else Rimuovi.Add("LF80");
                }
                if ( ecc == "LFH6" )
                {
                    if (
                          R.giornata != null
                          && R.eccezioni != null
                          && ( R.giornata.tipoGiornata == "R" || R.giornata.tipoGiornata == "S" )

                          && R.giornata.orarioReale != "90" && R.giornata.orarioReale != "95" && R.giornata.orarioReale != "96"
                          &&
                             ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) )
                               ||
                             ( R.timbrature != null && R.timbrature.Length > 0 ) )
                          )
                    {
                        //bravo ti spetta
                    }
                    else Rimuovi.Add("LFH6");
                }
                if ( ecc == "LFH8" )
                {
                    if (
                          R.giornata != null
                          && R.eccezioni != null
                          && ( R.giornata.tipoGiornata == "R" || R.giornata.tipoGiornata == "S" )

                          && R.giornata.orarioReale != "90" && R.giornata.orarioReale != "95" && R.giornata.orarioReale != "96"
                          &&
                             ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) )
                               ||
                             ( R.timbrature != null && R.timbrature.Length > 0 ) )
                          )
                    {
                        //bravo ti spetta
                    }
                    else Rimuovi.Add("LFH8");
                }
                if ( ecc == "LH50" )
                {
                    if (
                          R.giornata != null
                          && R.eccezioni != null
                          && R.giornata.tipoGiornata == "P"
                          && R.eccezioni.Any( x => x.cod.Trim( ) == "MF" )

                          &&
                             ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) )
                               ||
                             ( R.timbrature != null && R.timbrature.Length > 0 ) )
                          )
                    {
                        if ( R.timbrature != null && R.timbrature.Length > 0 && R.timbrature.First( ).entrata != null && R.timbrature.Last( ).uscita != null )
                        {
                            int timbIniz = EccezioniManager.calcolaMinuti(R.timbrature.First().entrata.orario);
                            int timbFin = EccezioniManager.calcolaMinuti(R.timbrature.Last().uscita.orario);
                            if ( ( timbIniz <= 21 * 60 && timbFin >= 6 * 60 ) )
                            {
                                //bravo ti spetta
                            }
                            else
                                Rimuovi.Add( "LH50" );
                        }
                        //bravo ti spetta
                    }
                    else
                        Rimuovi.Add( "LH50" );
                }
                if ( ecc == "LH65" )
                {
                    if (
                          R.giornata != null
                          && R.eccezioni != null
                          && R.giornata.tipoGiornata == "P"
                           && R.eccezioni.Any( x => x.cod.Trim( ) == "MF" )

                          &&
                             ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) )
                               ||
                             ( R.timbrature != null && R.timbrature.Length > 0 ) )
                          )
                    {
                        if ( R.timbrature != null && R.timbrature.Length > 0 && R.timbrature.First( ).entrata != null && R.timbrature.Last( ).uscita != null )

                        {
                            int timbIniz = EccezioniManager.calcolaMinuti(R.timbrature.First().entrata.orario);
                            int timbFin = EccezioniManager.calcolaMinuti(R.timbrature.Last().uscita.orario);
                            if ( ( timbIniz <= 24 * 60 && timbFin >= 21 * 60 ) )
                            {
                                //bravo ti spetta
                            }
                            else
                                Rimuovi.Add( "LH65" );
                        }
                        //bravo ti spetta
                    }
                    else Rimuovi.Add("LH65");
                }
                if ( ecc == "LH75" )
                {
                    if (
                          R.giornata != null
                          && R.eccezioni != null
                          && R.giornata.tipoGiornata == "P"
                           && R.eccezioni.Any( x => x.cod.Trim( ) == "MF" )

                          &&
                             ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) )
                               ||
                             ( R.timbrature != null && R.timbrature.Length > 0 ) )
                          )
                    {
                        if ( R.timbrature != null && R.timbrature.Length > 0 && R.timbrature.First( ).entrata != null && R.timbrature.Last( ).uscita != null )

                        {
                            int timbIniz = EccezioniManager.calcolaMinuti(R.timbrature.First().entrata.orario);
                            int timbFin = EccezioniManager.calcolaMinuti(R.timbrature.Last().uscita.orario);
                            if ( ( ( timbIniz <= 6 * 60 && timbIniz >= 0 ) || ( timbFin >= 24 * 60 ) ) )
                            {
                                //bravo ti spetta
                            }
                            else
                                Rimuovi.Add( "LH75" );
                        }
                        //bravo ti spetta
                    }
                    else Rimuovi.Add("LH75");
                }
                if ( ecc == "LMH5" )
                {
                    if (
                          R.giornata != null
                          && R.eccezioni != null
                          && R.giornata.tipoGiornata == "P"
                          && R.eccezioni.Any( x => x.cod.Trim( ) == "MF" )

                           &&
                             ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) || x.cod.Trim( ).StartsWith( "IS" ) )
                               ||
                             ( R.timbrature != null && R.timbrature.Length > 0 ) )
                          )
                    {
                        //bravo ti spetta
                    }
                    else Rimuovi.Add("LMH5");
                }
                if ( ecc == "LMH6" )
                {
                    if (
                          R.giornata != null
                          && R.eccezioni != null
                          && R.giornata.tipoGiornata == "P"
                          && R.eccezioni.Any( x => x.cod.Trim( ) == "MF" )

                          &&
                             ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) || x.cod.Trim( ).StartsWith( "IS" ) )
                               ||
                             ( R.timbrature != null && R.timbrature.Length > 0 ) )
                          )
                    {
                        //bravo ti spetta
                    }
                    else Rimuovi.Add("LMH6");
                }
                if ( ecc == "LMH7" )
                {
                    if (
                          R.giornata != null
                          && R.eccezioni != null
                          && R.giornata.tipoGiornata == "P"


                          &&
                             ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) || x.cod.Trim( ).StartsWith( "IS" ) )
                               ||
                             ( R.timbrature != null && R.timbrature.Length > 0 ) )
                          )
                    {
                        //bravo ti spetta
                    }
                    else Rimuovi.Add("LMH7");
                }
                if ( ecc == "LN16" )
                {
                    bool FuoriSede = R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) || x.cod.Trim( ).StartsWith( "IS" ) );
                    bool HoTimbrature = R.timbrature != null && R.timbrature.Length > 0 && R.timbrature.First( ).entrata != null;
                    bool LN16 = false;
                    if (
                      Utente.TipoDipendente() == "G"
                       && !Utente.GiornalistaDelleReti()
                        && R.giornata != null
                        && R.eccezioni != null
                        && R.giornata.orarioReale != "90" && R.giornata.orarioReale != "95" && R.giornata.orarioReale != "96"
                        &&
                          ( FuoriSede || HoTimbrature )
                        )
                    {
                        int entIniz = 0;
                        int.TryParse( R.orario.entrata_iniziale , out entIniz );
                        if ( entIniz <= 360 )
                        {
                            if (FuoriSede) LN16 = true;
                            if (HoTimbrature && R.timbrature.First().entrata.orario == "<6:00") LN16 = true;
                        }
                    }
                    if (!LN16) Rimuovi.Add("LN16");
                }
                if ( ecc == "LN20" )
                {

                    bool FuoriSede = R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) || x.cod.Trim( ).StartsWith( "IS" ) );
                    bool HoTimbrature = R.timbrature != null && R.timbrature.Length > 0;

                    if (
                      Utente.TipoDipendente() == "G"
                        && !Utente.GiornalistaDelleReti()
                        && R.giornata != null
                        && R.eccezioni != null
                        && R.giornata.orarioReale != "90" && R.giornata.orarioReale != "95" && R.giornata.orarioReale != "96"
                        &&
                           ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) || x.cod.Trim( ).StartsWith( "IS" ) )
                             ||
                           ( R.timbrature != null && R.timbrature.Length > 0 ) )
                        )
                    {

                        int uscIniz = 0;
                        int.TryParse( R.orario.uscita_iniziale , out uscIniz );
                        if ( uscIniz > 1380 || uscIniz == 0 )
                        {
                            if ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) || x.cod.Trim( ).StartsWith( "IS" ) ) )
                            {
                                //ok ti spetta
                            }
                            else
                            {
                                if ( R.timbrature != null && R.timbrature.Count( ) > 0 )
                                {
                                    if ( R.timbrature.Last( ).uscita != null && R.timbrature.Last( ).uscita.orario == "> 23" )
                                    {
                                        //ok ti spetta
                                    }
                                    else
                                        Rimuovi.Add( "LN20" );
                                }
                                else
                                    Rimuovi.Add( "LN20" );
                            }

                        }
                        else
                            Rimuovi.Add( "LN20" );
                    }
                    else
                        Rimuovi.Add( "LN20" );
                }
                if ( ecc == "LN25" )
                {
                    if (
                      Utente.TipoDipendente() == "G"
                        && !Utente.GiornalistaDelleReti()
                        && R.giornata != null
                        && R.eccezioni != null
                        && R.giornata.orarioReale != "90" && R.giornata.orarioReale != "95" && R.giornata.orarioReale != "96"
                        &&
                           ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) || x.cod.Trim( ).StartsWith( "IS" ) )
                             ||
                           ( R.timbrature != null && R.timbrature.Length > 0 ) )
                        )
                    {

                        int entIniz = 0;
                        int.TryParse( R.orario.entrata_iniziale , out entIniz );
                        if ( entIniz <= 330 )
                        {
                            if ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) || x.cod.Trim( ).StartsWith( "IS" ) ) )
                            {
                                //ok ti spetta
                            }
                            else
                            {
                                if ( R.timbrature != null && R.timbrature.Count( ) > 0 && R.timbrature.First( ).entrata != null )
                                {
                                    if ( R.timbrature.First( ).entrata.orario == "<5:31" )
                                    {
                                        //ok ti spetta
                                    }
                                    else
                                        Rimuovi.Add( "LN25" );
                                }
                                else
                                    Rimuovi.Add( "LN25" );
                            }
                        }
                        else
                            Rimuovi.Add( "LN25" );


                    }
                    else
                        Rimuovi.Add( "LN25" );
                }

                if ( ecc == "LN50" )
                {
                    bool FuoriSede = R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) || x.cod.Trim( ).StartsWith( "IS" ) );
                    bool HoTimbrature = R.timbrature != null && R.timbrature.Length > 0;
                    bool LN50 = false;
                    if (
                      Utente.TipoDipendente() == "G"
                       && !Utente.GiornalistaDelleReti()
                        && R.giornata != null
                        && R.eccezioni != null
                        && R.giornata.orarioReale != "90" && R.giornata.orarioReale != "95" && R.giornata.orarioReale != "96"
                        &&
                           ( FuoriSede || HoTimbrature )
                        )
                    {
                        int entIniz = 0;
                        int.TryParse( R.orario.entrata_iniziale , out entIniz );
                        int uscIniz = 0;
                        int.TryParse( R.orario.uscita_iniziale , out uscIniz );
                        bool OrarioRispettato = ( entIniz <= ( 24 * 60 ) && uscIniz >= ( ( 29 * 60 ) + 30 ) );

                        if ( OrarioRispettato )
                        {
                            if (FuoriSede) LN50 = true;
                            if (HoTimbrature && R.timbrature.First().entrata.orario == "> 23") LN50 = true;
                        }
                    }
                    if (!LN50) Rimuovi.Add("LN50");
                }
                if ( ecc == "LNH5" )
                {
                    if (
                        R.giornata.data.DayOfWeek != DayOfWeek.Sunday &&
                       R.giornata != null
                        && R.eccezioni != null
                        && R.giornata.orarioReale != "90" && R.giornata.orarioReale != "95" && R.giornata.orarioReale != "96"
&& !R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "MF" ) || x.cod.Trim( ).StartsWith( "MR" ) )
                        &&
                           ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ) == "SE" )
                             ||
                           ( R.timbrature != null && R.timbrature.Length > 0 ) )
                        )
                    {
                        if (myRai.Models.Utente.TipoDipendente() == "Q" && !myRai.Models.Utente.ExArt12inequipeex33per())
                        {
                            Rimuovi.Add("LNH5");
                        }
                        else if (R.eccezioni.Any(x => x.cod.Trim().StartsWith("FS") || x.cod.Trim().StartsWith("SE")))
                        {
                            //ok ti spetta
                        }
                        else
                        {
                            int entIniz = 0;
                            int timbIniz = 0;
                            int timbFIN = 0;
                            int.TryParse( R.orario.entrata_iniziale , out entIniz );
                            if ( entIniz < 360 )
                            {
                                if ( R.timbrature != null && R.timbrature.Count( ) > 0 )
                                {
                                    if ( R.timbrature.Last( ).uscita != null )
                                    {
                                        timbIniz = EccezioniManager.calcolaMinuti(R.timbrature.First().entrata.orario);
                                        if ( timbIniz < 360 )
                                        {
                                            //ok ti spetta
                                        }
                                        else
                                            Rimuovi.Add( "LNH5" );
                                    }
                                    else
                                        Rimuovi.Add( "LNH5" );

                                }
                                else
                                    Rimuovi.Add( "LNH5" );

                            }
                            else
                            {

                                if ( R.timbrature != null && R.timbrature.Count( ) > 0 && R.timbrature.Last( ).uscita != null )
                                {
                                    timbFIN = EccezioniManager.calcolaMinuti(R.timbrature.Last().uscita.orario);
                                    Boolean spettante = false;
                                    var ee = R.eccezioni.Where( x => x.cod.Trim( ) == "SMAP" ).ToList( );
                                    if ( ee != null && ee.Count( ) > 0 )
                                    {
                                        foreach ( var item in ee )
                                        {
                                            int mi = EccezioniManager.calcolaMinuti(item.alle);
                                            if ( mi > 1260 )
                                            {
                                                spettante = true;
                                                break;
                                            }
                                        }
                                    }
                                    if ( timbFIN > 1260 || spettante )
                                    {
                                        //ok ti spetta
                                    }
                                    else
                                        Rimuovi.Add( "LNH5" );
                                }
                                else
                                    Rimuovi.Add( "LNH5" );
                            }
                        }
                    }
                    else
                        Rimuovi.Add( "LNH5" );
                }
                if ( ecc == "LPH5" )
                {
                    if (
                          R.giornata != null
                          && R.eccezioni != null
                          && R.giornata.tipoGiornata == "P"
                          && R.eccezioni.Any( x => x.cod.Trim( ) == "MFS" )

                          &&
                             ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) )
                               ||
                             ( R.timbrature != null && R.timbrature.Length > 0 ) )
                          )
                    {
                        if ( R.timbrature != null && R.timbrature.Length > 0 && R.timbrature.First( ).entrata != null && R.timbrature.Last( ).uscita != null )
                        {
                            int timbIniz = EccezioniManager.calcolaMinuti(R.timbrature.First().entrata.orario);
                            int timbFin = EccezioniManager.calcolaMinuti(R.timbrature.Last().uscita.orario);
                            if ( ( timbIniz <= 21 * 60 && timbFin >= 6 * 60 ) )
                            {
                                //bravo ti spetta
                            }
                            else
                                Rimuovi.Add( "LPH5" );
                        }
                        //bravo ti spetta
                    }
                    else
                        Rimuovi.Add( "LPH5" );
                }
                if ( ecc == "LPH6" )
                {
                    if (
                          R.giornata != null
                          && R.eccezioni != null
                          && R.giornata.tipoGiornata == "P"
                           && R.eccezioni.Any( x => x.cod.Trim( ) == "MFS" )

                          &&
                             ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) )
                               ||
                             ( R.timbrature != null && R.timbrature.Length > 0 ) )
                          )
                    {
                        if ( R.timbrature != null && R.timbrature.Length > 0 && R.timbrature.First( ).entrata != null && R.timbrature.Last( ).uscita != null )

                        {
                            int timbIniz = EccezioniManager.calcolaMinuti(R.timbrature.First().entrata.orario);
                            int timbFin = EccezioniManager.calcolaMinuti(R.timbrature.Last().uscita.orario);
                            if ( ( timbIniz <= 24 * 60 && timbFin >= 21 * 60 ) )
                            {
                                //bravo ti spetta
                            }
                            else
                                Rimuovi.Add( "LPH6" );
                        }
                        //bravo ti spetta
                    }
                    else Rimuovi.Add("LPH6");
                }
                if ( ecc == "LPH7" )
                {
                    if (
                          R.giornata != null
                          && R.eccezioni != null
                          && R.giornata.tipoGiornata == "P"
                           && R.eccezioni.Any( x => x.cod.Trim( ) == "MFS" )

                          &&
                             ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) )
                               ||
                             ( R.timbrature != null && R.timbrature.Length > 0 ) )
                          )
                    {
                        if ( R.timbrature != null && R.timbrature.Length > 0 && R.timbrature.First( ).entrata != null && R.timbrature.Last( ).uscita != null )

                        {
                            int timbIniz = EccezioniManager.calcolaMinuti(R.timbrature.First().entrata.orario);
                            int timbFin = EccezioniManager.calcolaMinuti(R.timbrature.Last().uscita.orario);
                            if ( ( ( timbIniz <= 6 * 60 && timbIniz >= 0 ) || ( timbFin >= 24 * 60 ) ) )
                            {
                                //bravo ti spetta
                            }
                            else
                                Rimuovi.Add( "LPH7" );
                        }
                        //bravo ti spetta
                    }
                    else Rimuovi.Add("LPH7");
                }
                if ( ecc == "MFH6" )
                {
                    if (
                          R.giornata != null
                          && R.eccezioni != null
                          && (R.giornata.tipoGiornata == "P" || R.giornata.tipoGiornata == "S" || R.giornata.tipoGiornata == "R")
                           && R.eccezioni.Any( x => x.cod.Trim( ) == "MF" )

                          &&
                             ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) || x.cod.Trim( ).StartsWith( "IS" ) )
                               ||
                             ( R.timbrature != null && R.timbrature.Length > 0 ) )
                          )
                    {
                        //bravo ti spetta
                    }
                    else Rimuovi.Add("MFH6");
                }
                if ( ecc == "MFH8" )
                {
                    if (
                          R.giornata != null
                          && R.eccezioni != null
                             && (R.giornata.tipoGiornata == "P" || R.giornata.tipoGiornata == "S" || R.giornata.tipoGiornata == "R")
                           && R.eccezioni.Any( x => x.cod.Trim( ) == "MF" )

                          &&
                             ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) || x.cod.Trim( ).StartsWith( "IS" ) )
                               ||
                             ( R.timbrature != null && R.timbrature.Length > 0 ) )
                          )
                    {
                        //bravo ti spetta
                    }
                    else Rimuovi.Add("MFH8");
                }
                if ( ecc == "MG" )
                {

                    if ( R.eccezioni.Any( x => x.cod.Trim( ) == "CAR" ) == false )
                        Rimuovi.Add( "MG" );
                }
                if ( ecc == "MLH" )
                {
                    var car = R.eccezioni.Where( x => x.cod.Trim( ) == "CAR" ).FirstOrDefault( );
                    if ( car != null && !String.IsNullOrWhiteSpace( car.qta ) && car.qta.Trim( ).Length == 5 && car.qta.Contains( ":" ) )
                    {
                        int carenza = EccezioniManager.calcolaMinuti(car.qta);

                        if ( carenza > 0 )
                        {
                            if ( carenza > ( Convert.ToInt32( R.orario.prevista_presenza ) / 2 ) )
                            {
                                Rimuovi.Add( "MLH" );
                            }
                        }
                    }
                    else
                        Rimuovi.Add( "MLH" );
                }
                if (ecc == "MN")
                {
                    if (R.eccezioni != null && R.eccezioni.Any(x => x.cod != null && x.cod.Trim() == "MN00"))
                    {
                        Rimuovi.Add("MN");
                    }
                }
                if ( ecc == "MN00" )
                {
                    if (
                       R.giornata != null
                        && R.eccezioni != null
                        && !R.eccezioni.Any(x => x.cod.Trim() == "MN")
                        && R.giornata.orarioReale != "90" && R.giornata.orarioReale != "95" && R.giornata.orarioReale != "96"
            && R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "MNS" ) )
                        &&
                           ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) )
                             ||
                           ( R.timbrature != null && R.timbrature.Length > 0 ) )
                        )
                    {




                    }
                    else
                        Rimuovi.Add( "MN00" );
                }

                if ( ecc == "MNS" )
                {
                    if ( R.giornata != null
                          && R.eccezioni != null
                          && R.giornata.orarioReale != "90" && R.giornata.orarioReale != "95" && R.giornata.orarioReale != "96"
                          &&
                             ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) || x.cod.Trim( ).StartsWith( "IS" ) )
                               ||
                             ( R.timbrature != null && R.timbrature.Length > 0 ) )
                          )
                    {
                        //bravo ti spetta
                    }
                    else Rimuovi.Add("MNS");
                }
                if ( ecc == "MR" )
                {
                    if (R.giornata.eccezioni != null && R.giornata.eccezioni.Any(x => x.cod.StartsWith("MR"))) Rimuovi.Add("MR");
                }
                if ( ecc == "MRM" )
                {
                    if (R.giornata != null && R.eccezioni != null && R.giornata.eccezioni != null && R.giornata.eccezioni.Any(x => x.cod.StartsWith("MR"))) Rimuovi.Add("MRM");
                }
                if ( ecc == "MRP" )
                {
                    if (R.giornata != null && R.giornata.eccezioni != null && R.giornata.eccezioni.Any(x => x.cod.StartsWith("MR"))) Rimuovi.Add("MRP");
                }
                if ( ecc == "ORV" )
                {

                    if ( R.giornata != null && R.eccezioni != null && R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ) == "FSV" || x.cod.Trim( ) == "NV" || x.cod.Trim( ) == "RV" || x.cod.Trim( ).StartsWith( "FF" ) ) )
                    {
                        //bravo ti spetta
                    }
                    else
                    {

                        //var trasferta = myRai.Business.TrasferteManager.GetTrasferte(R.giornata.data, Utente.Matricola());
                        //var db = new myRaiData.digiGappEntities( );
                        //var tmpListEccC = db.L2D_ECCEZIONE.Where( x => x.flag_eccez == "C" ).Select( y => y.cod_eccezione );

                        //if ( trasferta != null
                        //    && trasferta.Count( ) > 0
                        //    && trasferta.Any( x => x.Itinerario != null && x.Itinerario.Count( ) > 0 && x.Itinerario.FirstOrDefault( ).DataPartenza.Value.Date == R.giornata.data.Date )
                        //    && R.giornata != null
                        //    && R.eccezioni != null
                        //    && R.eccezioni.Any( x => tmpListEccC.Contains( x.cod.Trim( ) ) ) )
                        //{
                        //}
                        //else
                        //    Rimuovi.Add( "ORV" );
                    }
                }
                {
                    if (
                      Utente.TipoDipendente() == "G"
                       && !Utente.GiornalistaDelleReti()
                        && R.giornata != null
                        && R.eccezioni != null
                        && R.giornata.orarioReale != "90" && R.giornata.orarioReale != "95" && R.giornata.orarioReale != "96"
                        &&
                           ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) || x.cod.Trim( ).StartsWith( "IS" ) )
                             ||
                           ( R.timbrature != null && R.timbrature.Length > 0 ) )
                        )
                    {
                        //bravo ti spetta
                    }
                    else
                        Rimuovi.Add( "PDCO" );
                }
                if ( ecc == "PDRA" )
                {

                    if (myRai.Business.CommonManager.HasInfoDipendente(15, true)
                                            && R.giornata != null
                                            && R.eccezioni != null
                                            && R.giornata.orarioReale != "90" && R.giornata.orarioReale != "95" && R.giornata.orarioReale != "96"
                                            &&
                                               ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) || x.cod.Trim( ).StartsWith( "IS" ) )
                                                 ||
                                               ( R.timbrature != null && R.timbrature.Length > 0 ) )
                                            )
                    {
                        //bravo ti spetta

                    }
                    else
                        Rimuovi.Add( "PDRA" );
                }
                if ( ecc == "PFM" )
                {

                    if (R.eccezioni.Any(x => x.cod.Trim() == "SNP") || CommonManager.IsDateInControlloSNM_SMP(R.giornata.data))
                    {

                        Rimuovi.Add( "PFM" );
                    }

                }
                if ( ecc == "PFP" )
                {

                    if (R.eccezioni.Any(x => x.cod.Trim() == "SNP") || CommonManager.IsDateInControlloSNM_SMP(R.giornata.data))
                    {

                        Rimuovi.Add( "PFP" );
                    }

                }
                if ( ecc == "PFQ" )
                {

                    if (R.eccezioni.Any(x => x.cod.Trim() == "SNP") || CommonManager.IsDateInControlloSNM_SMP(R.giornata.data) || R.eccezioni.Any(x => x.cod.Trim().EndsWith("Q")))
                    {

                        Rimuovi.Add( "PFQ" );
                    }

                }
                if (ecc == "PGM")
                               {

                    if (R.eccezioni.Any(x => x.cod.Trim() == "SNP") || CommonManager.IsDateInControlloSNM_SMP(R.giornata.data))
                                  {

                                       Rimuovi.Add("PGM");
                                   }

                }
                if (ecc == "PGP")
                               {

                    if (R.eccezioni.Any(x => x.cod.Trim() == "SNP") || CommonManager.IsDateInControlloSNM_SMP(R.giornata.data))
                                  {

                                       Rimuovi.Add("PGP");
                                   }

                }
                if ( ecc == "PGQ" )
                {
                    if (R.eccezioni.Any(x => x.cod.Trim().EndsWith("Q")) || CommonManager.IsDateInControlloSNM_SMP(R.giornata.data))
                    {
                        Rimuovi.Add( "PGQ" );
                    }
                }
                /*if (ecc=="POH")
                { 

                                           if (R.eccezioni.Any(x => x.cod.Trim() == "CAR") == false)
                                           Rimuovi.Add("POH");
                }*/

                if ( ecc == "PRM" )
                {

                    if (R.eccezioni.Any(x => x.cod.Trim() == "SNP") || CommonManager.IsDateInControlloSNM_SMP(R.giornata.data))
                    {

                        Rimuovi.Add( "PRM" );
                    }

                }
                if ( ecc == "PRP" )
                {

                    if (R.eccezioni.Any(x => x.cod.Trim() == "SNP") || CommonManager.IsDateInControlloSNM_SMP(R.giornata.data))
                    {

                        Rimuovi.Add( "PRP" );
                    }

                }
                if ( ecc == "PRQ" )
                {

                    if (R.eccezioni.Any(x => x.cod.Trim() == "SNP") || R.eccezioni.Any(x => x.cod.Trim().EndsWith("Q")) || CommonManager.IsDateInControlloSNM_SMP(R.giornata.data))
                    {

                        Rimuovi.Add( "PRQ" );
                    }

                }
                if ( ecc == "PVH" )
                {


                    if (
                                   R.giornata != null
                                    && R.eccezioni != null
                                    && R.giornata.orarioReale != "90" && R.giornata.orarioReale != "95" && R.giornata.orarioReale != "96"
                                    &&
                                       ( R.timbrature != null && R.timbrature.Length > 0 )
                                    )
                    {
                        if ( R.eccezioni.Any( x => x.cod.Trim( ) == "CAR" ) == false )

                            Rimuovi.Add( "PVH" );


                    }
                    else
                        Rimuovi.Add( "PVH" );




                }
                if (ecc == "RFM")
                {
                    if (CommonManager.IsDateInControlloSNM_SMP(R.giornata.data))
                    {

                        Rimuovi.Add("RFM");
                    }
                }
                if (ecc == "RFP")
                {
                    if (CommonManager.IsDateInControlloSNM_SMP(R.giornata.data))
                    {

                        Rimuovi.Add("RFP");
                    }
                }
                if ( ecc == "RKMF" )
                {
                    string sedecont = myRai.Models.Utente.EsponiAnagrafica().SedeContabile;
                    if (
                        (sedecont == "770" || sedecont == "680" || sedecont == "410")
                        &&
                        (R.giornata.orarioReale.Trim() == "A" || R.giornata.orarioReale.Trim() == "A*" || R.giornata.orarioReale.Trim() == "A+")
                       )
                    {
                        //ti spetta
                    }
                    else Rimuovi.Add("RKMF");
                }
                if ( ecc == "RMTR" )
                {
                    if (R.eccezioni != null && R.eccezioni.Any(x => x.cod != null && x.cod.Trim() == "FS"))
                    {
                        Rimuovi.Add("RMTR");
                    }
                    else
                    {
                    var db = new myRaiData.digiGappEntities( );
                        string mysede = Utente.SedeGapp();
                    DateTime dnow = DateTime.Now;

                    var sedegapp = EccezioniManager.GetSedeGappFromL2D( mysede , R.giornata.data );

                        string intRmtr = EccezioniManager.GetIntervalloRMTRinsediamento(R);//  sedegapp.RMTR_intervallo;

                    int minMattina = 0, minSera = 0;

                    if ( !string.IsNullOrWhiteSpace( intRmtr ) && intRmtr.Length == 9 && intRmtr.Contains( "/" ) )
                    {
                            minMattina = EccezioniManager.calcolaMinuti(intRmtr.Split('/')[0]);
                            minSera = EccezioniManager.calcolaMinuti(intRmtr.Split('/')[1]);
                    }
                    if ( minMattina != 0 && minSera != 0 )
                    {
                        if ( R.timbrature != null &&
                                R.timbrature.Count( ) > 0 &&
                                R.timbrature.First( ).entrata != null &&
                                R.timbrature.Last( ).uscita != null &&
                                (
                                        (EccezioniManager.calcolaMinuti(R.timbrature.Last().uscita.orario) >= minSera)
                                ||
                                    ( R.orario != null &&
                                        !String.IsNullOrEmpty( R.orario.entrata_iniziale ) &&
                                        Convert.ToInt32( R.orario.entrata_iniziale ) < minMattina &&
                                            EccezioniManager.calcolaMinuti(R.timbrature.First().entrata.orario) < minMattina)
                                )
                            )
                            //   ((EccezioniManager.calcolaMinuti(R.timbrature.First().entrata.orario) < minMattina
                            //   || EccezioniManager.calcolaMinuti(R.timbrature.Last().uscita.orario) >= minSera
                            //   )
                            //  &&
                            //   (R.orario != null &&
                            //       !String.IsNullOrEmpty(R.orario.entrata_iniziale) &&
                            //       Convert.ToInt32(R.orario.entrata_iniziale) < minMattina &&
                            //       EccezioniManager.calcolaMinuti(R.timbrature.First().entrata.orario) < minMattina
                            //   ))


                            //)
                        {
                            //ti spetta
                        }
                        else
                        {
                            Rimuovi.Add( "RMTR" );
                        }
                    }
                    else
                        Rimuovi.Add( "RMTR" );
                }

                }
                if (ecc == "RNM")
                {
                    if (CommonManager.IsDateInControlloSNM_SMP(R.giornata.data))
                    {

                        Rimuovi.Add("RNM");
                    }
                }
                if (ecc == "RNP")
                {
                    if (CommonManager.IsDateInControlloSNM_SMP(R.giornata.data))
                    {

                        Rimuovi.Add("RNP");
                    }
                }
                if ( ecc == "ROH" )
                {

                    if (Utente.GetQuadratura() == Quadratura.Settimanale ||
                        R.giornata.maggiorPresenza.Trim( ) == "00:00" ||
                        Utente.GetPOH(false, R.giornata.data) <= Utente.GetROH(false, R.giornata.data)
                        )
                        Rimuovi.Add( "ROH" );
                }
                if (ecc == "RPAF")
                {
                    if (R.eccezioni != null && R.eccezioni.Any(x => x.cod.Trim() == "SW"))
                
    {
                        Rimuovi.Add("RPAF");
                    }

                    if (R.giornata.data >= DateTime.Now.Date || CommonManager.MensaFruitaData(R.giornata.data))
                    {
                        Rimuovi.Add("RPAF");
                    }

                }
                if ( ecc == "RR" )
                {
                    if ( R.giornata.orarioReale != "90" )
                    {
                        //bravo ti spetta
                    }
                    else
                        Rimuovi.Add( "RR" );
                }
                if (ecc == "RRM")
                {
                    if (CommonManager.IsDateInControlloSNM_SMP(R.giornata.data))
                    {

                        Rimuovi.Add("RRM");
                    }
                }
                if (ecc == "RRP")
                {
                    if (CommonManager.IsDateInControlloSNM_SMP(R.giornata.data))
                    {

                        Rimuovi.Add("RRP");
                    }
                }
                if ( ecc == "RVIV" )
                {
                    string sedecont = myRai.Models.Utente.EsponiAnagrafica().SedeContabile;

                    if (
                        (sedecont == "770" || sedecont == "680" || sedecont == "410") &&
                        (R.giornata.orarioReale.Trim() == "A" || R.giornata.orarioReale.Trim() == "A*")
                        )
                    {
                        //ti spetta
                    }
                    else Rimuovi.Add("RVIV");
                }
                if ( ecc == "SE" )
                {

                    if ( R.eccezioni.Any( x => ( x.cod.Trim( ).StartsWith( "FE" ) || x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) || x.cod.StartsWith( "FO" ) ) ) )
                    {

                        //bravo ti spetta
                        Rimuovi.Add( "SE" );
                    }

                }
                if ( ecc == "SEH" )
                {

                    if ( R.eccezioni.Any( x => ( x.cod.Trim( ) == "FE" || x.cod.Trim( ) == "FS" || x.cod.Trim( ) == "FO" ) ) )
                    {

                        //bravo ti spetta
                        Rimuovi.Add( "SEH" );
                    }

                }
                if ( ecc == "SEM" )
                {

                    if ( R.eccezioni.Any( x => ( x.cod.Trim( ) == "FEM" || x.cod.Trim( ) == "FE" ) ) )
                    {

                        //bravo ti spetta
                        Rimuovi.Add( "SEM" );
                    }
                    if ( R.eccezioni.Any( x => ( x.cod.Trim( ) == "FSM" || x.cod.Trim( ) == "FS" ) ) )
                    {

                        //bravo ti spetta
                        Rimuovi.Add( "SEM" );
                    }
                    if ( R.eccezioni.Any( x => ( x.cod.Trim( ) == "FOM" || x.cod.Trim( ) == "FO" ) ) )
                    {

                        //bravo ti spetta
                        Rimuovi.Add( "SEM" );
                    }
                    if ( R.eccezioni.Any( x => x.cod.Trim( ) == "SE" || x.cod.Trim( ) == "SEH" ) )
                    {

                        //bravo ti spetta
                        Rimuovi.Add( "SEM" );
                    }
                }

                if ( ecc == "SEP" )
                {

                    if ( R.eccezioni.Any( x => ( x.cod.Trim( ) == "FEP" || x.cod.Trim( ) == "FE" ) ) )
                    {

                        //bravo ti spetta
                        Rimuovi.Add( "SEP" );
                    }
                    if ( R.eccezioni.Any( x => ( x.cod.Trim( ) == "FSP" || x.cod.Trim( ) == "FS" ) ) )
                    {

                        //bravo ti spetta
                        Rimuovi.Add( "SEP" );
                    }
                    if ( R.eccezioni.Any( x => ( x.cod.Trim( ) == "FOP" || x.cod.Trim( ) == "FO" ) ) )
                    {

                        //bravo ti spetta
                        Rimuovi.Add( "SEP" );
                    }
                    if ( R.eccezioni.Any( x => x.cod.Trim( ) == "SE" || x.cod.Trim( ) == "SEH" ) )
                    {

                        //bravo ti spetta
                        Rimuovi.Add( "SEP" );
                    }
                }
                if ( ecc == "SRH6" )
                {
                    if (
                       R.giornata.orarioReale != "90" && R.giornata.orarioReale != "95" && R.giornata.orarioReale != "96"
                        &&
                           ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "MR" ) ) || R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "L7G" ) )
                        ) )
                    {
                        if ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "DH" ) ) )
                        {
                            Rimuovi.Add( "SRH6" );
                        }
                    }
                    else
                        Rimuovi.Add( "SRH6" );
                }
                if ( ecc == "SRH8" )
                {
                    if (
                       R.giornata.orarioReale != "90" && R.giornata.orarioReale != "95" && R.giornata.orarioReale != "96"
                        &&
                           ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "MR" ) ) || R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "L7G" ) )
                        ) )
                    {
                        if ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "DH" ) ) )
                        {
                            Rimuovi.Add( "SRH8" );
                        }
                    }
                    else
                        Rimuovi.Add( "SRH8" );
                }
                if ( ecc == "STR" )
                {
                    if (EccezioniManager.calcolaMinuti(R.giornata.maggiorPresenza.Trim()) < 10
                        || R.eccezioni.Any( x => x.cod.Trim( ) == "FE" )
                        || R.eccezioni.Any( x => x.cod.Trim( ) == "PR" )
                        || R.eccezioni.Any( x => x.cod.Trim( ) == "PF" )
                        || R.eccezioni.Any( x => x.cod.Trim( ) == "RR" )
                        || R.eccezioni.Any( x => x.cod.Trim( ) == "RF" )
                        || R.eccezioni.Any( x => x.cod.Trim( ) == "RN" )
                        )
                        //if (R.giornata.maggiorPresenza.Trim() == "00:00")
                        Rimuovi.Add( "STR" );
                }
                if ( ecc == "TN30" )
                {
                    if (

                         R.giornata != null
                        && R.eccezioni != null
                        && R.giornata.orarioReale != "90" && R.giornata.orarioReale != "95" && R.giornata.orarioReale != "96"
                        &&
                           ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) )
                             ||
                           ( R.timbrature != null && R.timbrature.Length > 0 ) )
                        )
                    {

                        int uscIniz = 0;
                        int.TryParse( R.orario.uscita_iniziale , out uscIniz );
                        if ( uscIniz >= 1470 )
                        {
                            if ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) ) )
                            {
                                //ok ti spetta
                            }
                            else
                            {
                                if ( R.timbrature != null && R.timbrature.Count( ) > 0 && R.timbrature.Last( ).uscita != null )
                                {
                                    if (EccezioniManager.calcolaMinuti(R.timbrature.Last().uscita.orario) >= 1470)
                                    {
                                        //ok ti spetta
                                    }
                                    else
                                        Rimuovi.Add( "TN30" );
                                }
                                else
                                    Rimuovi.Add( "TN30" );
                            }
                        }
                        else
                            Rimuovi.Add( "TN30" );
                    }
                    else
                        Rimuovi.Add( "TN30" );
                }
                if ( ecc == "TN35" )
                {
                    if (
                       R.giornata != null
                        && R.eccezioni != null
                        && R.giornata.orarioReale != "90" && R.giornata.orarioReale != "95" && R.giornata.orarioReale != "96"
                        &&
                           ( R.eccezioni.Any( x => x.cod.Trim( ).StartsWith( "FS" ) || x.cod.Trim( ).StartsWith( "SE" ) )
                             ||
                           ( R.timbrature != null && R.timbrature.Length > 0 ) )
                        )
                    {

                        int entIniz = 0;
                        int timbIniz = 0;

                        int.TryParse( R.orario.entrata_iniziale , out entIniz );
                        if ( ( entIniz >= 300 ) && ( entIniz <= 390 ) )
                        {
                            if ( R.eccezioni.Any( x => x.cod.Trim( ) == "FS" || x.cod.Trim( ) == "SE" || x.cod.Trim( ) == "FSM" || x.cod.Trim( ) == "SEM" ) )
                            {
                                //ok ti spetta
                            }
                            else
                            {
                                if ( R.timbrature != null && R.timbrature.Count( ) > 0 )
                                {
                                    if ( R.timbrature.Last( ).uscita != null )
                                    {
                                        timbIniz = EccezioniManager.calcolaMinuti(R.timbrature.First().entrata.orario);

                                        if ( R.eccezioni.Any( x => x.cod.Trim( ) == "FSH" ) )
                                        {
                                            var ee = R.eccezioni.Where( x => x.cod.Trim( ) == "FSH" ).FirstOrDefault( );

                                            if (EccezioniManager.calcolaMinuti(ee.dalle) < timbIniz)
                                                timbIniz = EccezioniManager.calcolaMinuti(ee.dalle);

                                        }
                                        if ( R.eccezioni.Any( x => x.cod.Trim( ) == "FS" ) || R.eccezioni.Any( x => x.cod.Trim( ) == "FSM" ) || ( ( timbIniz <= 390 ) ) )
                                        {
                                            //ok ti spetta
                                        }
                                        else
                                            Rimuovi.Add( "TN35" );
                                    }
                                    else
                                        Rimuovi.Add( "TN35" );

                                }
                                else
                                    Rimuovi.Add( "TN35" );
                            }

                        }
                        else
                            Rimuovi.Add( "TN35" );

                    }
                    else
                        Rimuovi.Add( "TN35" );
                }
                if (ecc == "UME")
                {

                    if (R.eccezioni.Any(x => x.cod.Trim() == "CAR") == false || R.eccezioni.Any(x => x.cod.Trim() == "URH"))
                        Rimuovi.Add("UME");



                    DateTime DOMANI = R.giornata.data.AddDays(1);
                    var db = new myRaiData.digiGappEntities();
                    string matricola = CommonManager.GetCurrentUserMatricola().PadLeft(8, '0');
                    myRaiData.MyRai_MensaXML scontrino = db.MyRai_MensaXML
                        .Where(x => ((x.TransactionDateTime > R.giornata.data) && (x.TransactionDateTime < DOMANI)))
                        .Where(g => g.Badge == matricola).FirstOrDefault();




                    if (scontrino != null)
                        Rimuovi.Add("UME");
                    if (R.giornata.orario != null && R.giornata.orario.intervallo_mensa == "00:00")
                        Rimuovi.Add("UME");


                }
                if ( ecc == "UMH" )
                {

                    if ( R.eccezioni.Any( x => x.cod.Trim( ) == "CAR" ) == false || R.eccezioni.Any( x => x.cod.Trim( ) == "URH" ) )
                        Rimuovi.Add( "UMH" );



                    DateTime DOMANI = R.giornata.data.AddDays( 1 );
                    var db = new myRaiData.digiGappEntities( );
                    string matricola = CommonManager.GetCurrentUserMatricola().PadLeft(8, '0');
                    myRaiData.MyRai_MensaXML scontrino = db.MyRai_MensaXML
                        .Where( x => ( ( x.TransactionDateTime > R.giornata.data ) && ( x.TransactionDateTime < DOMANI ) ) )
                        .Where( g => g.Badge == matricola ).FirstOrDefault( );




                    if ( scontrino != null )
                        Rimuovi.Add( "UMH" );
                    if (R.giornata.orario != null && R.giornata.orario.intervallo_mensa == "00:00")
                        Rimuovi.Add("UMH");


                }
                if ( ecc == "URH" )
                {
                    if ( R.eccezioni.Any( x => x.cod.Trim( ) == "UMH" ) )
                    {
                        Rimuovi.Add( "URH" );
                    }
                    else
                    {
                        var db = new myRaiData.digiGappEntities( );
                        var sedeg = Utente.SedeGapp();
                        var s = db.L2D_SEDE_GAPP.Where( x => x.cod_sede_gapp.Trim( ) == sedeg.Trim( )
                             && x.data_inizio_validita <= R.giornata.data
                             && x.data_fine_validita >= R.giornata.data
                            ).FirstOrDefault( );
                        if ( s == null || String.IsNullOrWhiteSpace( s.periodo_mensa ) || s.periodo_mensa == "0" )
                        {
                            Rimuovi.Add( "URH" );
                        }
                    }
                    if (R.giornata.orario != null && R.giornata.orario.intervallo_mensa == "00:00")
                        Rimuovi.Add("URH");

                }


            }
            return Rimuovi;
        }
    }
}