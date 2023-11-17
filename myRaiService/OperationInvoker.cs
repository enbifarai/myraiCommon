using myRaiData;
using myRaiService.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace myRaiService
{
    public class LoggingOperationInvoker : IOperationInvoker
    {
        IOperationInvoker _baseInvoker;
        string _operationName;

        public LoggingOperationInvoker(IOperationInvoker baseInvoker, DispatchOperation operation)
        {
            _baseInvoker = baseInvoker;
            _operationName = operation.Name;
        }
        public bool IsSynchronous
        {
            get { return _baseInvoker.IsSynchronous; }

        }
        public object[] AllocateInputs()
        {
            return _baseInvoker.AllocateInputs();
        }
        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            return _baseInvoker.InvokeBegin(instance, inputs, callback, state);
        }

        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            return _baseInvoker.InvokeEnd(instance, out outputs, result);
        }
      

        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {

            string action = GetAction();
            string body = GetBody();
            try
            {
                LogWCFAction( action , body );

                if (IsMonitorized(action)) LogActionCall(action, body);

                object resp= _baseInvoker.Invoke(instance, inputs, out outputs);

                string respSerialized = Serialize( resp );

                if (IsMonitorized(action))
                {
                    LogActionResponse(action,respSerialized);
                }
                
                LogWCFActionResponse( action , respSerialized );

                return resp;
            }
            catch (Exception ex)
            {
                LogActionError(action, body, ex.ToString());
                LogWCFError( action , body , ex );
                outputs = null;
                return null;
            }
        }
        public  string Serialize(object v)
        {
            try
            {
                XmlSerializer xmlserializer = new XmlSerializer(v.GetType());
                StringWriter stringWriter = new StringWriter();
                XmlWriter writer = XmlWriter.Create(stringWriter);

                xmlserializer.Serialize(writer, v);

                return stringWriter.ToString();
            }
            catch  
            {
                return null;
            }
        }
        
        public string GetBody()
        {
            try
            {
                return OperationContext.Current.RequestContext.RequestMessage.ToString();
            }
            catch
            {
                return null;
            }

        }
        public string GetAction()
        {
            try
            {
                string header = OperationContext.Current.RequestContext.RequestMessage.Headers.Action;
                if (!String.IsNullOrWhiteSpace(header))
                    return header.Split('/').Last();
            }
            catch
            {
                return null;
            }
            return null;
        }
        public void LogActionCall(string action, string body)
        {
            var db = new myRaiData.digiGappEntities();
            myRaiData.MyRai_LogAzioni az = new myRaiData.MyRai_LogAzioni()
            {
                applicativo = "WCFservice",
                data = DateTime.Now,
                matricola = "",
                operazione = action+"-REQ" ,
                descrizione_operazione = "Call params:" + body,
                provenienza = Environment.MachineName

            };
            db.MyRai_LogAzioni.Add(az);
            try
            {
                db.SaveChanges();
            }
            catch
            {
            }
        }

        public void LogWCFAction ( string action , string body )
        {
            try
            {
                if (action == "Get")
                {
                    return;
                }
                using ( digiGappEntities dbWCF = new digiGappEntities( ) )
                {
                    bool logAttivo = false;
                    var par = dbWCF.MyRai_ParametriSistema.Where( w => w.Chiave.Equals( "AttivaLogWCF" ) ).FirstOrDefault( );
                    if ( par != null )
                    {
                        logAttivo = par.Valore1.ToUpper( ).Equals( "TRUE" );
                    }

                    MyRai_LogWCF log = new MyRai_LogWCF( );
                    log.Data = DateTime.Now;
                    log.Messaggio = null;
                    log.StackTrace = null;
                    log.Parametri = "call parameters: " + body;
                    log.Azione = action + "-REQ";
                    log.Caller = Environment.MachineName;

                    dbWCF.MyRai_LogWCF.Add( log );

                    if (logAttivo)
                    {
                        dbWCF.SaveChanges( );
                    }
                }
            }
            catch ( Exception ex2 )
            {

            }
        }

        public void LogActionResponse(string action,string response)
        {
            var db = new myRaiData.digiGappEntities();
            myRaiData.MyRai_LogAzioni az = new myRaiData.MyRai_LogAzioni()
            {
                applicativo = "WCFservice",
                data = DateTime.Now,
                matricola = "",
                operazione = action+"-RESP",
                descrizione_operazione =  response,
                provenienza = Environment.MachineName

            };
            db.MyRai_LogAzioni.Add(az);
            try
            {
                db.SaveChanges();
            }
            catch
            {
            }
        }

        public void LogWCFActionResponse ( string action , string body )
        {
            try
            {
                if ( action == "Get" )
                {
                    return;
                }
                using ( digiGappEntities dbWCF = new digiGappEntities( ) )
                {
                    bool logAttivo = false;
                    var par = dbWCF.MyRai_ParametriSistema.Where( w => w.Chiave.Equals( "AttivaLogWCF" ) ).FirstOrDefault( );
                    if ( par != null )
                    {
                        logAttivo = par.Valore1.ToUpper( ).Equals( "TRUE" );
                    }

                    MyRai_LogWCF log = new MyRai_LogWCF( );
                    log.Data = DateTime.Now;
                    log.Messaggio = null;
                    log.StackTrace = null;
                    log.Parametri = "call parameters: " + body;
                    log.Azione = action + "-RESP";
                    log.Caller = Environment.MachineName;

                    dbWCF.MyRai_LogWCF.Add( log );

                    if ( logAttivo )
                    {
                        dbWCF.SaveChanges( );
                    }
                }
            }
            catch ( Exception ex2 )
            {

            }
        }

        public void LogActionError(string action, string body, string error)
        {
            var db = new myRaiData.digiGappEntities();
            myRaiData.MyRai_LogErrori err = new myRaiData.MyRai_LogErrori()
            {
                applicativo = "WCFservice",
                data = DateTime.Now,
                error_message = error + " Call params:" + body,
                matricola = "",
                provenienza =Environment.MachineName +"-"+action

            };
            db.MyRai_LogErrori.Add(err);
            try
            {
                db.SaveChanges();
            }
            catch
            {
            }
        }

        public void LogWCFError ( string action , string body , Exception ex )
        {
            try
            {
                if ( action == "Get" )
                {
                    return;
                }
                using ( digiGappEntities dbWCF = new digiGappEntities( ) )
                {
                    bool logAttivo = false;
                    var par = dbWCF.MyRai_ParametriSistema.Where( w => w.Chiave.Equals( "AttivaLogWCF" ) ).FirstOrDefault( );
                    if ( par != null )
                    {
                        logAttivo = par.Valore1.ToUpper( ).Equals( "TRUE" );
                    }
                    MyRai_LogWCF log = new MyRai_LogWCF( );
                    log.Data = DateTime.Now;
                    log.Messaggio = ex.Message;
                    log.StackTrace = ex.StackTrace;
                    log.Parametri = "call parameters: " + body;
                    log.Azione = action + "-ERR";
                    log.Caller = Environment.MachineName;

                    dbWCF.MyRai_LogWCF.Add( log );
                    if ( logAttivo )
                    {
                        dbWCF.SaveChanges( );
                    }
                }
            }
            catch ( Exception ex2 )
            {

            }
        }

        public Boolean IsMonitorized(string action)
        {
            if (String.IsNullOrWhiteSpace(action)) return false;
            var db = new myRaiData.digiGappEntities();
            var par=db.MyRai_ParametriSistema.Where(x => x.Chiave == "WcfDebugAction").FirstOrDefault();
            if (par == null || par.Valore1 ==null ) return false;
            return (par.Valore1.ToUpper().Trim() == action.ToUpper().Trim() || par.Valore1=="*") ;
        }

    }
    public class LoggingOperationBehavior : IOperationBehavior
    {
        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            dispatchOperation.Invoker = new LoggingOperationInvoker(dispatchOperation.Invoker, dispatchOperation);
        }

        // (TODO stub implementations)

        public void AddBindingParameters(OperationDescription operationDescription, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
            //throw new NotImplementedException();
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
            // throw new NotImplementedException();
        }

        public void Validate(OperationDescription operationDescription)
        {
            //throw new NotImplementedException();
        }
    }

    public class UserLoggingAttribute : Attribute, IServiceBehavior
    {
        public void AddBindingParameters ( ServiceDescription serviceDescription , ServiceHostBase serviceHostBase , System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints , System.ServiceModel.Channels.BindingParameterCollection bindingParameters )
        {
            //throw new NotImplementedException();
        }

        public void ApplyDispatchBehavior ( ServiceDescription serviceDescription , ServiceHostBase serviceHostBase )
        {
            IParameterInspector parameterInspector = new UserLogger( );

            foreach ( ChannelDispatcher dispatcher in serviceHostBase.ChannelDispatchers )
            {
                foreach ( EndpointDispatcher endpointDispatcher in dispatcher.Endpoints )
                {
                    DispatchRuntime dispatchRuntime = endpointDispatcher.DispatchRuntime;
                    IEnumerable<DispatchOperation> dispatchOperations = dispatchRuntime.Operations;

                    foreach ( DispatchOperation dispatchOperation in dispatchOperations )
                    {
                        dispatchOperation.ParameterInspectors.Add( parameterInspector );
                    }
                }
            }
        }

        public void Validate ( ServiceDescription serviceDescription , ServiceHostBase serviceHostBase )
        {
        }
    }

    public class ServiceLoggingBehavior : Attribute, IServiceBehavior
    {
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ServiceEndpoint endpoint in serviceDescription.Endpoints)
            {
                foreach (OperationDescription operation in endpoint.Contract.Operations)
                {
                    IOperationBehavior behavior = new LoggingOperationBehavior();
                    operation.Behaviors.Add(behavior);
                }
            }
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
            //throw new NotImplementedException();
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            //throw new NotImplementedException();
        }
    }



}