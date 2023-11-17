using myRaiData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static myRaiCommonTasks.CommonTasks;

namespace myRaiCommonTasks
{
    public class Arretrati2019
    {
        public static float GetFECE_MNCE_MRCE_MenoDonateSuFoglioExcel(string matricola,
          float? v1=null
            )
        {
            if (v1==null) v1 = GetFECE_MNCE_MRCE(matricola);
            float v = GetDonateDaFoglioExcel(matricola);
           
            if ((v1 - v) < 0) return 0;
            else return ((float)v1 - v);
        }

        public static float GetDonateDaFoglioExcel(string matricola)
        {
            var db = new digiGappEntities();
            var dip = db.MyRai_ArretratiExcel2019.Where(x => x.matricola == matricola).FirstOrDefault();
            if (dip == null || dip.donate == null) return 0;
            else return (float)dip.donate;
        }
        public static  MyRaiServiceInterface.MyRaiServiceReference1.GetContatoriEccezioniResponse 
            GetFECE_MNCE_MRCE_Details(string matricola, int anno)
        {
            MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client cl =
                           new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();

            cl.ClientCredentials.Windows.ClientCredential =
                new System.Net.NetworkCredential(GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                                                 GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            //string[] eccArr = "FECE,MNCE,MRCE,FEDO,MNDO,MRDO".Split(',');
            string[] eccArr = GetParametro<string>(EnumParametriSistema.Regalate).Split(',');

            var cont =
                           cl.GetContatoriEccezioni(matricola,
                                                       new DateTime(anno, 1, 1),
                                                       new DateTime(anno, 12, 31),
                                                       eccArr);
            return cont;
        }
        public static float GetFECE_MNCE_MRCE(string matricola
            
            )
        {
            MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client cl =
                           new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();

            cl.ClientCredentials.Windows.ClientCredential =
                new System.Net.NetworkCredential(GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                                                 GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            //string[] eccArr = "FECE,MNCE,MRCE,FEDO,MNDO,MRDO".Split(',');
            string[] eccArr= GetParametro<string>(EnumParametriSistema.Regalate).Split(',');

           var cont =
                           cl.GetContatoriEccezioni(matricola,
                                                       new DateTime(2020, 1, 1),
                                                       new DateTime(2020, 12, 31),
                                                       eccArr);

            if (cont == null || cont.ContatoriEccezioni == null || cont.ContatoriEccezioni.Length == 0)
                return 0;

            float totaleDonati = 0;
            for (int i = 0; i < eccArr.Length -1; i++)
            {
                if (cont.ContatoriEccezioni.Length > i && cont.ContatoriEccezioni[i] != null)
                {
                    string t = cont.ContatoriEccezioni[i].Totale;
                    if (!String.IsNullOrWhiteSpace(t))
                    {
                        float tot = 0;
                        if (float.TryParse(t, out tot))
                        {
                            totaleDonati += tot;
                        }
                    }
                }
            }
            return totaleDonati;

        }
        public static float[] GetGiornalisti_RR_RF_FE(float RRap, float RFap, float FEap, float ArretratiDaMettereDaFoglioExcel,
           string matricola, int? FECE = null)
        {
            float RRdaMetterePerFoglioExcel = 0;
            float RFdaMetterePerFoglioExcel = 0;
            float FEdaMetterePerFoglioExcel = 0;
            if (ArretratiDaMettereDaFoglioExcel > 0)
            {
                if (FEap > 0)
                {
                    if (FEap < ArretratiDaMettereDaFoglioExcel)
                        FEdaMetterePerFoglioExcel = FEap;
                    else
                        FEdaMetterePerFoglioExcel = ArretratiDaMettereDaFoglioExcel;
                }
                if (FEdaMetterePerFoglioExcel < ArretratiDaMettereDaFoglioExcel)
                {
                    RRdaMetterePerFoglioExcel = ArretratiDaMettereDaFoglioExcel - FEdaMetterePerFoglioExcel;

                    MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client cl =
                          new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();

                    cl.ClientCredentials.Windows.ClientCredential =
                        new System.Net.NetworkCredential(GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                                                         GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
                   var cont= cl.GetContatoriEccezioni(matricola, new DateTime(2020, 1, 1), DateTime.Now.Date, 
                       new string[] { "MR" });
                    float MR = 0;
                    if (cont != null && cont.ContatoriEccezioni != null && cont.ContatoriEccezioni.Any())
                    {
                        if (! String.IsNullOrWhiteSpace(cont.ContatoriEccezioni[0].Totale))
                        {
                            float.TryParse(cont.ContatoriEccezioni[0].Totale, out MR);
                        }
                    }
                    RRdaMetterePerFoglioExcel += MR;
                }
            }

            return new float[] { RRdaMetterePerFoglioExcel, RFdaMetterePerFoglioExcel, FEdaMetterePerFoglioExcel };

        }
        public static float[] GetRR_RF_FE(float RRap, float RFap, float ArretratiDaMettereDaFoglioExcel,
            string matricola, int? FECE = null)
        {
            float RRdaMetterePerFoglioExcel = 0;
            float RFdaMetterePerFoglioExcel = 0;
            float FEdaMetterePerFoglioExcel = 0;

            if (ArretratiDaMettereDaFoglioExcel > 0)
            {

                if (RRap > 0)
                {
                    if (RRap < ArretratiDaMettereDaFoglioExcel)
                        RRdaMetterePerFoglioExcel = RRap;
                    else
                        RRdaMetterePerFoglioExcel = ArretratiDaMettereDaFoglioExcel;

                    RRdaMetterePerFoglioExcel = (int)RRdaMetterePerFoglioExcel;
                }
                if (RFap > 0)
                {
                    if (RRdaMetterePerFoglioExcel < ArretratiDaMettereDaFoglioExcel)
                    {
                        float DaCoprireConRF = ArretratiDaMettereDaFoglioExcel - RRdaMetterePerFoglioExcel;
                        if (RFap < DaCoprireConRF)
                            RFdaMetterePerFoglioExcel = RFap;
                        else
                            RFdaMetterePerFoglioExcel = DaCoprireConRF;

                        RFdaMetterePerFoglioExcel = (int)RFdaMetterePerFoglioExcel;
                    }
                }
                FEdaMetterePerFoglioExcel = ArretratiDaMettereDaFoglioExcel - RRdaMetterePerFoglioExcel - RFdaMetterePerFoglioExcel;
            }
            return new float[] { RRdaMetterePerFoglioExcel, RFdaMetterePerFoglioExcel, FEdaMetterePerFoglioExcel };

        }

    }
}
