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
// Codice sorgente generato automaticamente da xsd, versione=4.0.30319.17929.
// 
namespace myRaiIlias {
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://tempuri.org/ilias_advanced_meta_data_3_9")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://tempuri.org/ilias_advanced_meta_data_3_9", IsNullable=false)]
    public partial class AdvancedMetaData {
        
        private Value[] valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Value")]
        public Value[] Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://tempuri.org/ilias_advanced_meta_data_3_9")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://tempuri.org/ilias_advanced_meta_data_3_9", IsNullable=false)]
    public partial class Value {
        
        private string idField;
        
        private string value1Field;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value1 {
            get {
                return this.value1Field;
            }
            set {
                this.value1Field = value;
            }
        }
    }
}