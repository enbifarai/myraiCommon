using System.Collections.Generic;
using myRaiDataTalentia;
using myRaiCommonModel;
using myRaiHelper;

namespace myRaiGestionale.AnagPersService
{
  
    public class AnagPersServices 
    {
        readonly UnitOfWork _unitOfWork = new UnitOfWork();      
        public bool Insert(AnagraficaTalentiaModel model)
        {
            ANAGPERS vm = new ANAGPERS();
            bool result = false;
            var codiceFiscale_Coincidenze =_unitOfWork.AnagPersRepository.GetMany(s => s.CSF_CFSPERSONA);
            foreach (var item in codiceFiscale_Coincidenze)
            {
                if (item == model.CodiceFiscale) { return result; }
                else { break; }
            }

            if (_unitOfWork.AnagPersRepository.Insert(vm))
                result = true;
            return result;

        }
    }
}