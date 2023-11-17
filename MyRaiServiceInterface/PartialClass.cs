using myRaiData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using System.Web;


namespace MyRaiServiceInterface.MyRaiServiceReference1
{
    public enum AnagSource
    {
        FromCICS,
        FromHRGB,
        FromLDAP
    }
    public partial class MyRaiService1Client
    {
        public static string[] GetParametri ( string chiave )
        {
            var db = new digiGappEntities( );
            MyRai_ParametriSistema p = db.MyRai_ParametriSistema.FirstOrDefault( x => x.Chiave == chiave );
            if (p == null) return null;
            else
            {
                string[] parametri = new string[] { p.Valore1 , p.Valore2 };
                return parametri;
            }
        }

        public wApiUtilitypresenzeSettimanali_resp GetPresenzeSettimanaliProtected ( string matricola , string dataDa , string dataA , DateTime? data_inizio_validita , DateTime Dmin )
        {
            if ( data_inizio_validita != null && data_inizio_validita > Dmin )
            {
                Dmin = ( DateTime ) data_inizio_validita;
                try
                {
                    myRaiData.digiGappEntities db = new myRaiData.digiGappEntities( );
                    MyRai_LogAzioni a = new MyRai_LogAzioni( )
                    {
                        applicativo = "PORTALE" ,
                        data = DateTime.Now ,
                        descrizione_operazione = "DataInizioValidita maggiore della data di inizio per " + matricola + " " +
                        ( data_inizio_validita != null ? ( ( DateTime ) data_inizio_validita ).ToString( "dd/MM/yyyy" ) : "" ) ,
                        matricola = matricola ,
                        operazione = "WCF PresenzeSettimanali" ,
                        provenienza = "GetPresenzeSettimanaliProtected"
                    };
                    db.MyRai_LogAzioni.Add( a );
                    db.SaveChanges( );
                }
                catch ( Exception ex )
                {
                }
            }
            return PresenzeSettimanali( matricola , Dmin.ToString( "ddMMyyyy" ) , dataA );
        }

        private wApiUtilitydipendente_resp GetAnagraficaFromLDAP ( wApiUtilitydipendente_resp resp , string matricola )
        {
            string[] par = GetParametri( "AccountUtenteServizio" );

            using ( PrincipalContext pc = new PrincipalContext( ContextType.Domain , "RAI" , null , ContextOptions.Negotiate ,
               par[0] , par[1] ) )
            {

                //string nomeDom= System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                //if (nomeDom.Contains("\\"))
                //    matricola = nomeDom.Split('\\')[1];
                //else
                //    matricola = nomeDom;
                if (matricola.StartsWith("BP")) matricola = matricola.Replace("BP", "");

                UserPrincipal user = UserPrincipal.FindByIdentity( pc , IdentityType.SamAccountName , matricola );
                var db = new myRaiData.digiGappEntities( );
                MyRai_LogAzioni az = new myRaiData.MyRai_LogAzioni( )
                {
                    applicativo = "WCF" ,
                    data = DateTime.Now ,
                    descrizione_operazione = "LDAP" ,
                    matricola = matricola ,
                    operazione = "richiesta LDAP per :" + matricola
                };
                db.MyRai_LogAzioni.Add( az );
                db.SaveChanges( );

                if ( user == null )
                {
                    throw ( new Exception( "Impossibile reperire anagrafica da LDAP (matr:" + matricola + ")" ) );
                }
                else
                {
                    resp.data = new Utente( );
                    resp.data.nominativo = user.GivenName + " " + user.Surname;
                    resp.data.Cognome = user.Surname;
                    resp.data.Nome = user.GivenName;
                    resp.data.NomeProprio = user.GivenName;
                    resp.data.matricola = matricola;
                    return resp;
                }
            }
        }
        private wApiUtilitydipendente_resp GetAnagraficaFromHRGB ( wApiUtilitydipendente_resp resp , string matricola )
        {

            it.rai.servizi.hrgb.Service service = new it.rai.servizi.hrgb.Service( );
            it.rai.servizi.hrgb.Anagrafica an = service.EsponiAnagrafica_Net( "RAICV" , matricola );
            resp.data = new Utente( );
            if ( an == null || an.DT_Anagrafica == null || an.DT_Anagrafica.Rows.Count == 0 )
                throw ( new Exception( "Impossibile reperire anagrafica da HRGB (matr:" + matricola + ")" ) );

            if ( an.DT_Anagrafica.Rows[0]["Nome"] != null && an.DT_Anagrafica.Rows[0]["Cognome"] != null )
                resp.data.nominativo = an.DT_Anagrafica.Rows[0]["Nome"].ToString( ) + " " + an.DT_Anagrafica.Rows[0]["Cognome"].ToString( );

            if ( an.DT_Anagrafica.Rows[0]["Data_Assunzione"] != null )
                resp.data.data_inizio_rapporto_lavorativo = an.DT_Anagrafica.Rows[0]["Data_Assunzione"].ToString( );

            if ( an.DT_Anagrafica.Rows[0]["Data_Nascita"] != null )
                resp.data.data_nascita = an.DT_Anagrafica.Rows[0]["Data_Nascita"].ToString( );

            if ( an.DT_Anagrafica.Rows[0]["Cod_Mansione"] != null )
                resp.data.mansione = an.DT_Anagrafica.Rows[0]["Cod_Mansione"].ToString( );

            resp.data.matricola = matricola.PadLeft( 7 , '0' );
            return resp;

        }
        private void LogErrore ( string errore , string provenienza , string matricola )
        {
            var db = new digiGappEntities( );
            MyRai_LogErrori err = new MyRai_LogErrori( )
            {
                applicativo = "portale" ,
                data = DateTime.Now ,
                error_message = errore ,
                matricola = matricola ,
                provenienza = provenienza
            };
            db.MyRai_LogErrori.Add( err );
            db.SaveChanges( );
        }

