﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Il codice è stato generato da uno strumento.
//     Versione runtime:4.0.30319.42000
//
//     Le modifiche apportate a questo file possono provocare un comportamento non corretto e andranno perse se
//     il codice viene rigenerato.
// </auto-generated>
//------------------------------------------------------------------------------

namespace myRaiGestionale.Properties {
    
    
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
        [global::System.Configuration.DefaultSettingValueAttribute("http://hrpaga.servizi.rai.it/utility/comunica.asmx")]
        public string myRai_it_rai_servizi_utility_Comunica {
            get {
                return ((string)(this["myRai_it_rai_servizi_utility_Comunica"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://hrpaganew.intranet.rai.it/utility/Comunica.asmx")]
        public string myRai_it_rai_servizi_hrpaga_HrPaga_New {
            get {
                return ((string)(this["myRai_it_rai_servizi_hrpaga_HrPaga_New"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://svilbts-r2d2.servizi.rai.it/GAPPPRP/Planning.svc")]
        public string myRai_CeitonWS_GappPrpService {
            get {
                return ((string)(this["myRai_CeitonWS_GappPrpService"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://digigappws-his2016vip.intranet.rai.it/WSDigigapp.asmx")]
        public string myRai_digigappws_WSDigigapp {
            get {
                return ((string)(this["myRai_digigappws_WSDigigapp"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://svildigigappws-his2016.intranet.rai.it/API/MyRaiService1.svc")]
        public string myRai_digigappws_wcf1_wcf_API_1 {
            get {
                return ((string)(this["myRai_digigappws_wcf1_wcf_API_1"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://raiconnectcoll.servizi.rai.it/rai_ruo_ws.asmx")]
        public string myRaiGestionale_it_rai_servizi_raiconnectcoll_rai_ruo_ws {
            get {
                return ((string)(this["myRaiGestionale_it_rai_servizi_raiconnectcoll_rai_ruo_ws"]));
            }
        }
    }
}
