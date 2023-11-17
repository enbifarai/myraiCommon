﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by Microsoft.VSDesigner, Version 4.0.30319.42000.
// 
#pragma warning disable 1591

namespace myRaiGestionale.it.rai.servizi.raiconnectcoll {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.ComponentModel;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.4084.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="rai_ruo_wsSoap", Namespace="http://tempuri.org/")]
    public partial class rai_ruo_ws : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback creaProtocolloOperationCompleted;
        
        private System.Threading.SendOrPostCallback inserisciAllegatoOperationCompleted;
        
        private System.Threading.SendOrPostCallback eseguiRicercaOperationCompleted;
        
        private System.Threading.SendOrPostCallback confermaSpedizioniOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public rai_ruo_ws() {
            this.Url = global::myRaiGestionale.Properties.Settings.Default.myRaiGestionale_it_rai_servizi_raiconnectcoll_rai_ruo_ws;
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event creaProtocolloCompletedEventHandler creaProtocolloCompleted;
        
        /// <remarks/>
        public event inserisciAllegatoCompletedEventHandler inserisciAllegatoCompleted;
        
        /// <remarks/>
        public event eseguiRicercaCompletedEventHandler eseguiRicercaCompleted;
        
        /// <remarks/>
        public event confermaSpedizioniCompletedEventHandler confermaSpedizioniCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/creaProtocollo", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string creaProtocollo(string id_ruolo, string matricola, string strXMLMetadati) {
            object[] results = this.Invoke("creaProtocollo", new object[] {
                        id_ruolo,
                        matricola,
                        strXMLMetadati});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void creaProtocolloAsync(string id_ruolo, string matricola, string strXMLMetadati) {
            this.creaProtocolloAsync(id_ruolo, matricola, strXMLMetadati, null);
        }
        
        /// <remarks/>
        public void creaProtocolloAsync(string id_ruolo, string matricola, string strXMLMetadati, object userState) {
            if ((this.creaProtocolloOperationCompleted == null)) {
                this.creaProtocolloOperationCompleted = new System.Threading.SendOrPostCallback(this.OncreaProtocolloOperationCompleted);
            }
            this.InvokeAsync("creaProtocollo", new object[] {
                        id_ruolo,
                        matricola,
                        strXMLMetadati}, this.creaProtocolloOperationCompleted, userState);
        }
        
        private void OncreaProtocolloOperationCompleted(object arg) {
            if ((this.creaProtocolloCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.creaProtocolloCompleted(this, new creaProtocolloCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/inserisciAllegato", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string inserisciAllegato(string id_ruolo, string matricola, string id_documento, string nome_file, string descrizione_file, string base64file, string file_principale, string id_attach) {
            object[] results = this.Invoke("inserisciAllegato", new object[] {
                        id_ruolo,
                        matricola,
                        id_documento,
                        nome_file,
                        descrizione_file,
                        base64file,
                        file_principale,
                        id_attach});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void inserisciAllegatoAsync(string id_ruolo, string matricola, string id_documento, string nome_file, string descrizione_file, string base64file, string file_principale, string id_attach) {
            this.inserisciAllegatoAsync(id_ruolo, matricola, id_documento, nome_file, descrizione_file, base64file, file_principale, id_attach, null);
        }
        
        /// <remarks/>
        public void inserisciAllegatoAsync(string id_ruolo, string matricola, string id_documento, string nome_file, string descrizione_file, string base64file, string file_principale, string id_attach, object userState) {
            if ((this.inserisciAllegatoOperationCompleted == null)) {
                this.inserisciAllegatoOperationCompleted = new System.Threading.SendOrPostCallback(this.OninserisciAllegatoOperationCompleted);
            }
            this.InvokeAsync("inserisciAllegato", new object[] {
                        id_ruolo,
                        matricola,
                        id_documento,
                        nome_file,
                        descrizione_file,
                        base64file,
                        file_principale,
                        id_attach}, this.inserisciAllegatoOperationCompleted, userState);
        }
        
        private void OninserisciAllegatoOperationCompleted(object arg) {
            if ((this.inserisciAllegatoCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.inserisciAllegatoCompleted(this, new inserisciAllegatoCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/eseguiRicerca", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string eseguiRicerca(string id_ruolo, string matricola, string id_ricerca, string top, string filtro_ricerca) {
            object[] results = this.Invoke("eseguiRicerca", new object[] {
                        id_ruolo,
                        matricola,
                        id_ricerca,
                        top,
                        filtro_ricerca});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void eseguiRicercaAsync(string id_ruolo, string matricola, string id_ricerca, string top, string filtro_ricerca) {
            this.eseguiRicercaAsync(id_ruolo, matricola, id_ricerca, top, filtro_ricerca, null);
        }
        
        /// <remarks/>
        public void eseguiRicercaAsync(string id_ruolo, string matricola, string id_ricerca, string top, string filtro_ricerca, object userState) {
            if ((this.eseguiRicercaOperationCompleted == null)) {
                this.eseguiRicercaOperationCompleted = new System.Threading.SendOrPostCallback(this.OneseguiRicercaOperationCompleted);
            }
            this.InvokeAsync("eseguiRicerca", new object[] {
                        id_ruolo,
                        matricola,
                        id_ricerca,
                        top,
                        filtro_ricerca}, this.eseguiRicercaOperationCompleted, userState);
        }
        
        private void OneseguiRicercaOperationCompleted(object arg) {
            if ((this.eseguiRicercaCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.eseguiRicercaCompleted(this, new eseguiRicercaCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/confermaSpedizioni", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string confermaSpedizioni(string id_ruolo, string matricola, string id_documento) {
            object[] results = this.Invoke("confermaSpedizioni", new object[] {
                        id_ruolo,
                        matricola,
                        id_documento});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void confermaSpedizioniAsync(string id_ruolo, string matricola, string id_documento) {
            this.confermaSpedizioniAsync(id_ruolo, matricola, id_documento, null);
        }
        
        /// <remarks/>
        public void confermaSpedizioniAsync(string id_ruolo, string matricola, string id_documento, object userState) {
            if ((this.confermaSpedizioniOperationCompleted == null)) {
                this.confermaSpedizioniOperationCompleted = new System.Threading.SendOrPostCallback(this.OnconfermaSpedizioniOperationCompleted);
            }
            this.InvokeAsync("confermaSpedizioni", new object[] {
                        id_ruolo,
                        matricola,
                        id_documento}, this.confermaSpedizioniOperationCompleted, userState);
        }
        
        private void OnconfermaSpedizioniOperationCompleted(object arg) {
            if ((this.confermaSpedizioniCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.confermaSpedizioniCompleted(this, new confermaSpedizioniCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.4084.0")]
    public delegate void creaProtocolloCompletedEventHandler(object sender, creaProtocolloCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.4084.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class creaProtocolloCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal creaProtocolloCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.4084.0")]
    public delegate void inserisciAllegatoCompletedEventHandler(object sender, inserisciAllegatoCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.4084.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class inserisciAllegatoCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal inserisciAllegatoCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.4084.0")]
    public delegate void eseguiRicercaCompletedEventHandler(object sender, eseguiRicercaCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.4084.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class eseguiRicercaCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal eseguiRicercaCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.4084.0")]
    public delegate void confermaSpedizioniCompletedEventHandler(object sender, confermaSpedizioniCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.4084.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class confermaSpedizioniCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal confermaSpedizioniCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591