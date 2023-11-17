using System;
using System.Collections.Generic;
using System.Linq;
using myRaiServiceHub.Autorizzazioni;
using MyRaiServiceInterface.MyRaiServiceReference1;

using System.Text;
using myRai.Business;
using myRaiHelper;

namespace myRaiCommonManager
{
    public class AmministrazioneManager
    {




        public static myRaiCommonModel.AmministrazioneModel.BustaPaga GetPagamenti(string matricola, string permesso, DateTime dataContabilizzazione)
        {
            List<myRaiCommonModel.AmministrazioneModel.CedoliniPossibili> lista = myRaiCommonManager.AmministrazioneManager.GetElencoBustePaga(matricola);
            myRaiCommonModel.AmministrazioneModel.BustaPaga resBuste = new myRaiCommonModel.AmministrazioneModel.BustaPaga();
            bool trovato = false;
            DateTime dataRicorrente = dataContabilizzazione;
            if (lista.Any())
            {


                while (!trovato)
                {

                    foreach (myRaiCommonModel.AmministrazioneModel.CedoliniPossibili element in lista.Where(x => x.Codice.StartsWith(dataRicorrente.ToString("yyMM"))))
                    {
                        myRaiCommonModel.AmministrazioneModel.BustaPaga res = myRaiCommonManager.AmministrazioneManager.GetBustaPaga(matricola, element.Codice.Substring(0, 4), element.Codice.Substring(5, 4), element.Codice.Substring(4, 1));

                        string[] voci = res.elencoVoci.Select(x => x.Descrittiva).ToArray();
                        string voc = String.Join("\r\n", voci.ToArray());

                        if (res.elencoVoci.Where(x => x.Descrittiva.Trim().StartsWith(permesso)).Count() > 0)
                        {
                            resBuste = res;
                            trovato = true;
                            break;
                        }

                    }
                    if (Int32.Parse(lista.Last().Codice.Substring(0, 4)) < Int32.Parse(dataRicorrente.ToString("yyMM"))) { return resBuste; };
                    dataRicorrente = dataRicorrente.AddMonths(1);
                }
            }

            return resBuste;




        }
        public static myRaiCommonModel.AmministrazioneModel.BustaPaga GetBustaPaga(string matricola, string dataCompetenza, string dataContabilizzazione, string tipo = " ")
        {
            MyRaiService1Client service = new MyRaiService1Client();
            string userRichiedente = CommonManager.GetCurrentUserMatricola();
            string S = "P956,BC,HRPESSP" + userRichiedente + "000000" + string.Empty.PadLeft(9 - matricola.Length) + matricola + "," + dataCompetenza + tipo + dataContabilizzazione + ",";
            string r = service.ComunicaCICS(S);
            r = r.Substring(10);
            myRaiCommonModel.AmministrazioneModel.BustaPaga resBusta = new myRaiCommonModel.AmministrazioneModel.BustaPaga();
            //RIEPILOGO
            if (string.IsNullOrWhiteSpace(r))
            {
                return resBusta;
            }
            resBusta.intestazione = r.Substring(0, 2) + " - " + r.Substring(3, 47);
            r = r.Substring(50);
            resBusta.competenza = "Mese di contabilizzazione: " + dataCompetenza;
            resBusta.inquadramento = "Inquadramento alla data: " + r.Substring(0, 45).Trim();
            resBusta.matricola = matricola;
            resBusta.dataCompetenza = dataCompetenza;
            resBusta.dataContabilizzazione = dataContabilizzazione;

            string first = dataCompetenza;
            if (Int32.Parse(first.Substring(0, 2)) > 40)
                first = "19" + first;
            else
                first = "20" + first;

            string second = dataContabilizzazione;
            if (Int32.Parse(second.Substring(0, 2)) > 40)
                second = "19" + second;
            else
                second = "20" + second;

            resBusta.DtCompetenza = first.ToDateTime("yyyyMM").ToString("MMMM yyyy");
            resBusta.DtContab = second.ToDateTime("yyyyMM").ToString("MMMM yyyy");
            resBusta.Tipo = tipo;

            List<string> VociRiepilogo = new List<string>() {
                    "Totale Accrediti",
                    "IRPEF",
                    "Previdenza RAI",
                    "Contributi Previd.",
                    "Contributi Assist.",
                    "GESCAL",
                    "Altri Addebiti (vedi singole voci)",
                    "Totale Addebiti",
                    "Arrotondamento",
                    "Netto a Pagare",
                    "ONERI",
                    "COSTO"

                };
            int offset = 34;
            foreach (var item in VociRiepilogo)
            {
                r = r.Substring(11 + offset);
                resBusta.elencoVoci.Add(new myRaiCommonModel.AmministrazioneModel.VociCedolino()
                {
                    Descrittiva = item.Trim(),
                    Sezione = myRaiCommonModel.AmministrazioneModel.SezioneVoci.Riepilogo,
                    Tipo = (r.Substring(0, 1) == "C") ? myRaiCommonModel.AmministrazioneModel.TipoImporto.Credito
                            : (r.Substring(0, 1) == "R") ? myRaiCommonModel.AmministrazioneModel.TipoImporto.Riepilogo
                            : myRaiCommonModel.AmministrazioneModel.TipoImporto.Debito,
                    Valore = r.Substring(1, 10)

                });
                offset = 0;
            }
            r = r.Substring(31);

            //VOCI
            int intNVoci = Int16.Parse(r.Substring(0, 3));
            r = r.Substring(3);
            for (int i = 0; i < intNVoci; i++)
            {
                if (String.IsNullOrWhiteSpace(r)) break;
                resBusta.elencoVoci.Add(new myRaiCommonModel.AmministrazioneModel.VociCedolino()
                {
                    Descrittiva = r.Substring(0, 20).Trim(),
                    Sezione = myRaiCommonModel.AmministrazioneModel.SezioneVoci.Dettaglio,
                    Tipo = (r.Substring(20, 1) == "C") ? myRaiCommonModel.AmministrazioneModel.TipoImporto.Credito
                            : (r.Substring(20, 1) == "R") ? myRaiCommonModel.AmministrazioneModel.TipoImporto.Riepilogo
                            : myRaiCommonModel.AmministrazioneModel.TipoImporto.Debito,
                    Valore = r.Substring(21, 9)

                });

                r = r.Substring(30);
            }

            return resBusta;
            //service.ComunicaCICS();

        }

