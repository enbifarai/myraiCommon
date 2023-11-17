using myRaiHelper.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using myRaiData.Incentivi;

namespace myRaiGestionale.RepositoryServices
{
    public class nazioneRepository : BaseRepository<TB_CITTAD>
    {
        public nazioneRepository(IncentiviEntities db) : base(db)
        {
        }
    }
}