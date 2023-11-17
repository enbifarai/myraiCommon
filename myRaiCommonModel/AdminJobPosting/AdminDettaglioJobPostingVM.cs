using System.Collections.Generic;
using myRaiData;
using myRai.Data.CurriculumVitae;

namespace myRaiCommonModel.AdminJobPosting
{
	public class AdminDettaglioJobPostingVM
	{
		public tblJobPosting JobPosting { get; set; }
		public List<JobPostingFilesExt> Files { get; set; }
	}


	public class JobPostingFilesExt : JobPostingFiles
	{
		public string Nominativo { get; set; }
	}
}