﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Il codice è stato generato da uno strumento.
//     Versione runtime:4.0.30319.42000
//
//     Le modifiche apportate a questo file possono provocare un comportamento non corretto e andranno perse se
//     il codice viene rigenerato.
// </auto-generated>
//------------------------------------------------------------------------------

namespace myRaiServiceHub.ServiceBonus100 {
    using System.Data;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ServiceBonus100.ezServiceSoap")]
    public interface ezServiceSoap {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/HelloWorld", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        string HelloWorld();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ElencoTrasferte", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        string ElencoTrasferte();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ElencoSituazioneDebitoria", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        string ElencoSituazioneDebitoria();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ElencoOSN", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        string ElencoOSN(string _sdata, string _sdataF, string _st);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/TLIQ_DATI_MATRICOLA", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Data.DataTable TLIQ_DATI_MATRICOLA(string _matricola, string _conteggio);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/TLIQ_DATI_MATRICOLA_S", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Data.DataTable TLIQ_DATI_MATRICOLA_S(string _matricola, string _conteggio);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/TBONUS_100EURO", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Data.DataTable TBONUS_100EURO(string _matricola);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/TUPDATE_BONUS_100EURO", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        string TUPDATE_BONUS_100EURO(string _matricola, string risposta);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/TRESET_BONUS_100EURO", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        string TRESET_BONUS_100EURO(string _matricola);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/TLIQ_PROSPETTO_MATRICOLA", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Data.DataTable TLIQ_PROSPETTO_MATRICOLA(string _matricola, string _conteggio);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/SetBadgeElettronico", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        string SetBadgeElettronico(myRaiServiceHub.ServiceBonus100.Timbrature_Elettronica _sBe);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetDataTable", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        string GetDataTable(string _sRc, string CER);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/TRINUNCIA_BONUS", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Data.DataTable TRINUNCIA_BONUS(string _matricola);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/TSAVE_RINUNCIA_BONUS", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        string TSAVE_RINUNCIA_BONUS(string _matricola, string annoRif, string risposta);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/TRESET_RINUNCIA_BONUS", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        string TRESET_RINUNCIA_BONUS(string _matricola, string annoRif);
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.4084.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/")]
    public partial class Timbrature_Elettronica : object, System.ComponentModel.INotifyPropertyChanged {
        
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
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public string _ID {
            get {
                return this._IDField;
            }
            set {
                this._IDField = value;
                this.RaisePropertyChanged("_ID");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public string _Matricola {
            get {
                return this._MatricolaField;
            }
            set {
                this._MatricolaField = value;
                this.RaisePropertyChanged("_Matricola");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=2)]
        public string _Prog {
            get {
                return this._ProgField;
            }
            set {
                this._ProgField = value;
                this.RaisePropertyChanged("_Prog");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=3)]
        public string _Data {
            get {
                return this._DataField;
            }
            set {
                this._DataField = value;
                this.RaisePropertyChanged("_Data");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=4)]
        public string _Stato {
            get {
                return this._StatoField;
            }
            set {
                this._StatoField = value;
                this.RaisePropertyChanged("_Stato");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=5)]
        public string _Indirizzo {
            get {
                return this._IndirizzoField;
            }
            set {
                this._IndirizzoField = value;
                this.RaisePropertyChanged("_Indirizzo");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=6)]
        public string _Citta {
            get {
                return this._CittaField;
            }
            set {
                this._CittaField = value;
                this.RaisePropertyChanged("_Citta");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=7)]
        public string _Provincia {
            get {
                return this._ProvinciaField;
            }
            set {
                this._ProvinciaField = value;
                this.RaisePropertyChanged("_Provincia");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=8)]
        public string _Cap {
            get {
                return this._CapField;
            }
            set {
                this._CapField = value;
                this.RaisePropertyChanged("_Cap");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=9)]
        public string _Posizione {
            get {
                return this._PosizioneField;
            }
            set {
                this._PosizioneField = value;
                this.RaisePropertyChanged("_Posizione");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=10)]
        public string _Latitudine {
            get {
                return this._LatitudineField;
            }
            set {
                this._LatitudineField = value;
                this.RaisePropertyChanged("_Latitudine");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=11)]
        public string _Longitudine {
            get {
                return this._LongitudineField;
            }
            set {
                this._LongitudineField = value;
                this.RaisePropertyChanged("_Longitudine");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface ezServiceSoapChannel : myRaiServiceHub.ServiceBonus100.ezServiceSoap, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ezServiceSoapClient : System.ServiceModel.ClientBase<myRaiServiceHub.ServiceBonus100.ezServiceSoap>, myRaiServiceHub.ServiceBonus100.ezServiceSoap {
        
        public ezServiceSoapClient() {
        }
        
        public ezServiceSoapClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public ezServiceSoapClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ezServiceSoapClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ezServiceSoapClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string HelloWorld() {
            return base.Channel.HelloWorld();
        }
        
        public string ElencoTrasferte() {
            return base.Channel.ElencoTrasferte();
        }
        
        public string ElencoSituazioneDebitoria() {
            return base.Channel.ElencoSituazioneDebitoria();
        }
        
        public string ElencoOSN(string _sdata, string _sdataF, string _st) {
            return base.Channel.ElencoOSN(_sdata, _sdataF, _st);
        }
        
        public System.Data.DataTable TLIQ_DATI_MATRICOLA(string _matricola, string _conteggio) {
            return base.Channel.TLIQ_DATI_MATRICOLA(_matricola, _conteggio);
        }
        
        public System.Data.DataTable TLIQ_DATI_MATRICOLA_S(string _matricola, string _conteggio) {
            return base.Channel.TLIQ_DATI_MATRICOLA_S(_matricola, _conteggio);
        }
        
        public System.Data.DataTable TBONUS_100EURO(string _matricola) {
            return base.Channel.TBONUS_100EURO(_matricola);
        }
        
        public string TUPDATE_BONUS_100EURO(string _matricola, string risposta) {
            return base.Channel.TUPDATE_BONUS_100EURO(_matricola, risposta);
        }
        
        public string TRESET_BONUS_100EURO(string _matricola) {
            return base.Channel.TRESET_BONUS_100EURO(_matricola);
        }
        
        public System.Data.DataTable TLIQ_PROSPETTO_MATRICOLA(string _matricola, string _conteggio) {
            return base.Channel.TLIQ_PROSPETTO_MATRICOLA(_matricola, _conteggio);
        }
        
        public string SetBadgeElettronico(myRaiServiceHub.ServiceBonus100.Timbrature_Elettronica _sBe) {
            return base.Channel.SetBadgeElettronico(_sBe);
        }
        
        public string GetDataTable(string _sRc, string CER) {
            return base.Channel.GetDataTable(_sRc, CER);
        }
        
        public System.Data.DataTable TRINUNCIA_BONUS(string _matricola) {
            return base.Channel.TRINUNCIA_BONUS(_matricola);
        }
        
        public string TSAVE_RINUNCIA_BONUS(string _matricola, string annoRif, string risposta) {
            return base.Channel.TSAVE_RINUNCIA_BONUS(_matricola, annoRif, risposta);
        }
        
        public string TRESET_RINUNCIA_BONUS(string _matricola, string annoRif) {
            return base.Channel.TRESET_RINUNCIA_BONUS(_matricola, annoRif);
        }
    }
}
