﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Il codice è stato generato da uno strumento.
//     Versione runtime:4.0.30319.42000
//
//     Le modifiche apportate a questo file possono provocare un comportamento non corretto e andranno perse se
//     il codice viene rigenerato.
// </auto-generated>
//------------------------------------------------------------------------------

namespace myRaiServiceTestClient.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.9.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://10.16.161.202:8080/FirmaRemota/services/FirmaStandard")]
        public string myRaiServiceTestClient_WebReference_firmaStandard {
            get {
                return ((string)(this["myRaiServiceTestClient_WebReference_firmaStandard"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://10.16.161.202:8080/FirmaRemota/services/RemoteSignature")]
        public string myRaiServiceTestClient_firmaRemota_remoteSignature {
            get {
                return ((string)(this["myRaiServiceTestClient_firmaRemota_remoteSignature"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://sendmail.servizi.rai.it/mail.asmx")]
        public string myRaiServiceTestClient_it_rai_servizi_sendmail_MailSender {
            get {
                return ((string)(this["myRaiServiceTestClient_it_rai_servizi_sendmail_MailSender"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://anagraficaws.servizi.rai.it/apwebservice/APWS.asmx")]
        public string myRaiServiceTestClient_it_rai_servizi_anagraficaws1_APWS {
            get {
                return ((string)(this["myRaiServiceTestClient_it_rai_servizi_anagraficaws1_APWS"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://digigappws-his2016vip.intranet.rai.it/wsdigigapp.asmx")]
        public string myRaiServiceTestClient_WSlocalhost_WSDigigapp {
            get {
                return ((string)(this["myRaiServiceTestClient_WSlocalhost_WSDigigapp"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://digigappws-his2016vip.intranet.rai.it/WSDigigapp.asmx")]
        public string myRaiServiceTestClient_it_rai_servizi_svildigigappws_WSDigigapp {
            get {
                return ((string)(this["myRaiServiceTestClient_it_rai_servizi_svildigigappws_WSDigigapp"]));
            }
        }
    }
}
