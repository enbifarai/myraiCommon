﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Il codice è stato generato da uno strumento.
//     Versione runtime:4.0.30319.42000
//
//     Le modifiche apportate a questo file possono provocare un comportamento non corretto e andranno perse se
//     il codice viene rigenerato.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MyRaiServiceInterface.Properties {
    
    
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
        [global::System.Configuration.DefaultSettingValueAttribute("http://hrgb.servizi.rai.it/ws/Service.asmx")]
        public string MyRaiServiceInterface_it_rai_servizi_hrgb_Service {
            get {
                return ((string)(this["MyRaiServiceInterface_it_rai_servizi_hrgb_Service"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://wiahrss.servizi.rai.it/rp2/ezService/ezService.asmx")]
        public string MyRaiServiceInterface_it_rai_servizi_svilwiahrss_ezService {
            get {
                return ((string)(this["MyRaiServiceInterface_it_rai_servizi_svilwiahrss_ezService"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://bts-r2d2.servizi.rai.it/GAPPPRP/Planning.svc")]
        public string MyRaiServiceInterface_CeitonWS_GappPrpService {
            get {
                return ((string)(this["MyRaiServiceInterface_CeitonWS_GappPrpService"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://BTS-R2D2.servizi.rai.it/GAPPScheduAll/Planning.svc")]
        public string MyRaiServiceInterface_CeitonScheduall_GappRadioPrpService {
            get {
                return ((string)(this["MyRaiServiceInterface_CeitonScheduall_GappRadioPrpService"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://svilruoesercizio.servizi.rai.it/WSdew.asmx")]
        public string MyRaiServiceInterface_it_rai_servizi_svilruoesercizio_WSDew {
            get {
                return ((string)(this["MyRaiServiceInterface_it_rai_servizi_svilruoesercizio_WSDew"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://digigappws-his2016vip.intranet.rai.it/wsdigigapp.asmx")]
        public string MyRaiServiceInterface_it_rai_servizi_digigappws_WSDigigapp {
            get {
                return ((string)(this["MyRaiServiceInterface_it_rai_servizi_digigappws_WSDigigapp"]));
            }
        }
    }
}
