//------------------------------------------------------------------------------
// <auto-generated>
//    Codice generato da un modello.
//
//    Le modifiche manuali a questo file potrebbero causare un comportamento imprevisto dell'applicazione.
//    Se il codice viene rigenerato, le modifiche manuali al file verranno sovrascritte.
// </auto-generated>
//------------------------------------------------------------------------------

namespace myRaiData.CurriculumVitae
{
    using System;
    using System.Collections.Generic;
    
    public partial class Files
    {
        public Files()
        {
            this.JobPostingFiles = new HashSet<JobPostingFiles>();
        }
    
        public int Id { get; set; }
        public string Matricola { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public string MimeType { get; set; }
        public System.DateTime CreationDate { get; set; }
        public byte[] ContentType { get; set; }
    
        public virtual ICollection<JobPostingFiles> JobPostingFiles { get; set; }
    }
}
