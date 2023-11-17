using myRaiData.Incentivi;
using myRaiHelper.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiGestionale.RepositoryServices
{
    public class AnagraficaRepository : BaseRepository<ANAGPERS>
    {
        public AnagraficaRepository(IncentiviEntities db) : base(db)
        {
        }

        public bool ExistAnagraficaByCodiceFiscale(string codiceFiscale)
        {
            
            return base.Get(w => w.CSF_CFSPERSONA == codiceFiscale) != null;
        }

    }
}