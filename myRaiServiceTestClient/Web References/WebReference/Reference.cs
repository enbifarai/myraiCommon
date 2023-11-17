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

namespace myRaiServiceTestClient.WebReference {
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
    [System.Web.Services.WebServiceBindingAttribute(Name="firmaStandardPortBinding", Namespace="http://firma.cnai.firmaremota.itagile.it/")]
    public partial class firmaStandard : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback signPDFOperationCompleted;
        
        private System.Threading.SendOrPostCallback signDigestOperationCompleted;
        
        private System.Threading.SendOrPostCallback getGraphicImageOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public firmaStandard() {
            this.Url = global::myRaiServiceTestClient.Properties.Settings.Default.myRaiServiceTestClient_WebReference_firmaStandard;
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
        public event signPDFCompletedEventHandler signPDFCompleted;
        
        /// <remarks/>
        public event signDigestCompletedEventHandler signDigestCompleted;
        
        /// <remarks/>
        public event getGraphicImageCompletedEventHandler getGraphicImageCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("", RequestNamespace="http://cosign.itagile.it", ResponseNamespace="http://cosign.itagile.it", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("signPDFReturn", DataType="base64Binary")]
        public byte[] signPDF(
                    string userid, 
                    string password, 
                    string pinToSign, 
                    [System.Xml.Serialization.XmlElementAttribute(DataType="base64Binary")] byte[] source, 
                    string fieldName, 
                    int page, 
                    int x, 
                    int y, 
                    int width, 
                    int height, 
                    string userName, 
                    string reason, 
                    string location, 
                    string dateFormat, 
                    bool graphic, 
                    string text, 
                    int fontSize) {
            object[] results = this.Invoke("signPDF", new object[] {
                        userid,
                        password,
                        pinToSign,
                        source,
                        fieldName,
                        page,
                        x,
                        y,
                        width,
                        height,
                        userName,
                        reason,
                        location,
                        dateFormat,
                        graphic,
                        text,
                        fontSize});
            return ((byte[])(results[0]));
        }
        
        /// <remarks/>
        public void signPDFAsync(
                    string userid, 
                    string password, 
                    string pinToSign, 
                    byte[] source, 
                    string fieldName, 
                    int page, 
                    int x, 
                    int y, 
                    int width, 
                    int height, 
                    string userName, 
                    string reason, 
                    string location, 
                    string dateFormat, 
                    bool graphic, 
                    string text, 
                    int fontSize) {
            this.signPDFAsync(userid, password, pinToSign, source, fieldName, page, x, y, width, height, userName, reason, location, dateFormat, graphic, text, fontSize, null);
        }
        
        /// <remarks/>
        public void signPDFAsync(
                    string userid, 
                    string password, 
                    string pinToSign, 
                    byte[] source, 
                    string fieldName, 
                    int page, 
                    int x, 
                    int y, 
                    int width, 
                    int height, 
                    string userName, 
                    string reason, 
                    string location, 
                    string dateFormat, 
                    bool graphic, 
                    string text, 
                    int fontSize, 
                    object userState) {
            if ((this.signPDFOperationCompleted == null)) {
                this.signPDFOperationCompleted = new System.Threading.SendOrPostCallback(this.OnsignPDFOperationCompleted);
            }
            this.InvokeAsync("signPDF", new object[] {
                        userid,
                        password,
                        pinToSign,
                        source,
                        fieldName,
                        page,
                        x,
                        y,
                        width,
                        height,
                        userName,
                        reason,
                        location,
                        dateFormat,
                        graphic,
                        text,
                        fontSize}, this.signPDFOperationCompleted, userState);
        }
        
        private void OnsignPDFOperationCompleted(object arg) {
            if ((this.signPDFCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.signPDFCompleted(this, new signPDFCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("", RequestNamespace="http://cosign.itagile.it", ResponseNamespace="http://cosign.itagile.it", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("signDigestReturn", DataType="base64Binary")]
        public byte[] signDigest(string utente, string password, string pinToSign, [System.Xml.Serialization.XmlElementAttribute(DataType="base64Binary")] byte[] digest, string digestAlgorithm) {
            object[] results = this.Invoke("signDigest", new object[] {
                        utente,
                        password,
                        pinToSign,
                        digest,
                        digestAlgorithm});
            return ((byte[])(results[0]));
        }
        
        /// <remarks/>
        public void signDigestAsync(string utente, string password, string pinToSign, byte[] digest, string digestAlgorithm) {
            this.signDigestAsync(utente, password, pinToSign, digest, digestAlgorithm, null);
        }
        
        /// <remarks/>
        public void signDigestAsync(string utente, string password, string pinToSign, byte[] digest, string digestAlgorithm, object userState) {
            if ((this.signDigestOperationCompleted == null)) {
                this.signDigestOperationCompleted = new System.Threading.SendOrPostCallback(this.OnsignDigestOperationCompleted);
            }
            this.InvokeAsync("signDigest", new object[] {
                        utente,
                        password,
                        pinToSign,
                        digest,
                        digestAlgorithm}, this.signDigestOperationCompleted, userState);
        }
        
        private void OnsignDigestOperationCompleted(object arg) {
            if ((this.signDigestCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.signDigestCompleted(this, new signDigestCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("", RequestNamespace="http://cosign.itagile.it", ResponseNamespace="http://cosign.itagile.it", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("getGraphicImageReturn", DataType="base64Binary")]
        public byte[] getGraphicImage(string userid, string password) {
            object[] results = this.Invoke("getGraphicImage", new object[] {
                        userid,
                        password});
            return ((byte[])(results[0]));
        }
        
        /// <remarks/>
        public void getGraphicImageAsync(string userid, string password) {
            this.getGraphicImageAsync(userid, password, null);
        }
        
        /// <remarks/>
        public void getGraphicImageAsync(string userid, string password, object userState) {
            if ((this.getGraphicImageOperationCompleted == null)) {
                this.getGraphicImageOperationCompleted = new System.Threading.SendOrPostCallback(this.OngetGraphicImageOperationCompleted);
            }
            this.InvokeAsync("getGraphicImage", new object[] {
                        userid,
                        password}, this.getGraphicImageOperationCompleted, userState);
        }
        
        private void OngetGraphicImageOperationCompleted(object arg) {
            if ((this.getGraphicImageCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.getGraphicImageCompleted(this, new getGraphicImageCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    public delegate void signPDFCompletedEventHandler(object sender, signPDFCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class signPDFCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal signPDFCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public byte[] Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((byte[])(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    public delegate void signDigestCompletedEventHandler(object sender, signDigestCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class signDigestCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal signDigestCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public byte[] Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((byte[])(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    public delegate void getGraphicImageCompletedEventHandler(object sender, getGraphicImageCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class getGraphicImageCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal getGraphicImageCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public byte[] Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((byte[])(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591