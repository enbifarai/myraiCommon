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

namespace myRaiCommonTasks.sendMail {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.ComponentModel;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.2053.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="MailSenderSoap", Namespace="http://rai/webservices/")]
    public partial class MailSender : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback SendOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public MailSender() {
            this.Url = global::myRaiCommonTasks.Properties.Settings.Default.myRaiCommonTasks_it_rai_servizi_sendmail_MailSender;
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
        public event SendCompletedEventHandler SendCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://rai/webservices/Send", RequestNamespace="http://rai/webservices/", ResponseNamespace="http://rai/webservices/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public void Send(Email objEmail) {

			//se non è produzione, controlla e riscrivi indirizzi
			if ( !myRaiCommonTasks.CommonTasks.IsProduzione() )
			{
				string[] par = myRaiCommonTasks.CommonTasks.GetParametri<string>( myRaiCommonTasks.CommonTasks.EnumParametriSistema.RedirectEmailSuSviluppo );
				if ( par != null && par.Length == 2 && Convert.ToBoolean( par[0] ) == true )
				{
					objEmail.toList = CommonTasks.RewriteAddress( objEmail.toList, objEmail, par[1].Split( ',' ) );
					objEmail.ccList = CommonTasks.RewriteAddress( objEmail.ccList, objEmail, par[1].Split( ',' ) );
					objEmail.bccList = CommonTasks.RewriteAddress( objEmail.bccList, objEmail, par[1].Split( ',' ) );
				}
			}

			this.Invoke("Send", new object[] {
                        objEmail});
        }
        
        /// <remarks/>
        public void SendAsync(Email objEmail) {
            this.SendAsync(objEmail, null);
        }
        
        /// <remarks/>
        public void SendAsync(Email objEmail, object userState) {
            if ((this.SendOperationCompleted == null)) {
                this.SendOperationCompleted = new System.Threading.SendOrPostCallback(this.OnSendOperationCompleted);
            }
            this.InvokeAsync("Send", new object[] {
                        objEmail}, this.SendOperationCompleted, userState);
        }
        
        private void OnSendOperationCompleted(object arg) {
            if ((this.SendCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.SendCompleted(this, new System.ComponentModel.AsyncCompletedEventArgs(invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.7.3163.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://rai/webservices/")]
    public partial class Email {
        
        private string[] toListField;
        
        private string[] ccListField;
        
        private string[] bccListField;
        
        private string subjectField;
        
        private string bodyField;
        
        private string fromField;
        
        private System.DateTime sendWhenField;
        
        private int priorityField;
        
        private string contentTypeField;
        
        private string charSetField;
        
        private string contentTransferEncodingField;
        
        private Attachement[] attachementsListField;
        
        /// <remarks/>
        public string[] toList {
            get {
                return this.toListField;
            }
            set {
                this.toListField = value;
            }
        }
        
        /// <remarks/>
        public string[] ccList {
            get {
                return this.ccListField;
            }
            set {
                this.ccListField = value;
            }
        }
        
        /// <remarks/>
        public string[] bccList {
            get {
                return this.bccListField;
            }
            set {
                this.bccListField = value;
            }
        }
        
        /// <remarks/>
        public string Subject {
            get {
                return this.subjectField;
            }
            set {
                this.subjectField = value;
            }
        }
        
        /// <remarks/>
        public string Body {
            get {
                return this.bodyField;
            }
            set {
                this.bodyField = value;
            }
        }
        
        /// <remarks/>
        public string From {
            get {
                return this.fromField;
            }
            set {
                this.fromField = value;
            }
        }
        
        /// <remarks/>
        public System.DateTime SendWhen {
            get {
                return this.sendWhenField;
            }
            set {
                this.sendWhenField = value;
            }
        }
        
        /// <remarks/>
        public int Priority {
            get {
                return this.priorityField;
            }
            set {
                this.priorityField = value;
            }
        }
        
        /// <remarks/>
        public string ContentType {
            get {
                return this.contentTypeField;
            }
            set {
                this.contentTypeField = value;
            }
        }
        
        /// <remarks/>
        public string CharSet {
            get {
                return this.charSetField;
            }
            set {
                this.charSetField = value;
            }
        }
        
        /// <remarks/>
        public string ContentTransferEncoding {
            get {
                return this.contentTransferEncodingField;
            }
            set {
                this.contentTransferEncodingField = value;
            }
        }
        
        /// <remarks/>
        public Attachement[] AttachementsList {
            get {
                return this.attachementsListField;
            }
            set {
                this.attachementsListField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.7.3163.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://rai/webservices/")]
    public partial class Attachement {
        
        private string attachementNameField;
        
        private string attachementTypeField;
        
        private byte[] attachementValueField;
        
        /// <remarks/>
        public string AttachementName {
            get {
                return this.attachementNameField;
            }
            set {
                this.attachementNameField = value;
            }
        }
        
        /// <remarks/>
        public string AttachementType {
            get {
                return this.attachementTypeField;
            }
            set {
                this.attachementTypeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(DataType="base64Binary")]
        public byte[] AttachementValue {
            get {
                return this.attachementValueField;
            }
            set {
                this.attachementValueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.2053.0")]
    public delegate void SendCompletedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);
}

#pragma warning restore 1591