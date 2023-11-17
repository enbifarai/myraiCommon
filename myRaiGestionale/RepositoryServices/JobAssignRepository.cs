using myRaiHelper.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using myRaiData.Incentivi;


namespace myRaiGestionale.RepositoryServices
{
    public class JobAssignRepository : BaseRepository<JOBASSIGN>
    {
        public JobAssignRepository(IncentiviEntities db) : base(db)
        {
        }
    }
}