        public wApiUtilitydipendente_resp GetRecuperaUtente ( string matricola , string data )
        {
            wApiUtilitydipendente_resp resp = new wApiUtilitydipendente_resp( );

            if ( HttpContext.Current.Session["Utente"] != null )
            {
                try
                {

                    var result = HttpContext.Current.Session["Utente"];
                    if ( ( ( wApiUtilitydipendente_resp ) result ).data.matricola.Equals( matricola ) )
                        return ( wApiUtilitydipendente_resp ) HttpContext.Current.Session["Utente"];
                }
                catch ( Exception ex )
                {
                    LogErrore( ex.ToString( ) , "GetRecuperaUtente WCF" , matricola.PadLeft( 7 , '0' ) );
                }
            }

            try
            {
                resp = recuperaUtente( matricola.PadLeft( 7 , '0' ) , data );
                if ( resp.esito == true && resp.data != null && !String.IsNullOrWhiteSpace( resp.data.nominativo ) )
                {
                    resp.data.SourceAnagrafica = AnagSource.FromCICS;

                    if ( HttpContext.Current.Session["Utente"] == null )
                    {
                        HttpContext.Current.Session["Utente"] = resp;
                    }
                    return resp;
                }
                else
                {
                    LogErrore( "Impossibile reperire anagrafica da WCF/CICS " + resp.errore , "GetRecuperaUtente WCF" , matricola.PadLeft( 7 , '0' ) );
                }
            }
            catch ( Exception ex )
            {
                LogErrore( ex.ToString( ) , "GetRecuperaUtente WCF" , matricola.PadLeft( 7 , '0' ) );
            }

            try
            {
                resp = GetAnagraficaFromHRGB( resp , ( matricola.Length == 7 && matricola.StartsWith( "0" ) ) ? matricola.Substring( 1 ) : matricola );
                resp.data.SourceAnagrafica = AnagSource.FromHRGB;

                if ( HttpContext.Current.Session["Utente"] == null )
                {
                    HttpContext.Current.Session["Utente"] = resp;
                }

                return resp;
            }
            catch ( Exception ex )
            {
                LogErrore( ex.ToString( ) , "GetRecuperaUtente HRGB" , matricola );
            }

            try
            {
                resp = GetAnagraficaFromLDAP( resp , ( matricola.Length == 7 && matricola.StartsWith( "0" ) ) ? "P" + matricola.Substring( 1 ) : matricola );
                resp.data.SourceAnagrafica = AnagSource.FromLDAP;
                HttpContext.Current.Session["Utente"] = resp;
                return resp;
            }
            catch ( Exception ex )
            {
                LogErrore( ex.ToString( ) , "GetRecuperaUtente LDAP" , matricola );
                throw ( new Exception( "Impossibile reperire anagrafica dai servizi (matr:" + matricola + ")" ) );
            }
        }

        public wApiUtilitydipendente_resp GetRecuperaUtenteNoSession ( string matricola , string data )
        {
            wApiUtilitydipendente_resp resp = new wApiUtilitydipendente_resp( );

            try
            {
                resp = recuperaUtente( matricola.PadLeft( 7 , '0' ) , data );
                if ( resp.esito == true && resp.data != null && !String.IsNullOrWhiteSpace( resp.data.nominativo ) )
                {
                    resp.data.SourceAnagrafica = AnagSource.FromCICS;
                    resp.data.nominativo = resp.data.nominativo.Replace( "\\" , " " );
                    return resp;
                }
                else
                {
                    LogErrore( "Impossibile reperire anagrafica da WCF/CICS " + resp.errore , "GetRecuperaUtente WCF" , matricola.PadLeft( 7 , '0' ) );
                }
            }
            catch ( Exception ex )
            {
                LogErrore( ex.ToString( ) , "GetRecuperaUtente WCF" , matricola.PadLeft( 7 , '0' ) );
            }

            try
            {
                resp = GetAnagraficaFromHRGB( resp , matricola );
                resp.data.SourceAnagrafica = AnagSource.FromHRGB;
                resp.data.nominativo = resp.data.nominativo.Replace( "\\" , " " );
                return resp;
            }
            catch ( Exception ex )
            {
                LogErrore( ex.ToString( ) , "GetRecuperaUtente HRGB" , matricola );
            }

            try
            {
                resp = GetAnagraficaFromLDAP( resp , matricola );
                resp.data.SourceAnagrafica = AnagSource.FromLDAP;
                resp.data.nominativo = resp.data.nominativo.Replace( "\\" , " " );
                return resp;
            }
            catch ( Exception ex )
            {
                LogErrore( ex.ToString( ) , "GetRecuperaUtente LDAP" , matricola );
                throw ( new Exception( "Impossibile reperire anagrafica dai servizi (matr:" + matricola + ")" ) );
            }
        }
    }

