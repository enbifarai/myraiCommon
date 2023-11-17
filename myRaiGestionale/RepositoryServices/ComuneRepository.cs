using myRaiData.Incentivi;
using myRaiHelper.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiGestionale.RepositoryServices
{
    public class ComuneRepository : BaseRepository<TB_COMUNE>
    {
        public ComuneRepository(IncentiviEntities db) : base(db)
        {
        }
    }
}