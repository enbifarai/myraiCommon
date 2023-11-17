using myRaiData.Incentivi;
using myRaiHelper.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiGestionale.RepositoryServices
{
    public class QualificaRepository : BaseRepository<QUALIFICA>
    {
        public QualificaRepository(IncentiviEntities db) : base(db)
        {
        }
    }
}