        public static List<myRaiCommonModel.AmministrazioneModel.CedoliniPossibili> GetElencoBustePaga(string matricola)
        {
            MyRaiService1Client service = new MyRaiService1Client();
            string userRichiedente = CommonManager.GetCurrentUserMatricola();
            //"P956,BI,HRAWEBP654552   000000654552,, "
            string S = "P956,BI,HRAWEBP" + userRichiedente + string.Empty.PadLeft(9 - matricola.Length) + "000000" + matricola + ",,";
            string r = service.ComunicaCICS(S);
            r = r.Substring(10);
            List<myRaiCommonModel.AmministrazioneModel.CedoliniPossibili> listaRes = new List<myRaiCommonModel.AmministrazioneModel.CedoliniPossibili>();
            //RIEPILOGO


            //VOCIintBI = Len(strBI) / 30
            int intNVoci = r.Length / 30;

            for (int i = 0; i < intNVoci; i++)
            {
                string line = r.Substring(i * 30, 30);
                string first = line.Substring(0, 4);
                int vv = 0;
                if (Int32.TryParse(first.Substring(0, 2), out vv) && vv > 40)
                {
                    first = "19" + first;
                }
                else
                    first = "20" + first;

                string second = line.Substring(5, 4);
                int vv2 = 0;
                if (Int32.TryParse(second.Substring(0, 2), out vv2) && vv2 > 40)
                {
                    second = "19" + second;
                }
                else
                    second = "20" + second;

                listaRes.Add(new myRaiCommonModel.AmministrazioneModel.CedoliniPossibili()
                {
                    Descrittiva = (first.ToDateTime("yyyyMM").ToString("MMMM") + " " + line.Substring(9, 20)).Trim(),
                    Codice = line.Substring(0, 9),

                });

                //r = r.Substring(30);
            }

            return listaRes;
            //service.ComunicaCICS();

        }

    }






}
