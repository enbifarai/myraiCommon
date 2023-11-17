using myRaiCommonTasks.it.rai.servizi.svildigigappws;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace myRaiCommonTasks
{
    public class Resoconti
    {
        public static string UpdateResocontiGetPresenze(string sede, List<DateTime> LdateInizio)
        {
            string response = null;
            string[] utenteConv = CommonTasks.GetParametri<string>(CommonTasks.EnumParametriSistema.UtentePerConvalida);
            WSDigigapp service = service = (WSDigigapp)CommonTasks.ServiceCreate(AsmxServices.Digigapp);
            service.Timeout = 600000;

            using (myRaiData.digiGappEntities db = new myRaiData.digiGappEntities())
            {
                foreach (DateTime Data_Inizio in LdateInizio)
                {
                    DateTime Data_Fine = Data_Inizio.AddDays(6);

                    var sw = new Stopwatch();
                    sw.Start();
                    presenzeResponse presenze = service.getPresenzeNoPDF(utenteConv[0], "*", Data_Inizio.ToString("ddMMyyyy"), Data_Fine.ToString("ddMMyyyy"), sede, 75, "**");
                    sw.Stop();
                    CommonTasks.Log(sede + " " + Data_Inizio.ToString("ddMMyyyy") + "-" + Data_Fine.ToString("ddMMyyyy") + " OK in : " + sw.ElapsedMilliseconds + "ms");


                    myRaiData.MyRai_Resoconti_GetPresenze old = db.MyRai_Resoconti_GetPresenze.Where(x => x.sede == sede && x.data_inizio == Data_Inizio && x.data_fine == Data_Fine).FirstOrDefault();
                    if (old != null)
                    {
                        var presCache = Newtonsoft.Json.JsonConvert.DeserializeObject<presenzeResponse>(old.contenuto);
                        var pdf = db.DIGIRESP_Archivio_PDF.Where(x => x.tipologia_pdf == "P" && x.sede_gapp == sede && x.data_inizio == Data_Inizio && x.data_fine == Data_Fine).FirstOrDefault();
                        if (pdf != null && !string.IsNullOrWhiteSpace(pdf.contenuto_eccezioni))
                        {
                            var presPDF = Newtonsoft.Json.JsonConvert.DeserializeObject<presenzeResponse>(pdf.contenuto_eccezioni);
                            if (presCache != null && presPDF != null && presCache.periodi != null && presPDF.periodi != null && presCache.periodi.Any() && presPDF.periodi.Any())
                            {
                                foreach (var p in presPDF.periodi)
                                {
                                    var pcache = presCache.periodi.Where(x => x.dipendente.matricola == p.dipendente.matricola).FirstOrDefault();
                                    if (pcache != null)
                                    {
                                        //for ( int i= 0;i< pcache.giornate.Length;i++)
                                        //{
                                        //    string EccCache= string.Join(",", pcache.giornate[i].eccezioni.Select(x => x.cod).ToArray());
                                        //}
                                        // if (pcache.deltaTotale != p.deltaTotale)
                                        //  {
                                            old.data_aggiornamento = DateTime.Now;
                                            old.contenuto = Newtonsoft.Json.JsonConvert.SerializeObject(presenze);
                                        continue;
                                        //}
                                        //else
                                        //{
                                        //    response += sede+ ": Nessuna differenza nel periodo " + Data_Inizio.ToString("dd/MM/yyyy") + "-" + Data_Fine.ToString("dd/MM/yyyy") + ", rigenerazione non necessaria\n";
                                        //}
                                    }
                                }
                            }
                        }
                        else
                        {
                            old.data_aggiornamento = DateTime.Now;
                            old.contenuto = Newtonsoft.Json.JsonConvert.SerializeObject(presenze);
                        }
                    }
                    else
                    {
                        myRaiData.MyRai_Resoconti_GetPresenze p = new myRaiData.MyRai_Resoconti_GetPresenze()
                        {
                            data_aggiornamento = DateTime.Now,
                            data_inizio = Data_Inizio,
                            data_fine = Data_Fine,
                            matricola_aggiornamento = utenteConv[0],
                            sede = sede,
                            contenuto = Newtonsoft.Json.JsonConvert.SerializeObject(presenze)
                        };
                        db.MyRai_Resoconti_GetPresenze.Add(p);

                        var newpr = Newtonsoft.Json.JsonConvert.DeserializeObject<presenzeResponse>(p.contenuto);
                    }
                }
                db.SaveChanges();
            }
            return response;
        }
    }
}
