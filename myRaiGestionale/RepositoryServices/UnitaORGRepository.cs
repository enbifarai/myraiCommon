using myRaiData.Incentivi;
using myRaiHelper;
using myRaiHelper.GenericRepository;
using System;
using System.Linq;

namespace myRaiGestionale.RepositoryServices
{
    public class UnitaORGRepository : BaseRepository<UNITAORG>
    {
        public UnitaORGRepository(IncentiviEntities db) : base(db)
        {
        }

        public int GetBySezione(string codicesezione)
        {
            try
            {
                return db.UNITAORG.SingleOrDefault(w => w.COD_UNITAORG == codicesezione.Trim()).ID_UNITAORG;
            }
            catch (Exception ex)
            {
                HrisHelper.AddSegnalazione("Ricerca sezione", "HRIS: immatricolazione", "Sezione " + codicesezione.Trim() + " non trovata");
                throw new Exception("Sezione "+codicesezione.Trim()+" non trovata");

            }
        }
    }
}