    public partial class InfoPresenza
    {
        public Boolean CopreOrario { get; set; }
    }
    public partial class Utente
    {
        //public Boolean IsAnagraficaFromCICS { get; set; }
        public string NomeProprio { get; set; }
        public AnagSource SourceAnagrafica { get; set; }
    }
  
}

namespace MyRaiServiceInterface.it.rai.servizi.digigappws
{
    public partial class Periodo
    {
        public string DeltaTotale_OldVersion { get; set; }
    }
    public partial class Timbratura
    {
        public string OraGiornoSuccessivo { get; set; }
    }
    public partial class Eccezione
    {
        public int IdAttivitaCeiton { get; set; }
        public string AttivitaCeiton { get; set; }
        public int IdStato { get; set; }
        public int IdEccezioneRichiesta { get; set; }
        public int IdRichiestaPadre { get; set; }
        public int? IdRaggruppamento { get; set; }
        public string MotivoRichiesta { get; set; }
        public string PeriodoRichiesta { get; set; }
        public bool StornoInRichiesta { get; set; }

        public bool EsisteStorno { get; set; }
        public int IdStatoStorno { get; set; }

        public bool IsUrgent { get; set; } // entro le 48 ore da ora
        public bool IsOverdue { get; set; } // richiesta scaduta e mai approvata


        public DateTime DataRichiesta { get; set; }
        public DateTime DataInserimento { get; set; }
        public int? IdDocumentoAssociato { get; set; }
        public int LivelloRichiedenteEccezione { get; set; }

        public int CaratteriObbligatoriNota { get; set; }
        public string CodiceTurno { get; set; }

        public string CodiceReparto { get; set; }
        public Boolean RichiedenteL1 { get; set; }
        public Boolean RichiedenteL2 { get; set; }

        public Boolean RichiedeAttivitaCeiton { get; set; }
        public bool IniettataDaRaiPerMe { get; set; }
        public bool RicalcolataDaRaiPerMe { get; set; }

        public bool no_corrispondenza_gapp { get; set; }

        public string qta_IpotesiNocarenza { get; set; }

        public string DescrizioneApprovatorePrimoLivello { get; set; }
        public string ApprovatoreSelezionato { get; set; }
        public string IdApprovatoreSelezionato { get; set; }

        public DateTime? DataVistoPositivo { get; set; }
        public DateTime? DataVistoNegativo { get; set; }

         
        public string EccezioneSostitutivaCodice{ get; set; }
        public string EccezioneSostitutivaDalle { get; set; }
        public string EccezioneSostitutivaAlle { get; set; }
        public bool   EccezioneSostitutivaSWH { get; set; }

        // true se la richiesta ha una nota da o per la segreteria
        public bool HasNotaSegreteria { get; set; }
    }

    public partial class dayResponse
    {
        public Boolean HideRefresh { get; set; }
        public OrarioUscitaModel OrarioInBaseAingresso { get; set; }

        public bool ContieneEccezione ( string e )
        {
            return this.eccezioni != null && this.eccezioni.Any( ) && this.eccezioni.Any( x => x.cod.Trim( ) == e );
        }
    }
    public partial class Giornata
    {
        public string Provenienza { get; set; }
        public string Frazione { get; set; }

        public string StatoDescrizione { get; set; }
        public int Stato { get; set; }

        public Boolean ShowDelete { get; set; }

    }
    public partial class pianoFerie
    {
        public int? anno { get; set; }
    }

    //public partial class Ferie
    //{
    //	/// <summary>
    //	/// MN
    //	/// </summary>
    //	public float mancatoNonLavorato { get; set; }

    //	/// <summary>
    //	/// PXC
    //	/// </summary>
    //	public int permRecuperoGiornalisti { get; set; }
    //}

    public partial class Evidenza
    {
        public string CarenzaEffettiva { get; set; }
        public Boolean IsQuadrabileDaDipendente { get; set; }
        public int IdSWdaStornare { get; set; }
    }
}

namespace MyRaiServiceInterface.MyRaiServiceReference1
{
    public partial class Viaggio
    {
        public string Stato { get; set; }
        public DateTime DataFromDB { get; set; }
        public decimal AnticipoFromDB { get; set; }
        public decimal RimborsoFromDB { get; set; }
        public decimal SpesaPrevistaFromDB { get; set; }

        public DateTime DataArrivoFromDB { get; set; }
    }
}