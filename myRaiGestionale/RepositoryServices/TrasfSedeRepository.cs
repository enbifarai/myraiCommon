using myRaiData.Incentivi;
using myRaiHelper.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiGestionale.RepositoryServices
{
    public class TrasfSedeRepository : BaseRepository<TRASF_SEDE>
    {
        public TrasfSedeRepository(IncentiviEntities db) : base(db)
        {
        }
    }
}