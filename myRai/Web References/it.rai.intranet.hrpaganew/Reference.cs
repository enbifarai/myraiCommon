﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Il codice è stato generato da uno strumento.
//     Versione runtime:4.0.30319.42000
//
//     Le modifiche apportate a questo file possono provocare un comportamento non corretto e andranno perse se
//     il codice viene rigenerato.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// Il codice sorgente è stato generato automaticamente da Microsoft.VSDesigner, versione 4.0.30319.42000.
// 
#pragma warning disable 1591

namespace myRai.it.rai.intranet.hrpaganew {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.ComponentModel;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="ComunicaSoap", Namespace="DocComunica")]
    public partial class Comunica : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback ElencoNotificheDocumentiOperationCompleted;
        
        private System.Threading.SendOrPostCallback ElencoUltimaBustaOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public Comunica() {
            this.Url = global::myRai.Properties.Settings.Default.myRai_it_rai_intranet_hrpaganew_Comunica;
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
        public event ElencoNotificheDocumentiCompletedEventHandler ElencoNotificheDocumentiCompleted;
        
        /// <remarks/>
        public event ElencoUltimaBustaCompletedEventHandler ElencoUltimaBustaCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("DocComunica/ElencoNotificheDocumenti", RequestNamespace="DocComunica", ResponseNamespace="DocComunica", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public ListaNotificheDocumenti ElencoNotificheDocumenti(string parametro_01, string parametro_02, string parametro_03) {
            object[] results = this.Invoke("ElencoNotificheDocumenti", new object[] {
                        parametro_01,
                        parametro_02,
                        parametro_03});
            return ((ListaNotificheDocumenti)(results[0]));
        }
        
        /// <remarks/>
        public void ElencoNotificheDocumentiAsync(string parametro_01, string parametro_02, string parametro_03) {
            this.ElencoNotificheDocumentiAsync(parametro_01, parametro_02, parametro_03, null);
        }
        
        /// <remarks/>
        public void ElencoNotificheDocumentiAsync(string parametro_01, string parametro_02, string parametro_03, object userState) {
            if ((this.ElencoNotificheDocumentiOperationCompleted == null)) {
                this.ElencoNotificheDocumentiOperationCompleted = new System.Threading.SendOrPostCallback(this.OnElencoNotificheDocumentiOperationCompleted);
            }
            this.InvokeAsync("ElencoNotificheDocumenti", new object[] {
                        parametro_01,
                        parametro_02,
                        parametro_03}, this.ElencoNotificheDocumentiOperationCompleted, userState);
        }
        
        private void OnElencoNotificheDocumentiOperationCompleted(object arg) {
            if ((this.ElencoNotificheDocumentiCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.ElencoNotificheDocumentiCompleted(this, new ElencoNotificheDocumentiCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("DocComunica/ElencoUltimaBusta", RequestNamespace="DocComunica", ResponseNamespace="DocComunica", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public ListaNotificheDocumenti ElencoUltimaBusta(string parametro_01, string parametro_02, string parametro_03) {
            object[] results = this.Invoke("ElencoUltimaBusta", new object[] {
                        parametro_01,
                        parametro_02,
                        parametro_03});
            return ((ListaNotificheDocumenti)(results[0]));
        }
        
        /// <remarks/>
        public void ElencoUltimaBustaAsync(string parametro_01, string parametro_02, string parametro_03) {
            this.ElencoUltimaBustaAsync(parametro_01, parametro_02, parametro_03, null);
        }
        
        /// <remarks/>
        public void ElencoUltimaBustaAsync(string parametro_01, string parametro_02, string parametro_03, object userState) {
            if ((this.ElencoUltimaBustaOperationCompleted == null)) {
                this.ElencoUltimaBustaOperationCompleted = new System.Threading.SendOrPostCallback(this.OnElencoUltimaBustaOperationCompleted);
            }
            this.InvokeAsync("ElencoUltimaBusta", new object[] {
                        parametro_01,
                        parametro_02,
                        parametro_03}, this.ElencoUltimaBustaOperationCompleted, userState);
        }
        
        private void OnElencoUltimaBustaOperationCompleted(object arg) {
            if ((this.ElencoUltimaBustaCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.ElencoUltimaBustaCompleted(this, new ElencoUltimaBustaCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.7.3056.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="DocComunica")]
    public partial class ListaNotificheDocumenti {
        
        private string stringaErroreField;
        
        private int esitoField;
        
        private string notificheField;
        
        /// <remarks/>
        public string StringaErrore {
            get {
                return this.stringaErroreField;
            }
            set {
                this.stringaErroreField = value;
            }
        }
        
        /// <remarks/>
        public int Esito {
            get {
                return this.esitoField;
            }
            set {
                this.esitoField = value;
            }
        }
        
        /// <remarks/>
        public string Notifiche {
            get {
                return this.notificheField;
            }
            set {
                this.notificheField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    public delegate void ElencoNotificheDocumentiCompletedEventHandler(object sender, ElencoNotificheDocumentiCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ElencoNotificheDocumentiCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal ElencoNotificheDocumentiCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public ListaNotificheDocumenti Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((ListaNotificheDocumenti)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    public delegate void ElencoUltimaBustaCompletedEventHandler(object sender, ElencoUltimaBustaCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ElencoUltimaBustaCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal ElencoUltimaBustaCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public ListaNotificheDocumenti Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((ListaNotificheDocumenti)(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591