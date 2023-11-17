using myRaiData.Incentivi;
using myRaiHelper.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace myRaiGestionale.RepositoryServices
{
    public class ImpresaRepository : BaseRepository<CODIFYIMP>
    {
        //public IEnumerable<SelectListItem> GetSelectItemImpresa()
        //{
        //    var result = (from a in db.CODIFYIMP
        //                  join b in db.R_STRGROUP
        //                  on a.COD_IMPRESA equals b.COD_IMPRESA
        //                  orderby a.COD_IMPRESA
        //                  select new SelectListItem { Value = a.COD_IMPRESA, Text = a.COD_SOGGETTO }).ToList();
        //    return result;
        //}
        public ImpresaRepository(IncentiviEntities db) : base(db)
        {
        }
    }
}