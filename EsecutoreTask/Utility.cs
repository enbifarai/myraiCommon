using myRaiData.Incentivi;
using myRaiHelper;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EsecutoreTask
{
    public static class ExtensionMethods
    {
        public static int ToMinutes(this string value)
        {
            if (value == null || value.Trim() == "" || value.Trim().Length < 4)
                return 0;

            string[] array = new string[2];

            if (value.IndexOf(':') > 0)
                array = value.Split(':');
            else
            {
                array[0] = value.Substring(0, 2);
                array[1] = value.Substring(2, 2);
            }
            return Int32.Parse(array[0]) * 60 + Int32.Parse(array[1]);
        }
        public static string ToHH_MM(this int value)
        {
            if (value <= 0)
                return "00:00";
            int h = (int)value / 60;
            int min = value - (h * 60);
            return h.ToString().PadLeft(2, '0') + ":" + min.ToString().PadLeft(2, '0');
        }
    }
        public class Utility
    {
        public static XR_MAT_GIORNI_CONGEDO GetEccezioneInPianificazione(DateTime data, XR_MAT_RICHIESTE Richiesta)
        {
            XR_MAT_PIANIFICAZIONI pian = Richiesta.XR_MAT_PIANIFICAZIONI.OrderByDescending(x => x.DATA_INSERIMENTO).FirstOrDefault();
            if (pian == null || pian.XR_MAT_GIORNI_CONGEDO == null || pian.XR_MAT_GIORNI_CONGEDO.Any() == false)
                return null;

            var ecc = pian.XR_MAT_GIORNI_CONGEDO.Where(x => x.DATA == data).FirstOrDefault();
            return ecc ;
        }

        public static string[] GetIntervalloPermessoMinuti(XR_MAT_GIORNI_CONGEDO giornoCongedo, string matricola)
        {
            if (string.IsNullOrWhiteSpace(giornoCongedo.FRUIZIONE_TURNO)) return null;
            int quarto;
            if (! int.TryParse(giornoCongedo.FRUIZIONE_TURNO.Replace("Q",""),out quarto))
            {
                return null;
            }
            WSDigigapp DigigappService = new WSDigigapp();
            DigigappService.Credentials = new System.Net.NetworkCredential(
               CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
               CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
            var resp = DigigappService.getEccezioni(matricola, giornoCongedo.DATA.ToString("ddMMyyyy"), "BU", 80);
            if (resp.orario != null)
            {
                int presenza = Convert.ToInt32(resp.orario.prevista_presenza);
                if (presenza == 0) return null;

                int presenzaQ = presenza / 4;

                int entrataMinuti = Convert.ToInt32(resp.orario.entrata_iniziale);
                if (entrataMinuti == 0) return null;

                int inizioPermesso = entrataMinuti + ((quarto - 1) * presenzaQ);
                int finePermesso = inizioPermesso + presenzaQ;
                return new string[] { inizioPermesso.ToHH_MM(), finePermesso.ToHH_MM() };
            }
            else
                return null;

        }
        public static string GetNumeroDocumento(WSDigigapp digigappService, string matricola, string eccezione, DateTime dataEccezione)
        {
            var resp = digigappService.getEccezioni(matricola, dataEccezione.ToString("ddMMyyyy"), "BU", 80);
            if (resp.eccezioni == null || resp.eccezioni.Any() == false)
                return null;

            var ec = resp.eccezioni.Where(x => x.cod.Trim() == eccezione).FirstOrDefault();
            if (ec == null || string.IsNullOrWhiteSpace(ec.ndoc)) return null;

            return ec.ndoc;
        }
        public static DateTime? GetDataPagamentoEccezioni(XR_MAT_TASK_IN_CORSO t, MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.WSDew service)
        {
            if (t.DATA_ULTIMO_TENTATIVO != null && t.TERMINATA == true)
                return new DateTime(t.DATA_ULTIMO_TENTATIVO.Value.Year, t.DATA_ULTIMO_TENTATIVO.Value.Month, 1);
            else
                return null;


            MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.CampiTracciatoResponse ResponseCampi =
                    service.GetCampiTracciato((int)t.XR_MAT_ELENCO_TASK.ID_TRACCIATO_DEW,
                    (int)t.XR_MAT_ELENCO_TASK.PROGRESSIVO_TRACCIATO_DEW);

            var campoMese = ResponseCampi.Campi.Where(x => x.NomeCampo == "MM COMPETENZA").FirstOrDefault();
            if (campoMese == null || campoMese.Posizione == null || campoMese.Posizione == 0 || campoMese.Lunghezza == null)
            {
                return null;
            }
            string MM = t.INPUT.Substring((int)campoMese.Posizione, (int)campoMese.Lunghezza);

            var campoAnno = ResponseCampi.Campi.Where(x => x.NomeCampo == "AA COMPETENZA").FirstOrDefault();
            if (campoAnno == null || campoAnno.Posizione == null || campoAnno.Posizione == 0 || campoAnno.Lunghezza == null)
            {
                return null;
            }
            string AA = t.INPUT.Substring((int)campoAnno.Posizione, (int)campoAnno.Lunghezza);
            return new DateTime(Convert.ToInt32(AA), Convert.ToInt32(MM), 1);
        }
        public static List<DateTime> GetDateEccezioni(XR_MAT_TASK_IN_CORSO t)
        {
            List<DateTime> Ldate = new List<DateTime>();

            if (String.IsNullOrWhiteSpace(t.NOTE) || !t.NOTE.Contains("-"))
                return null;

            string date1 = t.NOTE.Split('-')[1];
            if (String.IsNullOrWhiteSpace(date1))
                return null;

            string date2 = t.NOTE.Split('-')[2];
            if (String.IsNullOrWhiteSpace(date2))
                return null;

            DateTime D1;
            bool esito1 = DateTime.TryParseExact(date1, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D1);
            DateTime D2;
            bool esito2 = DateTime.TryParseExact(date2, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D2);

            if (esito1 == false || esito2 == false)
                return null;

            if (D1 == D2)
            {
                Ldate.Add(D1);
                return Ldate;
            }


            DateTime Dcurrent = D1;
            while (true)
            {
                Ldate.Add(Dcurrent);
                if (Dcurrent == D2) break;
                Dcurrent = Dcurrent.AddDays(1);
            }
            return Ldate;

        }
    }
}
