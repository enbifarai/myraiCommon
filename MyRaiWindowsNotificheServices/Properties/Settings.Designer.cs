﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Il codice è stato generato da uno strumento.
//     Versione runtime:4.0.30319.42000
//
//     Le modifiche apportate a questo file possono provocare un comportamento non corretto e andranno perse se
//     il codice viene rigenerato.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MyRaiWindowsNotificheServices.Properties {
    
    
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
        [global::System.Configuration.DefaultSettingValueAttribute("http://sendmail.servizi.rai.it/mail.asmx")]
        public string MyRaiWindowsNotificheServices_it_rai_servizi_sendmail_MailSender {
            get {
                return ((string)(this["MyRaiWindowsNotificheServices_it_rai_servizi_sendmail_MailSender"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://hrgb.servizi.rai.it/ws/Service.asmx")]
        public string MyRaiWindowsNotificheServices_it_rai_servizi_hrgb_Service {
            get {
                return ((string)(this["MyRaiWindowsNotificheServices_it_rai_servizi_hrgb_Service"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://hrga.servizi.rai.it/Filtro/sedi.asmx")]
        public string MyRaiWindowsNotificheServices_it_rai_servizi_hrga_Sedi {
            get {
                return ((string)(this["MyRaiWindowsNotificheServices_it_rai_servizi_hrga_Sedi"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://digigappws-his2016vip.intranet.rai.it/wsdigigapp.asmx")]
        public string MyRaiWindowsNotificheServices_it_rai_servizi_digigappws_WSDigigapp {
            get {
                return ((string)(this["MyRaiWindowsNotificheServices_it_rai_servizi_digigappws_WSDigigapp"]));
            }
        }
    }
}
