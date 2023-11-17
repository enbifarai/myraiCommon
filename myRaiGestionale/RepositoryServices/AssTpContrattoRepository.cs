using myRaiData.Incentivi;
using myRaiHelper.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiGestionale.RepositoryServices
{
    public class AssTpContrattoRepository : BaseRepository<ASSTPCONTR>
    {
        public AssTpContrattoRepository(IncentiviEntities db) : base(db)
        {
        }
    }
}