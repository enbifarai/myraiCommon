using System;
using System.Linq;
using myRai.Business;
//using myRai.digigappws;
using MyRaiServiceInterface.it.rai.servizi.digigappws;

namespace myRaiHelper
{
    public class AnalisiEccezioni
    {
        /// <summary>
        /// Restituisce NULL in caso di errore, 0 se non ha carenza, >0 numero di minuti (gia sottratti i minimi per sede gapp)
        /// 
        /// </summary>
        /// <param name="giornata"></param>
        /// <param name="dataNoSlash"></param>
        /// <param name="matricola"></param>
        /// <returns></returns>
        public static int? GiornataHaCarenza(string currentMatricola, dayResponse giornata, Boolean SottraiMinimo = true, string dataNoSlash = null, string matricola = null)
        {
            if (giornata == null)
            {
                if (String.IsNullOrWhiteSpace(dataNoSlash) || String.IsNullOrWhiteSpace(matricola)) return null;
                giornata = GetGiornata(currentMatricola, dataNoSlash, matricola);
                if (giornata == null ) return null;
            }
            if (giornata.eccezioni == null || giornata.eccezioni.Count() == 0) return 0;

            Eccezione car = giornata.eccezioni.Where(x =>x.cod!=null &&  x.cod.Trim() == "CAR").FirstOrDefault();

            if (car == null) return 0;

            if (SottraiMinimo)
            {
                var db = new myRaiData.digiGappEntities();
                int minimoIntervallo = 0;
                string minimo = db.L2D_SEDE_GAPP.Where(x => x.cod_sede_gapp == giornata.giornata.sedeGapp).Select(x => x.minimo_car).FirstOrDefault();
                if (!String.IsNullOrWhiteSpace(minimo)) minimoIntervallo = Convert.ToInt32(minimo);

                int minutiCar = EccezioniManager.calcolaMinuti(car.qta) - minimoIntervallo;
                if (minutiCar < 0)
                    return 0;
                else
                    return minutiCar;
            }
            else
            {
                return EccezioniManager.calcolaMinuti(car.qta);
            }

        }

        public static dayResponse GetGiornata(string currentMatricola, string dataDDMMYYYY, string matricola)
        {
            try
            {
				//WSDigigapp service = new WSDigigapp()
				//{
				//	Credentials = new System.Net.NetworkCredential(CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
				//		CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1])
				//};

				//return service.getEccezioni(matricola, dataDDMMYYYY, "BU", 70);

				WSDigigappDataController service = new WSDigigappDataController();

				return service.GetEccezioni(currentMatricola, matricola, dataDDMMYYYY, "BU", 70 );

            }
            catch (Exception ex)
            {
                Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    matricola = currentMatricola,
                    error_message = ex.ToString(),
                    provenienza = "AnalisiEccezioni.GetGiornata"
                });
                return null;
            }

        }
    }
}