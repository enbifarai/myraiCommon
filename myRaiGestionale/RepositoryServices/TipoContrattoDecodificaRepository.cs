using myRaiData.Incentivi;
using myRaiHelper.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiGestionale.RepositoryServices
{
    public class TipoContrattoDecodificaRepository : BaseRepository<TB_TPCNTR>
    {
        public TipoContrattoDecodificaRepository(IncentiviEntities db) : base(db)
        {
        }
    }
}