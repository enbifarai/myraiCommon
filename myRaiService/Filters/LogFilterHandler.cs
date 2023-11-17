using myRaiData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Web;

namespace myRaiService.Filters
{
    public class UserLogger : IParameterInspector
    {
        public void AfterCall ( string operationName , object[] outputs , object returnValue , object correlationState )
        {
            // se operationName == Get è l'init che non va loggato
            if ( operationName.Equals( "Get" ) )
            {
                return;
            }

            var callModel = returnValue as Exception;
            try
            {
                OperationContext operationContext = OperationContext.Current;
                ServiceSecurityContext securityContext = ServiceSecurityContext.Current;

                string user = null;
                bool isAnonymous = true;

                if ( securityContext != null )
                {
                    user = securityContext.PrimaryIdentity.Name;
                    isAnonymous = securityContext.IsAnonymous;
                }

                Uri remoteAddress = operationContext.Channel.LocalAddress.Uri;
                string sessionId = operationContext.SessionId;
                MessageVersion messageVersion = operationContext.IncomingMessageVersion;

                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    //MyRai_LogWCF log = new MyRai_LogWCF( );
                    //log.Date = DateTime.Now;

                    //if ( callModel != null)
                    //{
                    //    log.Exception = callModel.StackTrace;
                    //    log.Message = "Termine richiesta - " + callModel.Message;
                    //}
                    //else
                    //{
                    //    log.Exception = "";
                    //    log.Message = "Termine richiesta";
                    //}
                    //log.Thread = remoteAddress.OriginalString;
                    //log.Level = String.IsNullOrEmpty( sessionId ) ? "" : sessionId;

                    //string pars = "";
                    //foreach ( object output in outputs )
                    //    pars += " " + outputs;

                    //if ( !String.IsNullOrEmpty( pars.Trim( ) ) )
                    //{
                    //    log.Logger = operationName + " parametri" + pars;
                    //}
                    //else
                    //{
                    //    log.Logger = operationName;
                    //}

                    //log.Logger = String.IsNullOrEmpty( log.Logger ) ? "" : log.Logger;

                    //db.MyRai_LogWCF.Add( log );
                    //db.SaveChanges( );
                }
            }
            catch ( Exception ex )
            {

            }
        }

        public object BeforeCall ( string operationName , object[] inputs )
        {
            try
            {
                // se operationName == Get è l'init che non va loggato
                if ( operationName.Equals( "Get" ) )
                {
                    return null;
                }

                OperationContext operationContext = OperationContext.Current;
                ServiceSecurityContext securityContext = ServiceSecurityContext.Current;

                string user = null;
                bool isAnonymous = true;

                if ( securityContext != null )
                {
                    user = securityContext.PrimaryIdentity.Name;
                    isAnonymous = securityContext.IsAnonymous;
                }

                Uri remoteAddress = operationContext.Channel.LocalAddress.Uri;
                string sessionId = operationContext.SessionId;
                MessageVersion messageVersion = operationContext.IncomingMessageVersion;

                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    //MyRai_LogWCF log = new MyRai_LogWCF( );
                    //log.Date = DateTime.Now;
                    //log.Exception = "";
                    //log.Message = "Avvio richiesta";
                    //log.Thread = remoteAddress.OriginalString;
                    //log.Level = String.IsNullOrEmpty( sessionId ) ? "" : sessionId;

                    //string pars = "";
                    //foreach ( object input in inputs )
                    //    pars += " " + input;

                    //if ( !String.IsNullOrEmpty( pars.Trim( ) ) )
                    //{
                    //    log.Logger = operationName + " parametri" + pars;
                    //}
                    //else
                    //{
                    //    log.Logger = operationName;
                    //}

                    //log.Logger = String.IsNullOrEmpty( log.Logger ) ? "" : log.Logger;

                    //db.MyRai_LogWCF.Add( log );
                    //db.SaveChanges( );
                }
            }
            catch ( Exception ex )
            {

            }

            return null;
        }
    }
}