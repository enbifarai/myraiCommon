//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace myRaiData.Incentivi
{
    using System;
    using System.Collections.Generic;
    
    public partial class XR_MAT_ELENCO_TASK
    {
        public XR_MAT_ELENCO_TASK()
        {
            this.XR_MAT_CATEGORIA_TASK = new HashSet<XR_MAT_CATEGORIA_TASK>();
            this.XR_MAT_TASK_DI_SERVIZIO = new HashSet<XR_MAT_TASK_DI_SERVIZIO>();
            this.XR_MAT_TASK_IN_CORSO = new HashSet<XR_MAT_TASK_IN_CORSO>();
        }
    
        public int ID { get; set; }
        public string TIPO { get; set; }
        public string NOME_TASK { get; set; }
        public string DESCRIZIONE_TASK { get; set; }
        public Nullable<int> ID_TRACCIATO_DEW { get; set; }
        public Nullable<int> PROGRESSIVO_TRACCIATO_DEW { get; set; }
        public string APPKEY { get; set; }
        public Nullable<bool> OBBLIGATORIO_PER_CONCLUSIONE { get; set; }
    
        public virtual ICollection<XR_MAT_CATEGORIA_TASK> XR_MAT_CATEGORIA_TASK { get; set; }
        public virtual ICollection<XR_MAT_TASK_DI_SERVIZIO> XR_MAT_TASK_DI_SERVIZIO { get; set; }
        public virtual ICollection<XR_MAT_TASK_IN_CORSO> XR_MAT_TASK_IN_CORSO { get; set; }
    }
}