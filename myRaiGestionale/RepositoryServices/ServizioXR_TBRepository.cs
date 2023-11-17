using myRaiData.Incentivi;
using myRaiHelper.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace myRaiGestionale.RepositoryServices
{
    public class ServizioXRRepository : BaseRepository<XR_TB_SERVIZIO>
    {
        public ServizioXRRepository(IncentiviEntities db) : base(db)
        {
        }

        
    }
}