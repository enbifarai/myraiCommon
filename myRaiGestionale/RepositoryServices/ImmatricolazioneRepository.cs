using myRaiHelper.GenericRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using myRaiData.Incentivi;
using System.Linq.Expressions;

namespace myRaiGestionale.RepositoryServices
{
    public class ImmatricolazioneRepository : BaseRepository<XR_IMM_IMMATRICOLAZIONI>
    {
        public ImmatricolazioneRepository(IncentiviEntities db) : base(db)
        {
        }

        public bool Add(XR_IMM_IMMATRICOLAZIONI entity, bool generateOid = false)
        {
            if (generateOid)
            {
               entity.ID_EVENTO = GeneraOid(w => w.ID_EVENTO);
            }
            return base.Add(entity);
        }
       
     
        public XR_IMM_IMMATRICOLAZIONI GetByIdWithRelation(int id)
        {
            return db.XR_IMM_IMMATRICOLAZIONI
                .Include("ANAGPERS")
                .Include("ANAGPERS.ASSQUAL")
                .Include("ANAGPERS.ASSTPCONTR")
                .Include("ANAGPERS.INCARLAV")
                .Include("ANAGPERS.TRASF_SEDE")
                .Include("ANAGPERS.XR_SERVIZIO")
                .FirstOrDefault(imm=> imm.ID_EVENTO == id);
        }

       


    }
}