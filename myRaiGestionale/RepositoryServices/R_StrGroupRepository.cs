using myRaiData.Incentivi;
using myRaiHelper.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiGestionale.RepositoryServices
{
    public class R_StrGroupRepository : BaseRepository<R_STRGROUP>
    {
        public R_StrGroupRepository(IncentiviEntities db) : base(db)
        {
        }
    }
}