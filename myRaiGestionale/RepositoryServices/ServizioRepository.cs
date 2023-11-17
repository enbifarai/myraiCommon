using myRaiData.Incentivi;
using myRaiHelper.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiGestionale.RepositoryServices
{
    public class ServizioRepository : BaseRepository<XR_SERVIZIO>
    {
        public ServizioRepository(IncentiviEntities db) : base(db)
        {
        }
    }
}