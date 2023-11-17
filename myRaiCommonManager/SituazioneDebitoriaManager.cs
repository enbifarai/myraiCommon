using myRaiHelper;
using MyRaiServiceInterface.MyRaiServiceReference1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonManager
{
    public class SituazioneDebitoriaManager
    {
        public static List<SituazioneDebitoria> GetSituazioneDebitoria(string richMatr = "")
        {
            List<SituazioneDebitoria> result = new List<SituazioneDebitoria>();

            MyRaiService1Client service = new MyRaiService1Client();

            try
            {
                string matricola = !String.IsNullOrWhiteSpace(richMatr) ? richMatr : UtenteHelper.EsponiAnagrafica()._matricola;
                string matrRichiedente = matricola;

                //matricola = "684930";
                SituazioneDebitoriaResponse serviceResponse = service.GetSituazioneDebitoria(matrRichiedente, matricola);

                if (!serviceResponse.Esito)
                {
                    //throw new Exception(serviceResponse.Errore);
                    result = new List<SituazioneDebitoria>();
                }
                else 
                    result = serviceResponse.List.ToList();
            }
            catch (Exception ex)
            {
                result = new List<SituazioneDebitoria>();
                //throw new Exception( ex.Message );
            }

            return result;
        }
    }
}