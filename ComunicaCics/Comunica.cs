
using System;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using myRaiData;

namespace ComunicaCics
{
    //necessaria per far rimanere la stessa identica chiamata dal software gia esistente
    public class ComunicaVersoCics : ComunicaVersoCicsTrick
    {
        public ComunicaVersoCics()
        {

        }
    }


    public class ComunicaVersoCicsTrick
    {
        public object ComunicaVersoCics(string StrFunzione)
        {
            /*
             * File necessario per sovrascrivere URL  - filename 'ComunicaOptions.xml' stessa cartella della DLL
             * 
             <ComunicaCics>
	            <UrlWcf>http://svildigigapp-his.intranet.rai.it/ComunicaCics.svc</UrlWcf>
             </ComunicaCics>
             */

            /* oppure aggiungere seguente key nel web.config :
             <add key="UrlWcf" value="http://svildigigapp-his.intranet.rai.it/ComunicaCics.svc" />
             */
            try
            {
                try
                {
                    ScriviLogIn( StrFunzione );
                }
                catch ( Exception ex )
                {

                }

                string DefaultEndpoint = "http://svildigigapp-his-rm.intranet.rai.it/ComunicaCics.svc";

                string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string xmlFileName = Path.Combine(assemblyFolder, "ComunicaOptions.xml");

                if (System.Configuration.ConfigurationManager.AppSettings["UrlWcf"] != null)
                {
                    DefaultEndpoint = System.Configuration.ConfigurationManager.AppSettings["UrlWcf"].ToString();
                }
                else if (System.IO.File.Exists(xmlFileName))
                {
                    XElement xelement = XElement.Load(xmlFileName);
                    DefaultEndpoint = xelement.Element("UrlWcf").Value;
                }
               

                EndpointAddress remoteAddress = new EndpointAddress(DefaultEndpoint);

                BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.TransportCredentialOnly);
                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;

                HisServiceReference.Service1Client client = new HisServiceReference.Service1Client(binding, remoteAddress);
                client.ClientCredentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
                string response = client.ComunicaCics(StrFunzione);

                try
                {
                    ScriviLogOut( response );
                }
                catch(Exception ex)
                {

                }
                return response;
            }
            catch (Exception ex)
            {
                return "Error:" + ex.Message;
            }
        }


        private void ScriviLogIn ( string msg )
        {
            bool result = false;

            if ( !String.IsNullOrEmpty( msg ) )
            {
                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    // reperimento dei parametri
                    var parametri = db.MyRai_ParametriSistema.Where( w => w.Chiave.Equals( "AbilitaLogComunicaCics" ) ).FirstOrDefault( );

                    if ( parametri != null )
                    {
                        if ( !String.IsNullOrEmpty( parametri.Valore1 ) )
                        {
                            var sContinua = parametri.Valore1.Trim( );
                            try
                            {
                                result = bool.Parse( sContinua );
                            }
                            catch ( Exception ex )
                            {
                                result = false;
                            }
                        }
                    }

                    if (result)
                    {
                        db.MyRai_LogAzioni.Add( new MyRai_LogAzioni( )
                        {
                            matricola = "FRANCESCO" ,
                            data = DateTime.Now ,
                            operazione = "LOGCOMUNICACICS_IN" ,
                            applicativo = "Portale" ,
                            descrizione_operazione = msg ,
                            provenienza = "DLL_COMUNICACICS"
                        } );
                        db.SaveChanges( );
                    }
                }
            }
        }

        private void ScriviLogOut ( string msg )
        {
            bool result = false;

            if ( !String.IsNullOrEmpty( msg ) )
            {
                string toWrite = msg.Replace( Convert.ToChar( 0x0 ).ToString( ) , " " );

                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    // reperimento dei parametri
                    var parametri = db.MyRai_ParametriSistema.Where( w => w.Chiave.Equals( "AbilitaLogComunicaCics" ) ).FirstOrDefault( );

                    if ( parametri != null )
                    {
                        if ( !String.IsNullOrEmpty( parametri.Valore1 ) )
                        {
                            var sContinua = parametri.Valore1.Trim( );
                            try
                            {
                                result = bool.Parse( sContinua );
                            }
                            catch ( Exception ex )
                            {
                                result = false;
                            }
                        }
                    }

                    if ( result )
                    {
                        db.MyRai_LogAzioni.Add( new MyRai_LogAzioni( )
                        {
                            matricola = "FRANCESCO" ,
                            data = DateTime.Now ,
                            operazione = "LOGCOMUNICACICS_OUT" ,
                            applicativo = "Portale" ,
                            descrizione_operazione = toWrite ,
                            provenienza = "DLL_COMUNICACICS"
                        } );
                        db.SaveChanges( );
                    }
                }
            }
        }
    }
}