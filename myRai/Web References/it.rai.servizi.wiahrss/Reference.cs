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

namespace myRai.it.rai.servizi.wiahrss {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.ComponentModel;
    using System.Data;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="ezServiceSoap", Namespace="http://tempuri.org/")]
    public partial class ezService : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback HelloWorldOperationCompleted;
        
        private System.Threading.SendOrPostCallback ElencoTrasferteOperationCompleted;
        
        private System.Threading.SendOrPostCallback ElencoSituazioneDebitoriaOperationCompleted;
        
        private System.Threading.SendOrPostCallback ElencoOSNOperationCompleted;
        
        private System.Threading.SendOrPostCallback TLIQ_DATI_MATRICOLAOperationCompleted;
        
        private System.Threading.SendOrPostCallback TLIQ_DATI_MATRICOLA_SOperationCompleted;
        
        private System.Threading.SendOrPostCallback TLIQ_PROSPETTO_MATRICOLAOperationCompleted;
        
        private System.Threading.SendOrPostCallback SetBadgeElettronicoOperationCompleted;
        
        private System.Threading.SendOrPostCallback GetDataTableOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public ezService() {
            this.Url = global::myRai.Properties.Settings.Default.myRai_it_rai_servizi_wiahrss_ezService;
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
        public event HelloWorldCompletedEventHandler HelloWorldCompleted;
        
        /// <remarks/>
        public event ElencoTrasferteCompletedEventHandler ElencoTrasferteCompleted;
        
        /// <remarks/>
        public event ElencoSituazioneDebitoriaCompletedEventHandler ElencoSituazioneDebitoriaCompleted;
        
        /// <remarks/>
        public event ElencoOSNCompletedEventHandler ElencoOSNCompleted;
        
        /// <remarks/>
        public event TLIQ_DATI_MATRICOLACompletedEventHandler TLIQ_DATI_MATRICOLACompleted;
        
        /// <remarks/>
        public event TLIQ_DATI_MATRICOLA_SCompletedEventHandler TLIQ_DATI_MATRICOLA_SCompleted;
        
        /// <remarks/>
        public event TLIQ_PROSPETTO_MATRICOLACompletedEventHandler TLIQ_PROSPETTO_MATRICOLACompleted;
        
        /// <remarks/>
        public event SetBadgeElettronicoCompletedEventHandler SetBadgeElettronicoCompleted;
        
        /// <remarks/>
        public event GetDataTableCompletedEventHandler GetDataTableCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/HelloWorld", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string HelloWorld() {
            object[] results = this.Invoke("HelloWorld", new object[0]);
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void HelloWorldAsync() {
            this.HelloWorldAsync(null);
        }
        
        /// <remarks/>
        public void HelloWorldAsync(object userState) {
            if ((this.HelloWorldOperationCompleted == null)) {
                this.HelloWorldOperationCompleted = new System.Threading.SendOrPostCallback(this.OnHelloWorldOperationCompleted);
            }
            this.InvokeAsync("HelloWorld", new object[0], this.HelloWorldOperationCompleted, userState);
        }
        
        private void OnHelloWorldOperationCompleted(object arg) {
            if ((this.HelloWorldCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.HelloWorldCompleted(this, new HelloWorldCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/ElencoTrasferte", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string ElencoTrasferte() {
            object[] results = this.Invoke("ElencoTrasferte", new object[0]);
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void ElencoTrasferteAsync() {
            this.ElencoTrasferteAsync(null);
        }
        
        /// <remarks/>
        public void ElencoTrasferteAsync(object userState) {
            if ((this.ElencoTrasferteOperationCompleted == null)) {
                this.ElencoTrasferteOperationCompleted = new System.Threading.SendOrPostCallback(this.OnElencoTrasferteOperationCompleted);
            }
            this.InvokeAsync("ElencoTrasferte", new object[0], this.ElencoTrasferteOperationCompleted, userState);
        }
        
        private void OnElencoTrasferteOperationCompleted(object arg) {
            if ((this.ElencoTrasferteCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.ElencoTrasferteCompleted(this, new ElencoTrasferteCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/ElencoSituazioneDebitoria", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string ElencoSituazioneDebitoria() {
            object[] results = this.Invoke("ElencoSituazioneDebitoria", new object[0]);
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void ElencoSituazioneDebitoriaAsync() {
            this.ElencoSituazioneDebitoriaAsync(null);
        }
        
        /// <remarks/>
        public void ElencoSituazioneDebitoriaAsync(object userState) {
            if ((this.ElencoSituazioneDebitoriaOperationCompleted == null)) {
                this.ElencoSituazioneDebitoriaOperationCompleted = new System.Threading.SendOrPostCallback(this.OnElencoSituazioneDebitoriaOperationCompleted);
            }
            this.InvokeAsync("ElencoSituazioneDebitoria", new object[0], this.ElencoSituazioneDebitoriaOperationCompleted, userState);
        }
        
        private void OnElencoSituazioneDebitoriaOperationCompleted(object arg) {
            if ((this.ElencoSituazioneDebitoriaCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.ElencoSituazioneDebitoriaCompleted(this, new ElencoSituazioneDebitoriaCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/ElencoOSN", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string ElencoOSN(string _sdata, string _sdataF, string _st) {
            object[] results = this.Invoke("ElencoOSN", new object[] {
                        _sdata,
                        _sdataF,
                        _st});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void ElencoOSNAsync(string _sdata, string _sdataF, string _st) {
            this.ElencoOSNAsync(_sdata, _sdataF, _st, null);
        }
        
        /// <remarks/>
        public void ElencoOSNAsync(string _sdata, string _sdataF, string _st, object userState) {
            if ((this.ElencoOSNOperationCompleted == null)) {
                this.ElencoOSNOperationCompleted = new System.Threading.SendOrPostCallback(this.OnElencoOSNOperationCompleted);
            }
            this.InvokeAsync("ElencoOSN", new object[] {
                        _sdata,
                        _sdataF,
                        _st}, this.ElencoOSNOperationCompleted, userState);
        }
        
        private void OnElencoOSNOperationCompleted(object arg) {
            if ((this.ElencoOSNCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.ElencoOSNCompleted(this, new ElencoOSNCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/TLIQ_DATI_MATRICOLA", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Data.DataTable TLIQ_DATI_MATRICOLA(string _matricola, string _conteggio) {
            object[] results = this.Invoke("TLIQ_DATI_MATRICOLA", new object[] {
                        _matricola,
                        _conteggio});
            return ((System.Data.DataTable)(results[0]));
        }
        
        /// <remarks/>
        public void TLIQ_DATI_MATRICOLAAsync(string _matricola, string _conteggio) {
            this.TLIQ_DATI_MATRICOLAAsync(_matricola, _conteggio, null);
        }
        
        /// <remarks/>
        public void TLIQ_DATI_MATRICOLAAsync(string _matricola, string _conteggio, object userState) {
            if ((this.TLIQ_DATI_MATRICOLAOperationCompleted == null)) {
                this.TLIQ_DATI_MATRICOLAOperationCompleted = new System.Threading.SendOrPostCallback(this.OnTLIQ_DATI_MATRICOLAOperationCompleted);
            }
            this.InvokeAsync("TLIQ_DATI_MATRICOLA", new object[] {
                        _matricola,
                        _conteggio}, this.TLIQ_DATI_MATRICOLAOperationCompleted, userState);
        }
        
        private void OnTLIQ_DATI_MATRICOLAOperationCompleted(object arg) {
            if ((this.TLIQ_DATI_MATRICOLACompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.TLIQ_DATI_MATRICOLACompleted(this, new TLIQ_DATI_MATRICOLACompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/TLIQ_DATI_MATRICOLA_S", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Data.DataTable TLIQ_DATI_MATRICOLA_S(string _matricola, string _conteggio) {
            object[] results = this.Invoke("TLIQ_DATI_MATRICOLA_S", new object[] {
                        _matricola,
                        _conteggio});
            return ((System.Data.DataTable)(results[0]));
        }
        
        /// <remarks/>
        public void TLIQ_DATI_MATRICOLA_SAsync(string _matricola, string _conteggio) {
            this.TLIQ_DATI_MATRICOLA_SAsync(_matricola, _conteggio, null);
        }
        
        /// <remarks/>
        public void TLIQ_DATI_MATRICOLA_SAsync(string _matricola, string _conteggio, object userState) {
            if ((this.TLIQ_DATI_MATRICOLA_SOperationCompleted == null)) {
                this.TLIQ_DATI_MATRICOLA_SOperationCompleted = new System.Threading.SendOrPostCallback(this.OnTLIQ_DATI_MATRICOLA_SOperationCompleted);
            }
            this.InvokeAsync("TLIQ_DATI_MATRICOLA_S", new object[] {
                        _matricola,
                        _conteggio}, this.TLIQ_DATI_MATRICOLA_SOperationCompleted, userState);
        }
        
        private void OnTLIQ_DATI_MATRICOLA_SOperationCompleted(object arg) {
            if ((this.TLIQ_DATI_MATRICOLA_SCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.TLIQ_DATI_MATRICOLA_SCompleted(this, new TLIQ_DATI_MATRICOLA_SCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/TLIQ_PROSPETTO_MATRICOLA", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public System.Data.DataTable TLIQ_PROSPETTO_MATRICOLA(string _matricola, string _conteggio) {
            object[] results = this.Invoke("TLIQ_PROSPETTO_MATRICOLA", new object[] {
                        _matricola,
                        _conteggio});
            return ((System.Data.DataTable)(results[0]));
        }
        
        /// <remarks/>
        public void TLIQ_PROSPETTO_MATRICOLAAsync(string _matricola, string _conteggio) {
            this.TLIQ_PROSPETTO_MATRICOLAAsync(_matricola, _conteggio, null);
        }
        
        /// <remarks/>
        public void TLIQ_PROSPETTO_MATRICOLAAsync(string _matricola, string _conteggio, object userState) {
            if ((this.TLIQ_PROSPETTO_MATRICOLAOperationCompleted == null)) {
                this.TLIQ_PROSPETTO_MATRICOLAOperationCompleted = new System.Threading.SendOrPostCallback(this.OnTLIQ_PROSPETTO_MATRICOLAOperationCompleted);
            }
            this.InvokeAsync("TLIQ_PROSPETTO_MATRICOLA", new object[] {
                        _matricola,
                        _conteggio}, this.TLIQ_PROSPETTO_MATRICOLAOperationCompleted, userState);
        }
        
        private void OnTLIQ_PROSPETTO_MATRICOLAOperationCompleted(object arg) {
            if ((this.TLIQ_PROSPETTO_MATRICOLACompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.TLIQ_PROSPETTO_MATRICOLACompleted(this, new TLIQ_PROSPETTO_MATRICOLACompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/SetBadgeElettronico", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string SetBadgeElettronico(Timbrature_Elettronica _sBe) {
            object[] results = this.Invoke("SetBadgeElettronico", new object[] {
                        _sBe});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void SetBadgeElettronicoAsync(Timbrature_Elettronica _sBe) {
            this.SetBadgeElettronicoAsync(_sBe, null);
        }
        
        /// <remarks/>
        public void SetBadgeElettronicoAsync(Timbrature_Elettronica _sBe, object userState) {
            if ((this.SetBadgeElettronicoOperationCompleted == null)) {
                this.SetBadgeElettronicoOperationCompleted = new System.Threading.SendOrPostCallback(this.OnSetBadgeElettronicoOperationCompleted);
            }
            this.InvokeAsync("SetBadgeElettronico", new object[] {
                        _sBe}, this.SetBadgeElettronicoOperationCompleted, userState);
        }
        
        private void OnSetBadgeElettronicoOperationCompleted(object arg) {
            if ((this.SetBadgeElettronicoCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.SetBadgeElettronicoCompleted(this, new SetBadgeElettronicoCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://tempuri.org/GetDataTable", RequestNamespace="http://tempuri.org/", ResponseNamespace="http://tempuri.org/", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        public string GetDataTable(string _sRc, string CER) {
            object[] results = this.Invoke("GetDataTable", new object[] {
                        _sRc,
                        CER});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void GetDataTableAsync(string _sRc, string CER) {
            this.GetDataTableAsync(_sRc, CER, null);
        }
        
        /// <remarks/>
        public void GetDataTableAsync(string _sRc, string CER, object userState) {
            if ((this.GetDataTableOperationCompleted == null)) {
                this.GetDataTableOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetDataTableOperationCompleted);
            }
            this.InvokeAsync("GetDataTable", new object[] {
                        _sRc,
                        CER}, this.GetDataTableOperationCompleted, userState);
        }
        
        private void OnGetDataTableOperationCompleted(object arg) {
            if ((this.GetDataTableCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetDataTableCompleted(this, new GetDataTableCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
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
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class Timbrature_Elettronica {
        
        private string _IDField;
        
        private string _MatricolaField;
        
        private string _ProgField;
        
        private string _DataField;
        
        private string _StatoField;
        
        private string _IndirizzoField;
        
        private string _CittaField;
        
        private string _ProvinciaField;
        
        private string _CapField;
        
        private string _PosizioneField;
        
        private string _LatitudineField;
        
        private string _LongitudineField;
        
        /// <remarks/>
        public string _ID {
            get {
                return this._IDField;
            }
            set {
                this._IDField = value;
            }
        }
        
        /// <remarks/>
        public string _Matricola {
            get {
                return this._MatricolaField;
            }
            set {
                this._MatricolaField = value;
            }
        }
        
        /// <remarks/>
        public string _Prog {
            get {
                return this._ProgField;
            }
            set {
                this._ProgField = value;
            }
        }
        
        /// <remarks/>
        public string _Data {
            get {
                return this._DataField;
            }
            set {
                this._DataField = value;
            }
        }
        
        /// <remarks/>
        public string _Stato {
            get {
                return this._StatoField;
            }
            set {
                this._StatoField = value;
            }
        }
        
        /// <remarks/>
        public string _Indirizzo {
            get {
                return this._IndirizzoField;
            }
            set {
                this._IndirizzoField = value;
            }
        }
        
        /// <remarks/>
        public string _Citta {
            get {
                return this._CittaField;
            }
            set {
                this._CittaField = value;
            }
        }
        
        /// <remarks/>
        public string _Provincia {
            get {
                return this._ProvinciaField;
            }
            set {
                this._ProvinciaField = value;
            }
        }
        
        /// <remarks/>
        public string _Cap {
            get {
                return this._CapField;
            }
            set {
                this._CapField = value;
            }
        }
        
        /// <remarks/>
        public string _Posizione {
            get {
                return this._PosizioneField;
            }
            set {
                this._PosizioneField = value;
            }
        }
        
        /// <remarks/>
        public string _Latitudine {
            get {
                return this._LatitudineField;
            }
            set {
                this._LatitudineField = value;
            }
        }
        
        /// <remarks/>
        public string _Longitudine {
            get {
                return this._LongitudineField;
            }
            set {
                this._LongitudineField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    public delegate void HelloWorldCompletedEventHandler(object sender, HelloWorldCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class HelloWorldCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal HelloWorldCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    public delegate void ElencoTrasferteCompletedEventHandler(object sender, ElencoTrasferteCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ElencoTrasferteCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal ElencoTrasferteCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    public delegate void ElencoSituazioneDebitoriaCompletedEventHandler(object sender, ElencoSituazioneDebitoriaCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ElencoSituazioneDebitoriaCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal ElencoSituazioneDebitoriaCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    public delegate void ElencoOSNCompletedEventHandler(object sender, ElencoOSNCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ElencoOSNCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal ElencoOSNCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    public delegate void TLIQ_DATI_MATRICOLACompletedEventHandler(object sender, TLIQ_DATI_MATRICOLACompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class TLIQ_DATI_MATRICOLACompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal TLIQ_DATI_MATRICOLACompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public System.Data.DataTable Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((System.Data.DataTable)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    public delegate void TLIQ_DATI_MATRICOLA_SCompletedEventHandler(object sender, TLIQ_DATI_MATRICOLA_SCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class TLIQ_DATI_MATRICOLA_SCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal TLIQ_DATI_MATRICOLA_SCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public System.Data.DataTable Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((System.Data.DataTable)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    public delegate void TLIQ_PROSPETTO_MATRICOLACompletedEventHandler(object sender, TLIQ_PROSPETTO_MATRICOLACompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class TLIQ_PROSPETTO_MATRICOLACompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal TLIQ_PROSPETTO_MATRICOLACompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public System.Data.DataTable Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((System.Data.DataTable)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    public delegate void SetBadgeElettronicoCompletedEventHandler(object sender, SetBadgeElettronicoCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class SetBadgeElettronicoCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal SetBadgeElettronicoCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    public delegate void GetDataTableCompletedEventHandler(object sender, GetDataTableCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.7.3056.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetDataTableCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetDataTableCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
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