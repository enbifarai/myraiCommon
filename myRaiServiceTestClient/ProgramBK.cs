using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using myRaiData;
using myRaiServiceTestClient.it.rai.servizi.svildigigappws;
using ClosedXML.Excel;
using System.Text.RegularExpressions;
using System.Data.Objects;
using System.Collections;
using System.Collections.Generic;
using MyRaiServiceInterface.MyRaiServiceReference1;

namespace myRaiServiceTestClient
{
    public class persona
    {
        private string test;
        public string nome { get; set; }
        public string cognome { get; set; }
       

    }

    public class ec
    {
        public string codice { get; set; }
        public string descrizione { get; set; }
        public string cose { get; set; }
        public string vincoli { get; set; }
        public string note { get; set; }
        public int idTipoAssenza { get; set; }
    }

    public class IDcouple
    {
        public int IdRich { get; set; }
        public int IdEccRich { get; set; }
        public DateTime rilevato { get; set; }

    }

    class ProgramBK
    {
        #region GetReport_Carenza_MP
        private static string CalcolaSaldo(string t1, string t2)
        {
            string result = "";

            try
            {
                #region primo TimeSpan
                string oreSpanT1 = t1.Substring(0, 2);
                string minutiSpanT1 = t1.Substring(3, 2);

                int oreT1 = Int32.Parse(oreSpanT1);
                int minutiT1 = Int32.Parse(minutiSpanT1);

                // Days, hours, minutes, seconds, milliseconds.
                TimeSpan spanT1 = new TimeSpan(0, oreT1, minutiT1, 0, 0);
                #endregion

                #region secondo TimeSpan
                string oreSpanT2 = t2.Substring(0, 2);
                string minutiSpanT2 = t2.Substring(3, 2);

                int oreT2 = Int32.Parse(oreSpanT2);
                int minutiT2 = Int32.Parse(minutiSpanT2);

                // Days, hours, minutes, seconds, milliseconds.
                TimeSpan spanT2 = new TimeSpan(0, oreT2, minutiT2, 0, 0);
                #endregion

                TimeSpan differenza = spanT1.Subtract(spanT2);

                result = String.Format("{0:00}:{1:00}", (differenza.Hours + (differenza.Days * 24)), differenza.Minutes);

            }
            catch (Exception ex)
            {
            }

            return result;
        }

        private static int CalcolaOccorrenze(List<Report_Lista_Item> items)
        {
            int count = 0;
            if (items != null && items.Any())
            {
                count = items.Where(i => i.Minuti > 0).Count();
            }

            return count;
        }

        private static string ConvertiTempoInStringa(int ore, int minuti)
        {
            if (minuti > 60)
            {
                int temp = (minuti / 60);
                ore += temp;
                minuti -= (temp * 60);
            }
            else if (minuti == 60)
            {
                ore += 1;
                minuti = 0;
            }

            return String.Format("{0:00}:{1:00}", ore, minuti);
        }

        private static string CalcolaOreTotali(List<Report_Lista_Item> items)
        {
            int minuti = 0;
            int ore = 0;
            string tempo = "00:00";

            if (items != null && items.Any())
            {
                foreach (var itm in items)
                {
                    minuti += itm.Minuti;
                }

                if (minuti > 60)
                {
                    ore = (minuti / 60);
                    minuti -= (ore * 60);
                }

                if (minuti == 60)
                {
                    ore = 1;
                    minuti = 0;
                }

                tempo = String.Format("{0:00}:{1:00}", ore, minuti);
            }

            return tempo;
        }

        private static string CalcolaOreTotali ( List<ItemCarMagPresenza> items , string tipo )
        {
            int ore = 0;
            int minuti = 0;
            int tempOre = 0;
            int tempMinuti = 0;

            string tempo = "00:00";

            if ( items != null && items.Any( ) )
            {

                List<ItemCarMagPresenza> list = new List<ItemCarMagPresenza>( );

                if ( tipo == "CAR" )
                {
                    foreach ( var itm in items )
                    {
                        tempOre = Int32.Parse( itm.CAR.Trim( ).Substring( 0 , 2 ) );
                        tempMinuti = Int32.Parse( itm.CAR.Trim( ).Substring( 3 , 2 ) );

                        ore += tempOre;
                        minuti += tempMinuti;
                    }
                }
                else
                {
                    foreach ( var itm in items )
                    {
                        tempOre = Int32.Parse( itm.MP.Trim( ).Substring( 0 , 2 ) );
                        tempMinuti = Int32.Parse( itm.MP.Trim( ).Substring( 3 , 2 ) );

                        ore += tempOre;
                        minuti += tempMinuti;
                    }
                }

                if ( minuti > 60 )
                {
                    int tempH = ( minuti / 60 );
                    ore += tempH;
                    minuti -= ( tempH * 60 );
                }

                if ( minuti == 60 )
                {
                    ore += 1;
                    minuti = 0;
                }

                tempo = String.Format( "{0:00}:{1:00}" , ore , minuti );
            }

            return tempo;
        }

        public class Report_Lista_Item
        {
            public DateTime Giorno { get; set; }
            public int Minuti { get; set; }
        }

        public class ItemCarMagPresenza
        {
            public int Settimana { get; set; }
            public string CAR { get; set; }
            public string MP { get; set; }
        }

        public class Report_Item
        {
            public string Matricola { get; set; }
            public string Nominativo { get; set; }
            public List<Report_Lista_Item> ListaPOH { get; set; }
            public List<Report_Lista_Item> ListaROH { get; set; }
            public string TotaleOreLista1 { get; set; }
            public string TotaleOreLista2 { get; set; }
            public string TotaleOreLista3 { get; set; }
            public string Saldo { get; set; }
            public int NumeroOccorrenze { get; set; }
            public List<Report_Lista_Item> ListaSTR { get; set; }
            public List<Report_Lista_Item> ListaSTRF { get; set; }
            public List<Report_Lista_Item> ListaRE20 { get; set; }
            public List<Report_Lista_Item> ListaRE22 { get; set; }
            public List<Report_Lista_Item> ListaRE25 { get; set; }
            public List<ItemCarMagPresenza> ListaCarMp { get; set; }
        }

        public class Report_Carenza_MP_Response
        {
            /// <summary>
            /// Esito della richiesta
            /// </summary>
            public bool Esito { get; set; }

            /// <summary>
            /// Eventuale errore
            /// </summary>
            public string Errore { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public List<Report_Item> Risposta { get; set; }
        }

        public static Report_Carenza_MP_Response GetReport_Carenza_MP(string matricola, string sede, int mese, int anno)
        {
            Report_Carenza_MP_Response result = new Report_Carenza_MP_Response();
            DateTime dt = new DateTime(anno, mese, 1);
            result.Risposta = new List<Report_Item>();

            List<Report_Item> rList = new List<Report_Item>();

            try
            {
                MyRaiService1Client client32 = new MyRaiService1Client();

                var resp = client32.presenzeGiornaliere(matricola, sede, dt.ToString("ddMMyyyy"));

                if (resp.esito == false && !String.IsNullOrEmpty(resp.errore))
                {
                    result.Esito = false;
                    result.Errore = resp.errore;
                    result.Risposta = null;
                    return result;
                }

                if (resp.dati == null || resp.dati.Count() == 0)
                {
                    result.Esito = true;
                    result.Errore = "Non ci sono dati disponibili";
                    result.Risposta = null;
                    return result;
                }


                foreach (var item in resp.dati)
                {
                    DateTime dtStart = new DateTime(anno, mese, 1);
                    DateTime dtEnd = dtStart.AddMonths(1);
                    dtEnd = dtEnd.AddDays(-1);
                    List<ItemCarMagPresenza> listaCarMp = new List<ItemCarMagPresenza>();

                    var presenze = client32.PresenzeSettimanali(matricola, dtStart.ToString("ddMMyyyy"), dtEnd.ToString("ddMMyyyy"));
                    if (presenze.esito)
                    {
                        if (presenze.dati.items != null && presenze.dati.items.Any())
                        {
                            int oreCarenza = 0;
                            int minutiCarenza = 0;
                            int oreMaggiorPresenza = 0;
                            int minutiMaggiorPresenza = 0;
                            int tempOre = 0;
                            int tempMinuti = 0;
                            int conteggioGiornate = 1;
                            int settimana = 1;

                            while (dtStart.Date <= dtEnd.Date)
                            {
                                var giornata = presenze.dati.items.Where(g => g.data == dtStart).FirstOrDefault();

                                tempOre = Int32.Parse(giornata.carenza.Trim().Substring(0, 2));
                                tempMinuti = Int32.Parse(giornata.carenza.Trim().Substring(3, 2));

                                oreCarenza += tempOre;
                                minutiCarenza += tempMinuti;

                                tempOre = Int32.Parse(giornata.maggiorPresenza.Trim().Substring(0, 2));
                                tempMinuti = Int32.Parse(giornata.maggiorPresenza.Trim().Substring(3, 2));

                                oreMaggiorPresenza += tempOre;
                                minutiMaggiorPresenza += tempMinuti;

                                // se è terminata la settimana oppure 
                                // è l'ultimo giorno del mese
                                if (conteggioGiornate == 7 ||
                                    dtStart.AddDays(1).Date == dtEnd.Date)
                                {
                                    conteggioGiornate = 1;
                                    listaCarMp.Add(new ItemCarMagPresenza()
                                    {
                                        CAR = ConvertiTempoInStringa(oreCarenza, minutiCarenza),
                                        MP = ConvertiTempoInStringa(oreMaggiorPresenza, minutiMaggiorPresenza),
                                        Settimana = settimana
                                    });

                                    settimana++;
                                    oreCarenza = 0;
                                    minutiCarenza = 0;
                                    oreMaggiorPresenza = 0;
                                    minutiMaggiorPresenza = 0;
                                }
                                else
                                {
                                    conteggioGiornate++;
                                }
                                dtStart = dtStart.AddDays(1);
                            }

                            string t1 = CalcolaOreTotali(listaCarMp, "MP");
                            string t2 = CalcolaOreTotali(listaCarMp, "CAR");

                            rList.Add(new Report_Item()
                            {
                                Matricola = item.matricola,
                                Nominativo = item.nominativo,
                                ListaCarMp = listaCarMp,
                                Saldo = CalcolaSaldo(t1, t2),
                                NumeroOccorrenze = 0,
                                TotaleOreLista1 = t1,
                                TotaleOreLista2 = t2
                            });
                        }
                    }
                }

                client32.Close();

            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.ToString();
                result.Risposta = null;
                return result;
            }

            result.Esito = true;
            result.Errore = null;
            result.Risposta = new List<Report_Item>();
            result.Risposta = rList;

            return result;
        }

        #endregion



        public static void AnalizzaLogEliminate()
        {


            var db = new digiGappEntities();

            var list = db.MyRai_LogAzioni.Where(x => x.operazione == "ELIMINATA SU GAPP").OrderByDescending(x => x.Id).ToList();

            List<IDcouple> ListIdRichiesta = new List<IDcouple>();

            Regex R = new Regex("id\\srich:(\\d+)");
            Regex R2 = new Regex("ecc_rich:(\\d+)");
            //id rich:64531 id ecc_rich:65273
            foreach (var azione in list)
            {
                Match M = R.Match(azione.descrizione_operazione);
                Match M2 = R2.Match(azione.descrizione_operazione);

                if (!ListIdRichiesta.Any(x => x.IdRich == Convert.ToInt32(M.Groups[1].Value) && x.IdEccRich == Convert.ToInt32(M2.Groups[1].Value)))
                    ListIdRichiesta.Add(
                        new IDcouple()
                        {
                            IdRich = Convert.ToInt32(M.Groups[1].Value),
                            IdEccRich = Convert.ToInt32(M2.Groups[1].Value),
                            rilevato = azione.data
                        });

            }
            WSDigigapp service = new WSDigigapp();
            service.Credentials = new System.Net.NetworkCredential("srvruofpo", "zaq22?mk");
            string filename = "log_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";

            LogEliminate("IDrichiesta;IDeccezione;Matricola;Sede;Eccezione;DataEccezione;SuGapp;Ndoc DB; Ndoc Gapp;Rilevato il", filename);

            int c = 0;
            foreach (IDcouple rich in ListIdRichiesta)
            {
                c++;
                MyRai_Richieste r = db.MyRai_Richieste.Find(rich.IdRich);
                MyRai_Eccezioni_Richieste er = r.MyRai_Eccezioni_Richieste.Where(x => x.id_eccezioni_richieste == rich.IdEccRich).FirstOrDefault();
                if (r == null || er == null) continue;

                Console.WriteLine("Checking gapp " + c + "/" + ListIdRichiesta.Count() + " " + r.matricola_richiesta + " " + er.data_eccezione.ToString("ddMMyyyy") + " ...");

                dayResponse response = service.getEccezioni(r.matricola_richiesta, er.data_eccezione.ToString("ddMMyyyy"), "BU", 75);
                var eccGapp = response.eccezioni.Where(x => x.cod == er.cod_eccezione).FirstOrDefault();

                string linea = rich.IdRich.ToString() + ";" + rich.IdEccRich + ";" + r.matricola_richiesta + ";" + r.codice_sede_gapp + ";" + er.cod_eccezione + ";" + er.data_eccezione.ToString("dd/MM/yyyy");
                if (eccGapp == null)
                {
                    LogEliminate(linea + ";NON PRESENTE;" + er.numero_documento + ";" + "-;" + rich.rilevato.ToString("dd/MM/yyyy HH.mm"), filename);
                    continue;
                }
                else
                {
                    LogEliminate(linea + ";PRESENTE;" + er.numero_documento + ";" + eccGapp.ndoc + ";" + rich.rilevato.ToString("dd/MM/yyyy HH.mm"), filename);
                }
            }
        }

        /// <summary>
        /// Verifica lo stato delle richieste in GAPP
        /// </summary>
        static void VerificaStatoRichiestaInGapp()
        {
            DateTime D = new DateTime(2018, 10, 1);
            var db = new digiGappEntities();

            var daVerificare = db.MyRai_Richieste.Where(x => x.MyRai_Eccezioni_Richieste.Any(a => a.id_stato == 10 && a.data_eccezione < D)).OrderBy(x => x.periodo_dal).ThenBy(x => x.matricola_richiesta).ToList();

            string lineaReport = "";

            foreach (var r in daVerificare)
            {
                string matr = r.matricola_richiesta;

                foreach (var rigaEcc in r.MyRai_Eccezioni_Richieste.ToList())
                {
                    it.rai.servizi.svildigigappws.WSDigigapp service = new it.rai.servizi.svildigigappws.WSDigigapp();
                    var response = service.getEccezioni(matr, rigaEcc.data_eccezione.ToString("ddMMyyyy"), "BU", 75);
                    var eccGapp = response.eccezioni.Where(x => x.cod == rigaEcc.cod_eccezione).FirstOrDefault();

                    lineaReport += matr + " ; " + rigaEcc.data_eccezione.ToString("dd/MM/yyyy") + " ; " +
                        "IDRICHIESTA:" + rigaEcc.id_richiesta + " ; " +
                    "ID:" + rigaEcc.id_eccezioni_richieste + " ; " +
                    rigaEcc.cod_eccezione + " ; ndoc DB:" + rigaEcc.numero_documento_riferimento + " ; " +
                    (eccGapp != null ? "Y" : "N") + " ; " +
                    (eccGapp != null ? "ndoc Gapp:" + eccGapp.ndoc : "") + " ; " +
                    (eccGapp == null ? "" : (Convert.ToInt32(eccGapp.ndoc) == Convert.ToInt32(rigaEcc.numero_documento_riferimento) ? "<--------------------" : "")) +
                    "\r\n";

                    //lineaReport += string.Format( "Matricola :{0} - Richiesta: {1} - Stato Richiesta: {2} - Eccezione Richiesta: {3} - Stato eccezione richiesta: {4} - Presente in GAPP: {5} - Numero documento di riferimento: {6} - Azione: {7}", matr, rigaEcc.id_richiesta, r.id_stato, rigaEcc.id_eccezioni_richieste, rigaEcc.id_stato, ( eccGapp != null ? "Y" : "N" ), rigaEcc.numero_documento_riferimento, rigaEcc.azione ) + "\r\n";

                }
            }
            System.IO.File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\reportEcc.txt", lineaReport);
        }

        static void VerificaStorni()
        {
            DateTime D = new DateTime(2018, 10, 1);
            var db = new digiGappEntities();
            var storniApp = db.MyRai_Richieste.Where(x => x.MyRai_Eccezioni_Richieste.Any(a => a.azione == "C"
                            && a.id_stato == 20 &&
                               a.data_eccezione >= D))
                               .OrderBy(x => x.periodo_dal).ThenBy(x => x.matricola_richiesta)
                               .ToList();
            string lineaReport = "";
            foreach (var r in storniApp)
            {
                string matr = r.matricola_richiesta;
                var rigaStorno = r.MyRai_Eccezioni_Richieste.Where(x => x.azione == "C").FirstOrDefault();

                it.rai.servizi.svildigigappws.WSDigigapp service = new it.rai.servizi.svildigigappws.WSDigigapp();
                var response = service.getEccezioni(matr, rigaStorno.data_eccezione.ToString("ddMMyyyy"), "BU", 75);
                var eccGapp = response.eccezioni.Where(x => x.cod == rigaStorno.cod_eccezione).FirstOrDefault();

                lineaReport += matr + " ; " + rigaStorno.data_eccezione.ToString("dd/MM/yyyy") + " ; " +
                    "ID:" + rigaStorno.id_eccezioni_richieste + " ; " +
                    rigaStorno.cod_eccezione + " ; ndoc DB:" + rigaStorno.numero_documento_riferimento + " ; " +
                    (eccGapp != null ? "Y" : "N") + " ; " +
                    (eccGapp != null ? "ndoc Gapp:" + eccGapp.ndoc : "") + " ; " +
                    (eccGapp == null ? "" : (Convert.ToInt32(eccGapp.ndoc) == Convert.ToInt32(rigaStorno.numero_documento_riferimento) ? "<--------------------" : "")) +
                    "\r\n";

                //if (eccGapp != null && Convert.ToInt32(eccGapp.ndoc) == Convert.ToInt32(rigaStorno.numero_documento_riferimento))
                //{
                //    string dalle = "-";
                //    if (rigaStorno.dalle != null) dalle = ((DateTime)rigaStorno.dalle).ToString("HH:mm");

                //    string alle = "-";
                //    if (rigaStorno.alle != null) dalle = ((DateTime)rigaStorno.alle).ToString("HH:mm");

                //    string[] args = new string[] { "del", rigaStorno.MyRai_Richieste.matricola_richiesta,
                //    rigaStorno.data_eccezione.ToString("ddMMyyyy"),
                //    rigaStorno.cod_eccezione,
                //    dalle,
                //    alle,
                //    "*",
                //    rigaStorno.numero_documento_riferimento.ToString(),
                //    rigaStorno.quantita.ToString()
                //    };

                //    Cancella(args);
                //}
            }
            System.IO.File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\report.txt", lineaReport);
        }

        public static void Cancella(string[] args)
        {


            //C:\Users\eam0708\Desktop\qg\qg\bin\Debug>qg del 103650 18052018 SEH 00:00 00:27 * 41663
            it.rai.servizi.svildigigappws.WSDigigapp service = new it.rai.servizi.svildigigappws.WSDigigapp();

            string matr = args[1];
            string data = args[2];
            string cod = args[3].PadRight(4, ' ');
            string dalle = args[4] != "-" ? calcolaMinuti(args[4]).ToString().PadLeft(4, '0') : "";
            string alle = args[5] != "-" ? calcolaMinuti(args[5]).ToString().PadLeft(4, '0') : "";
            string storno = args[6];
            string ndoc = args[7].PadLeft(6, '0');
            string quantita = args[8];

            var resp = service.getEccezioni(matr, data, "BU", 70);
            string oldData = resp.data.Substring(142, 62) + resp.giornata.tipoDipendente;


            var u = service.updateEccezione(
                matr,
                data,
                cod,
                oldData,
                quantita,
                dalle,
                alle,
                "",
                storno[0],
                ndoc,
                "", "", "", "", "", "", 75);

            Console.WriteLine("Cod Errore:" + u.codErrore + " - Desc Errore:" + u.descErrore + " - raw:" + u.rawInput);
        }

        public static void AggiornaPDF()
        {
            var db = new digiGappEntities();
            int[] ids = new int[] { 6007 };

            foreach (int id in ids)
            {
                byte[] buff = System.IO.File.ReadAllBytes("d:\\tho\\" + id.ToString() + ".pdf");
                var pdf = db.DIGIRESP_Archivio_PDF.Where(x => x.ID == id).FirstOrDefault();
                if (pdf != null)
                    pdf.pdf = buff;

            }
            db.SaveChanges();
        }

        public class miaClass
        {
            public string Matricola { get; set; }
            public string Data { get; set; }
            public string Cod_eccezione { get; set; }
        }

        public static void mi()
        {
            string lineaReport = "";

            string input = "015900,2018-10-25,UMH|" +
"023860,2018-10-24,UMH|" +
"030782,2018-10-24,GAVE|" +
"033210,2018-10-24,UMH|" +
"033210,2018-10-26,UMH|" +
"064042,2018-10-24,UMH|" +
"070770,2018-10-23,URH|" +
"074648,2018-10-25,PFQ|" +
"086430,2018-10-25,PRQ|" +
"093128,2018-10-26,SEH|" +
"111079,2018-10-25,UMH|" +
"111079,2018-10-24,UMH|" +
"158135,2018-10-23,URH|" +
"158135,2018-10-23,RMTR|" +
"158135,2018-10-23,LNH5|" +
"168065,2018-10-25,SEH |" +
"175820,2018-10-23,UMH|" +
"175820,2018-10-24,PGM|" +
"175820,2018-10-24,UMH|" +
"178646,2018-10-24,UMH|" +
"178646,2018-10-25,UMH|" +
"178646,2018-10-25,PFQ|" +
"198571,2018-10-25,UMH|" +
"206191,2018-10-23,PFQ|" +
"206191,2018-10-24,PFQ|" +
"206191,2018-10-25,FE|" +
"206191,2018-10-26,FE|" +
"219672,2018-10-25,UMH|" +
"298927,2018-10-24,UMH|" +
"298927,2018-10-23,UMH|" +
"312097,2018-11-02,PR|" +
"324960,2018-10-30,FS|" +
"343617,2018-10-23,SEH|" +
"343617,2018-10-25,SEH|" +
"348946,2018-10-25,UMH|" +
"350585,2018-10-22,RICO|" +
"352710,2018-10-24,UMH|" +
"368809,2018-10-15,SEH|" +
"368809,2018-10-16,SEH|" +
"395772,2018-10-16,SEH|" +
"442916,2018-10-25,UMH|" +
"442916,2018-10-24,UMH|" +
"451688,2018-10-24,PRQ|" +
"455185,2018-10-23,URH|" +
"455185,2018-10-23,RICO|" +
"455185,2018-10-24,URH|" +
"455185,2018-10-24,RICO|" +
"455185,2018-10-22,URH|" +
"455185,2018-10-22,RICO|" +
"455185,2018-10-23,PFQ|" +
"455185,2018-10-19,SEH|" +
"455185,2018-10-25,URH|" +
"455185,2018-10-25,RICO|" +
"455185,2018-10-25,PFQ|" +
"488304,2018-10-26,SEH|" +
"508935,2018-10-23,UMH|" +
"508935,2018-10-22,UMH|" +
"566871,2018-10-23,SEH|" +
"575480,2018-10-26,PRQ|" +
"581598,2018-10-25,UMH|" +
"593945,2018-10-24,SEH|" +
"593945,2018-10-24,URH|" +
"593945,2018-10-24,RICO|" +
"593945,2018-10-24,GAVU|" +
"596701,2018-10-25,SEH|" +
"596701,2018-10-26,SEH|" +
"614300,2018-10-24,UMH|" +
"615660,2018-10-25,FF|" +
"615660,2018-10-24,UMH|" +
"633421,2018-10-15,TN35|" +
"633421,2018-10-16,GAVE|" +
"656420,2018-10-24,FO|" +
"682350,2018-10-24,UMH|" +
"717027,2018-10-24,UMH|" +
"724528,2018-10-25,URH|" +
"724528,2018-10-25,RICO|" +
"748040,2018-10-26,UMH|" +
"782460,2018-10-24,SEH|" +
"782460,2018-10-25,URH|" +
"782460,2018-10-25,RICO|" +
"783695,2018-10-25,FS|" +
"783695,2018-10-23,PRQ|" +
"786393,2018-10-24,UMH|" +
"786393,2018-10-25,UMH|" +
"800422,2018-10-21,DH40|" +
"822030,2018-10-25,UMH|" +
"822333,2018-10-26,PRQ|" +
"877040,2018-10-25,UMH|" +
"877040,2018-10-30,PRQ|" +
"877040,2018-10-31,PRQ|" +
"905960,2018-10-24,TN35|" +
"914590,2018-10-25,URH|" +
"914590,2018-10-25,RICO|" +
"917659,2018-10-24,SEH|" +
"961680,2018-10-24,RMTR|" +
"961680,2018-10-24,TN35|" +
"983466,2018-10-25,URH|" +
"983466,2018-10-25,RICO|" +
"983840,2018-10-26,UMH";

            List<miaClass> parametriInput = new List<miaClass>();

            List<string> split1 = input.Split('|').ToList();

            if (split1 != null && split1.Any())
            {
                foreach (var item in split1)
                {
                    var valore = item.Split(',');

                    string mydata = valore[1];

                    string dataInvertita = mydata.Substring(8, 2) + mydata.Substring(5, 2) + mydata.Substring(0, 4);

                    parametriInput.Add(new miaClass()
                    {
                        Matricola = valore[0],
                        Data = dataInvertita,
                        Cod_eccezione = valore[2]
                    });
                }
            }

            if (parametriInput != null && parametriInput.Any())
            {
                foreach (var x in parametriInput)
                {
                    it.rai.servizi.svildigigappws.WSDigigapp service = new it.rai.servizi.svildigigappws.WSDigigapp();
                    var response = service.getEccezioni(x.Matricola, x.Data, "BU", 75);
                    var eccGapp = response.eccezioni.Where(k => k.cod.Trim() == x.Cod_eccezione.Trim()).FirstOrDefault();

                    lineaReport += x.Matricola + " ; " + x.Data + " ; " + x.Cod_eccezione + " ; " +
                        (eccGapp != null ? "Y" : "N") + " ; " +
                        (eccGapp != null ? "ndoc Gapp:" + eccGapp.ndoc : "") + " ; " +
                        (eccGapp != null ? eccGapp.stato_eccezione : "") + " ; " + "\r\n";

                }
            }
            System.IO.File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\reportEliminatiDaGAPP.txt", lineaReport);
        }

        public static void ControllaRMTR()
        {
            string sediAbilitate = "1AB40,1AB41,1FA00,1FA01,1FA10,1FA20,1FA50,1FA70,1FA80,1FA90,1FAA0,1FAC0,1FAD0,1FAF0,1FAI0,1FAL0,1SA00,1SA01,1SA02,1SA10,1SA20,1SA21,1SA30,1SA40,1SA50,1SA60,1SA61,1SA70,1SA80,1SA90,1SB00,1SB10,2TA00,2TA01,2TA10,2TA30,2TA40,2TA41,2TA50,2TA60,2TA70,2TA80,2TA90,2TA91,2TF00,2TF01,2TF02,2TF10,2TH00,2TN00,2TN40,2TP00,3CM02,3CM09,3CM18,4FR20,5EA00,5EA10,5EB00,5ED00,5FF00,5FF20,5LL00,5LL01,5LL02,5LL03,5LL04,5LL10,5LL20,5PD40,5PD50,5PD60,5PD70,5PD80,5PD90,5PE00,5PE01,5PE10,5PE30,5PE40,5PE60,5PH00,5PR00,5PR10,5PR20,5PR30,5TA00,5TA01,5TA20,5TD00,5TD01,5TD10,5TD20,5TD30,5VH00,5VH02,6CP20,6CP30,8AC00,8AC02,8AC12,8AE00,8AG00,8AG10,8AG20,8AH00,8AK00,8LA00,8LC00,8LC10,8LC12,8LE00,8LG00,8LK00,8LK10,9BC00,9BC02,9BC10,9BC12,9BE00,9BG00,9BG10,9BH00,9BV00,9PC00,9PC02,9PC10,9PC12,9PC32,9PC42,9PD00,9PD02,9PD10,9PE00,9PE20,9PF00,9PF30,9PF60,9PH00,9PH10,9PK00,9PV00,9PV02,DAA00,DAA01,DAC00,DAC01,DCA00,DDC00,DDC10,DDC20,DDCA0,DDD01,DDD02,DDD10,DDD12,DDD20,DDD30,DDD40,DDD50,DDD60,DDD80,DDE10,DDE20,DDE30,DDE40,DDE50,DEA00,DEA01,DEA10,DEC00,DEC10,DFA00,DFB00,QAE00,QAE01,QAE20,QAE30,QAE40,SBA00,SBA01,SBA10,SBA11,SBA20,SBA30,SBA40,SBA50,SBA60,SBA70,SBA80,SBB00,SBB10,SBB20,SBB30,SBB40,SBB50,WBA00,WBA01";

            string lineaReport = "";

            lineaReport += "Sedi abilitate:\r\n";

            lineaReport += sediAbilitate + "\r\n";
            lineaReport += "\r\n";
            lineaReport += "\r\n";



            List<string> sedi = sediAbilitate.Split(',').ToList();

            using (digiGappEntities db = new digiGappEntities())
            {
                var l2d = db.L2D_SEDE_GAPP.Where(c => sedi.Contains(c.cod_sede_gapp) && c.RMTR_intervallo == null).ToList();

                if (l2d != null && l2d.Any())
                {
                    foreach (var item in l2d)
                    {
                        lineaReport += item.cod_sede_gapp.Trim() + "," + item.desc_sede_gapp.Trim() + "," + "\r\n";
                    }
                }

                lineaReport += "\r\n";
                lineaReport += "Numero sedi abilitate: " + sedi.Count() + "\r\n";
                lineaReport += "Numero sedi senza RMTR: " + l2d.Count() + "\r\n";
            }

            System.IO.File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\reportRMTR.txt", lineaReport);
        }

        /// <summary>
        /// Ricerca delle richieste aventi eccezioni con stato diverse dal padre e che non siano storni
        /// Nel caso di richieste in stato 70, controlla sul GAPP lo stato della richiesta e verifica 
        /// l'effettiva cancellazione, altrimenti ripristina lo stato della richiesta allo stato più 
        /// basso tra quelli dell'eccezioni figlie
        /// </summary>
        public static void AllineaRichieste()
        {
            string lineaReport = "";

            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    // recupero tutti i record che sono disallineati. Cioè lo stato del padre
                    // è diverso dallo stato dei figli

                    /*
                    * SELECT e.[id_eccezioni_richieste], e.[id_stato] as statoEccezione, 
                    *					r.id_stato as statoRichiesta, e.id_richiesta, r.*
                    * FROM [digiGapp].[dbo].[MyRai_Richieste] r
                    * join [digiGapp].[dbo].myrai_eccezioni_richieste e
                    * on r.id_richiesta = e.id_richiesta
                    * where r.data_richiesta >= '2018-10-01 00:00:00.000'
                    * and r.data_richiesta <= '2018-10-31 23:59:59.000'
                    * and r.id_stato <> e.id_stato
                    * and e.azione <> 'C'
                    */

                    var records = from richieste in db.MyRai_Richieste
                                  join eccezioni in db.MyRai_Eccezioni_Richieste
                                  on richieste.id_richiesta equals eccezioni.id_richiesta
                                  where richieste.id_stato != eccezioni.id_stato && richieste.id_stato == 70
                                  select new
                                  {
                                      richiesta = richieste
                                  };

                    it.rai.servizi.svildigigappws.WSDigigapp service = new it.rai.servizi.svildigigappws.WSDigigapp();

                    // è stato trovato qualche elemento
                    if (records != null && records.Any())
                    {
                        foreach (var item in records)
                        {
                            AllineaStatoRichiesta(item.richiesta.id_richiesta);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lineaReport += "Si è verificato un errore: " + ex.ToString() + "\r\n";
            }
        }
        public static void AllineaStatoRichiesta(int idRichiesta)
        {
            try
            {
                it.rai.servizi.svildigigappws.WSDigigapp service = new it.rai.servizi.svildigigappws.WSDigigapp();

                using (digiGappEntities db = new digiGappEntities())
                {
                    var richiesta = db.MyRai_Richieste.Where(w => w.id_richiesta.Equals(idRichiesta)).FirstOrDefault();

                    if (richiesta == null)
                    {
                        throw new Exception("");
                    }

                    var eccezioni = db.MyRai_Eccezioni_Richieste.Where(w => w.id_richiesta.Equals(idRichiesta)).ToList();

                    if (eccezioni == null)
                    {
                        throw new Exception("");
                    }

                    bool disallineato = eccezioni.Count(w => w.id_stato != richiesta.id_stato) > 0;

                    if (!disallineato)
                    {
                        throw new Exception("");
                    }

                    int statoMinimo = eccezioni.Min(w => w.id_stato);
                    bool hasChildInGapp = false;
                    int childInGapp = 0;

                    foreach (var e in eccezioni)
                    {
                        // recupero dal GAPP le eccezioni richieste dall'utente nella giornata in esame
                        var response = service.getEccezioni(richiesta.matricola_richiesta, e.data_eccezione.ToString("ddMMyyyy"), "BU", 75);

                        if (!response.esito)
                        {
                            continue;
                        }

                        // cerco nelle eccezioni restituite dal GAPP l'eccezioni correntemente esaminata
                        var eccGapp = response.eccezioni.Where(k => k.cod.Trim() == e.cod_eccezione.Trim()).FirstOrDefault();

                        // se l'eccezione è in stato 10 ma è presente sul GAPP allora deve passare in stato 20
                        // se è stato convalidato
                        if (e.id_stato == 10 && eccGapp != null)
                        {
                            if (eccGapp.stato_eccezione.Trim() == "C")
                            {
                                e.id_stato = 20;
                            }
                        }	// se risulta approvato, ma non è presente sul GAPP
                        else if (e.id_stato == 20 && eccGapp == null)
                        {
                            // va verificato se è stato stornato
                            var storno = eccezioni.Where(w => w.numero_documento_riferimento.Equals(e.numero_documento) && w.azione.Equals("C")).FirstOrDefault();

                            // se c'è uno storno e lo stato è 20 allora è normale che sul GAPP non
                            // sia presente l'eccezione e.
                            // se invece lo stato dello storno è diverso da 20, allora va controllato 
                            // che non ci sia un disallineamento dello storno stesso, 
                            // Ad esempio lo storno è stato approvato ma non è stato aggiornato il record
                            if (storno != null && storno.id_stato != 20)
                            {
                                // presumibilmente lo storno è stato approvato sul gapp
                                if (storno.id_stato == 10)
                                {
                                    storno.id_stato = 20;
                                }
                                else
                                {
                                    // a questo punto l'eccezione è stata cancellata dal GAPP
                                    e.id_stato = 70;
                                }
                            }
                            else if (storno == null)
                            {
                                // se non c'è lo storno allora l'eccezione è da considerarsi come cancellata sul GAPP ?
                                e.id_stato = 70;
                            }
                        }
                        else if (e.id_stato == 20 && eccGapp != null)
                        {
                            // se l'eccezione è nello stato Approvata ed è presente sul GAPP
                            // Ma in stato D, allora lo stato dell'eccezione deve tornare in approvazione
                            if (eccGapp.stato_eccezione.Trim() == "D")
                            {
                                e.id_stato = 10;
                            }
                        }

                        if (eccGapp != null)
                        {
                            hasChildInGapp = true;
                            childInGapp++;
                        }
                    }

                    // al termine del ciclo, dopo aver allineato le eccezioni richieste
                    // bisogna verificare lo stato della richiesta padre
                    if (richiesta.id_stato == 70)
                    {
                        // se lo stato richiesta è 70 allora risulta cancellata
                        // dal GAPP, se c'è almeno un figlio presente sul GAPP allora
                        // la richiesta non può avere lo stato 70
                        if (hasChildInGapp)
                        {
                            richiesta.id_stato = statoMinimo;
                        }
                        else
                        {
                            foreach (var e in eccezioni)
                            {
                                e.id_stato = 70;
                            }
                        }
                    }
                    else
                    {
                        // se lo stato è diverso da 70, allora
                        // la richiesta deve prendere lo stato più basso 
                        // tra le varie eccezioni figlie
                        richiesta.id_stato = statoMinimo;
                    }
                }
            }
            catch (Exception ex)
    {
    }
}

        public static void ModificaStatoRichiesta(int idRichiesta, int idStato)
        {
            try
            {
                it.rai.servizi.svildigigappws.WSDigigapp service = new it.rai.servizi.svildigigappws.WSDigigapp();

                using (digiGappEntities db = new digiGappEntities())
                {
                    var richiesta = db.MyRai_Richieste.Where(w => w.id_richiesta.Equals(idRichiesta)).FirstOrDefault();

                    if (richiesta == null)
                    {
                        throw new Exception("");
                    }

                    richiesta.id_stato = idStato;

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void CercaStorniUMHAutoApprovati()
        {
            string lineaReport = "Ricerca Storni UMH auto approvati e richiesti tra il 16/11/2018 ed il 23/11/2018 \r\n";

            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    DateTime start = new DateTime(2018, 11, 15, 0, 0, 0);
                    DateTime end = new DateTime(2018, 11, 16, 23, 59, 59);
                    var listaUMH = (from ecc in db.MyRai_Eccezioni_Richieste
                                    join ric in db.MyRai_Richieste
                                    on ecc.id_richiesta equals ric.id_richiesta
                                    where ecc.cod_eccezione == "UMH" &&
                                         ecc.id_stato == 20 &&
                                         ecc.azione == "C" &&
                                         ecc.matricola_primo_livello == ric.matricola_richiesta &&
                                         ric.data_richiesta >= start && ric.data_richiesta <= end
                                    select ecc).ToList();


                    if (listaUMH != null && listaUMH.Any())
                    {
                        listaUMH.ForEach(u =>
                        {
                            lineaReport += String.Format("Eccezione id: {0} richiesta id: {1} per il {2} richiesta ed approvata da {3} stato stornata \r\n", u.id_eccezioni_richieste, u.id_richiesta, u.data_eccezione.ToString("dd/MM/yyyy"), u.matricola_primo_livello);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                lineaReport += "Si è verificato il seguente errore:" + ex.Message + "\r\n";
            }

            System.IO.File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\reportCercaStorniUMHAutoApprovati.txt", lineaReport);
        }

        public class StatoEccezioniGiornateResponse
        {
            /// <summary>
            /// Esito della richiesta
            /// </summary>
            public bool Esito { get; set; }

            /// <summary>
            /// Eventuale errore
            /// </summary>
            public string Errore { get; set; }

            /// <summary>
            /// Dati aggiornati
            /// </summary>
            public List<MyRai_StatoEccezioniGiornate_Custom> StatoEccezioniGiornate { get; set; }
        }

        public partial class MyRai_StatoEccezioniGiornate_Custom
        {
            public System.DateTime DataEccezione { get; set; }
            public bool InApprovazione { get; set; }
            public bool Approvate { get; set; }
            public bool Visualizzato { get; set; }
            /// <summary>
            /// Se true per quella data non ci sono eccezioni
            /// </summary>
            public bool Empty { get; set; }
            public string Matricola { get; set; }
            public MyRai_Visualizzazione_Giornate_Da_Segreteria DettaglioVisualizzazione { get; set; }
        }

        /// <summary>
        /// il Range è 1 mese es: 1/11 a 30/11
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="dataDa"></param>
        /// <param name="dataA"></param>
        /// <returns></returns>
        public static StatoEccezioniGiornateResponse GetStatoEccezioniGiornate(string matricola, DateTime dataDa, DateTime dataA)
        {
            StatoEccezioniGiornateResponse response = new StatoEccezioniGiornateResponse();

            response.Errore = String.Empty;
            response.Esito = true;
            response.StatoEccezioniGiornate = new List<MyRai_StatoEccezioniGiornate_Custom>();
            List<MyRai_StatoEccezioniGiornate_Custom> temp = new List<MyRai_StatoEccezioniGiornate_Custom>();

            try
            {
                if (String.IsNullOrEmpty(matricola))
                {
                    throw new Exception("La matricola è un dato obbligatorio.");
                }

                if (matricola.Length < 6)
                {
                    matricola = matricola.PadLeft(6, '0');
                }
                else if (matricola.Length > 6)
                {
                    throw new Exception("Matricola non valida. La lunghezza prevista è 6.");
                }

                if (dataDa.Date > dataA.Date)
                {
                    throw new Exception("Data inizio range non può essere superiore alla data di fine range.");
                }

                DateTime start = new DateTime(dataDa.Year, dataDa.Month, dataDa.Day, 0, 0, 0);
                DateTime stop = new DateTime(dataA.Year, dataA.Month, dataA.Day, 23, 59, 59);

                using (digiGappEntities db = new digiGappEntities())
                {
                    // ciclo i giorni del mese
                    // per ogni giorno verifico se c'è il visualizzato
                    // e se ci sono giorni in approvazione e/o approvati
                    for (int giorno = 1; giorno <= stop.Day; giorno++)
                    {
                        DateTime _dt = new DateTime(dataDa.Year, dataDa.Month, giorno, 0, 0, 0);

                        // verifica se ci sono visualizzati
                        var visualizzato = db.MyRai_Visualizzazione_Giornate_Da_Segreteria
                                            .Where(w => EntityFunctions.TruncateTime(w.DataRichiesta) ==
                                                        EntityFunctions.TruncateTime(_dt) &&
                                                    w.Matricola == matricola).FirstOrDefault();

                        //verifica se ci sono eccezioni in approvazione per quella giornata
                        bool inApprovazione = ((from ecc in db.MyRai_Eccezioni_Richieste
                                                join r in db.MyRai_Richieste
                                                on ecc.id_richiesta equals r.id_richiesta
                                                where r.matricola_richiesta == matricola &&
                                                    ecc.id_stato == 10 &&
                                                    EntityFunctions.TruncateTime(ecc.data_eccezione) ==
                                                    EntityFunctions.TruncateTime(_dt)
                                                select ecc).Count() > 0);

                        // verifica se ci sono eccezioni approvate per la giornata esaminata
                        bool approvati = ((from ecc in db.MyRai_Eccezioni_Richieste
                                           join r in db.MyRai_Richieste
                                           on ecc.id_richiesta equals r.id_richiesta
                                           where r.matricola_richiesta == matricola &&
                                               ecc.id_stato == 20 &&
                                               EntityFunctions.TruncateTime(ecc.data_eccezione) ==
                                               EntityFunctions.TruncateTime(_dt)
                                           select ecc).Count() > 0);

                        bool empty = ((from ecc in db.MyRai_Eccezioni_Richieste
                                       join r in db.MyRai_Richieste
                                       on ecc.id_richiesta equals r.id_richiesta
                                       where r.matricola_richiesta == matricola &&
                                           EntityFunctions.TruncateTime(ecc.data_eccezione) ==
                                           EntityFunctions.TruncateTime(_dt)
                                       select ecc).Count() == 0);

                        response.StatoEccezioniGiornate.Add(new MyRai_StatoEccezioniGiornate_Custom
                        {
                            Visualizzato = visualizzato != null ? visualizzato.Visualizzato : false,
                            DataEccezione = _dt,
                            DettaglioVisualizzazione = visualizzato != null ? visualizzato : null,
                            Matricola = matricola,
                            InApprovazione = inApprovazione,
                            Approvate = inApprovazione ? false : approvati,
                            Empty = empty
                        });

                    }
                }
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.Errore = ex.Message;
                response.StatoEccezioniGiornate = null;
            }

            return response;
        }

        public static void InserisciPatentiGAVU()
        {
            string patenteB = "Patente B";
            string lineaReport = "";
            int idPatenteB = -1;

            //// per sviluppo
            //patenteB = "Patente C";

            using (var db = new digiGappEntities())
            {
                var b = db.MyRai_InfoDipendente_Tipologia.Where(w => w.info.Equals(patenteB)).FirstOrDefault();
                if (b != null)
                {
                    idPatenteB = b.id;
                }
            }

            string strStart = "P003267," +
                            "P006842," +
                            "P062499," +
                            "P162102," +
                            "P176680," +
                            "P192415," +
                            "P192597," +
                            "P202879," +
                            "P267420," +
                            "P299630," +
                            "P299960," +
                            "P311837," +
                            "P329136," +
                            "P329415," +
                            "P486250," +
                            "P512185," +
                            "P527769," +
                            "P593912," +
                            "P674060," +
                            "P685185," +
                            "P754634," +
                            "P759970," +
                            "P775137," +
                            "P862629," +
                            "P921167," +
                            "P940570";

            string matricolAConScadenza = "P202879," +
                                        "P267420," +
                                        "P311837," +
                                        "P329136," +
                                        "P329415," +
                                        "P593912," +
                                        "P674060," +
                                        "P754634," +
                                        "P759970," +
                                        "P921167";

            List<DateTime> scadenze = new List<DateTime>();
            scadenze.Add(new DateTime(2026, 08, 01, 23, 59, 59));
            scadenze.Add(new DateTime(2022, 11, 19, 23, 59, 59));
            scadenze.Add(new DateTime(2024, 07, 10, 23, 59, 59));
            scadenze.Add(new DateTime(2028, 03, 28, 23, 59, 59));
            scadenze.Add(new DateTime(2023, 09, 26, 23, 59, 59));
            scadenze.Add(new DateTime(2022, 02, 16, 23, 59, 59));
            scadenze.Add(new DateTime(2020, 12, 09, 23, 59, 59));
            scadenze.Add(new DateTime(2023, 08, 06, 23, 59, 59));
            scadenze.Add(new DateTime(2021, 09, 13, 23, 59, 59));
            scadenze.Add(new DateTime(2027, 02, 02, 23, 59, 59));

            List<string> matricole = strStart.Split(',').ToList();
            List<string> matricoleConScadenza = matricolAConScadenza.Split(',').ToList();

            if (matricole != null && matricole.Any())
            {
                DateTime inizio = new DateTime(2018, 12, 1);
                DateTime fine = DateTime.MaxValue;

                foreach (var matricola in matricole)
                {
                    if (matricola.Trim().Length == 0)
                    {
                        continue;
                    }

                    try
                    {
                        string dipendente = matricola.Trim();

                        if (matricoleConScadenza.Contains(dipendente))
                        {
                            fine = scadenze.First();
                            scadenze.RemoveAt(0);
                        }
                        else
                        {
                            fine = DateTime.MaxValue;
                        }

                        dipendente = dipendente.Replace("P", "");

                        using (var db = new digiGappEntities())
                        {
                            db.MyRai_InfoDipendente.Add(new MyRai_InfoDipendente
                            {
                                matricola = dipendente,
                                id_infodipendente_tipologia = idPatenteB,
                                valore = "True",
                                data_inizio = inizio,
                                data_fine = fine,
                                note = null
                            });

                            db.SaveChanges();

                            lineaReport += "Inserita GAVU matricola: " + matricola +
                                        "\r\n";
                        }
                    }
                    catch (Exception ex)
                    {
                        lineaReport += "Si è verificato un errore con la matricola: " + matricola + ex.Message +
                                        "\r\n";
                    }
                }
            }
            System.IO.File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\reportInserimentoPatenti.txt", lineaReport);
        }

        public static GetTimbratureOspitiResponse GetTimbratureOspiti(string matricolaCaller, string matricolaRichiesta, string dataRicerca = "00000000")
        {
            GetTimbratureOspitiResponse response = new GetTimbratureOspitiResponse();

            response.Request = new GetTimbratureOspitiRequest()
            {
                Matricola = matricolaRichiesta,
                DataRicerca = dataRicerca
            };

            response.Response = new List<GetTimbratureOspitiElement>();
            response.Esito = true;
            response.RispostaCics = new List<string>();

            // se la matricola inizia per P, rimuove la P
            matricolaCaller = matricolaCaller.ToUpper().Replace("P", "");

            // se la matricola inizia per RAI\, viene rimosso e rimuove la P
            if (matricolaCaller.StartsWith("RAI\\", StringComparison.InvariantCultureIgnoreCase))
            {
                matricolaCaller = matricolaCaller.Replace("RAI\\", "");
                matricolaCaller = matricolaCaller.ToUpper().Replace("P", "");
            }

            List<GetTimbratureOspitiElement> listaTimbrature = new List<GetTimbratureOspitiElement>();
            try
            {
                matricolaCaller = "103650";
                matricolaRichiesta = "16969801";

                int livelloUtente = 75;
                bool richiama = false;
                bool esitoCics = false;
                int records = 0;
                string numeroRecord = "0000";

                string spaces = new String(' ', 30);

                ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics();
                string S = areaSistema2("3VI", matricolaCaller, livelloUtente, false);
                S += matricolaRichiesta + dataRicerca + numeroRecord;
                S += spaces;
                string risposta = "";
                int iterazioni = -1;

                do
                {
                    iterazioni++;
                    if (richiama)
                    {
                        S = areaSistema2("3VI", matricolaCaller, livelloUtente, true);
                        numeroRecord = records.ToString().PadLeft(4, '0');

                        int numeroR = int.Parse(numeroRecord);
                        numeroR *= iterazioni;
                        numeroRecord = numeroR.ToString().PadLeft(4, '0');

                        S += matricolaRichiesta + dataRicerca + numeroRecord;
                        S += spaces;
                        risposta = "";
                    }

                    try
                    {
                        risposta = (string)c.ComunicaVersoCics(S.PadRight(8000));
                        risposta = risposta.Replace(Convert.ToChar(0x0).ToString(), " ");
                        response.RispostaCics.Add(risposta);

                        // recupero esito elaborazione
                        esitoCics = (risposta.Substring(45, 2) == "OK");

                        string codiceErrore = risposta.Substring(47, 4);
                        string descrizioneErrore = risposta.Substring(51, 25);

                        if (esitoCics)
                        {
                            records = int.Parse(risposta.Substring(108, 4));

                            int MAX = (records < 100 ? records : 100);

                            // verifica se va richiamato il metodo
                            richiama = (risposta.Substring(44, 1) == "S");

                            // rimuovo la prima parte della risposta
                            // che contiene la parte relativa al nome funzione
                            // Area sistema e chiave di ricerca
                            string tabella = risposta.Substring(125);

                            int baseIndex = 17;

                            for (int idx = 1; idx <= MAX; idx++)
                            {
                                GetTimbratureOspitiElement e = new GetTimbratureOspitiElement();

                                e.DataTimbratura = tabella.Substring(0 + baseIndex, 8);
                                e.OraTimbratura = int.Parse(tabella.Substring(8 + baseIndex, 2));
                                e.MinutoTimbratura = int.Parse(tabella.Substring(10 + baseIndex, 2));
                                e.ProgressivoTimbratura = int.Parse(tabella.Substring(12 + baseIndex, 1));

                                string entrata = tabella.Substring(13 + baseIndex, 1);

                                if (entrata.Length > 0)
                                {
                                    e.EntrataUscita = (entrata == "0" ? false : true);
                                }
                                else
                                {
                                    throw new Exception("Errore nella conversione della timbratura di entrata o uscita");
                                }

                                e.Rete = tabella.Substring(14 + baseIndex, 1);
                                e.CodiceTerminale = tabella.Substring(15 + baseIndex, 4);
                                e.Linea = tabella.Substring(19 + baseIndex, 1);
                                e.In_Di_Ri = tabella.Substring(20 + baseIndex, 1);
                                e.Cod_1 = tabella.Substring(22 + baseIndex, 1);
                                e.CodiceGapp = tabella.Substring(23 + baseIndex, 3);
                                e.TipoTerminale = tabella.Substring(25 + baseIndex, 1);
                                e.DescrizioneTerminale = tabella.Substring(26 + baseIndex, 15);
                                e.TipoBadge = tabella.Substring(41 + baseIndex, 2);
                                e.ProgressivoBadge = tabella.Substring(43 + baseIndex, 2);

                                try
                                {
                                    string itm = tabella.Substring(45 + baseIndex, 8);

                                    itm = itm.Replace(" ", "");
                                    itm = itm.Replace("0", "");

                                    if (!String.IsNullOrEmpty(itm))
                                    {
                                        string _gg = itm.Substring(0, 2);
                                        string _mm = itm.Substring(2, 2);
                                        string _aa = itm.Substring(4, 4);

                                        int gg;
                                        int mm;
                                        int aa;

                                        gg = int.Parse(_gg);
                                        mm = int.Parse(_mm);
                                        aa = int.Parse(_aa);

                                        e.DataRilascio = new DateTime(aa, mm, gg);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception("Data di rilascio badge non valida");
                                }

                                listaTimbrature.Add(e);

                                if (tabella.Length > 70)
                                {
                                    // rimuove la riga appena analizzata
                                    tabella = tabella.Substring(70);
                                }
                                else
                                {
                                    throw new Exception("Errore nella dimensione della risposta");
                                }
                            }

                            // se ci sono ulteriori records da esaminare allora
                            // dovrà rifare la chiamata a cics per reperire gli 
                            // ulteriori dati
                            if (records > MAX)
                            {
                                richiama = true;
                            }
                        }
                        else
                        {
                            string error = String.Format("{0} - {1}", codiceErrore, descrizioneErrore);
                            throw new Exception(error);
                        }
                    }
                    catch (Exception ex)
                    {
                        response.Esito = false;
                        response.Errore = ex.Message;
                        response.Response = null;
                        break;
                    }
                    response.Response.AddRange(listaTimbrature);
                } while (richiama);
            }
            catch (Exception ex)
            {
                response.Esito = false;
                response.Errore = ex.Message;
                response.Response = null;
            }

            return response;
        }

        public class GetTimbratureOspitiRequest
        {
            public string Matricola { get; set; }
            public string DataRicerca { get; set; }
        }

        public class GetTimbratureOspitiElement
        {
            public string DataTimbratura { get; set; }
            public int OraTimbratura { get; set; }
            public int MinutoTimbratura { get; set; }
            public int ProgressivoTimbratura { get; set; }
            public bool EntrataUscita { get; set; }
            public string Rete { get; set; }
            public string CodiceTerminale { get; set; }
            public string Linea { get; set; }
            public string In_Di_Ri { get; set; }
            public string Cod_1 { get; set; }
            public string CodiceGapp { get; set; }
            public string TipoTerminale { get; set; }
            public string DescrizioneTerminale { get; set; }
            public string TipoBadge { get; set; }
            public string ProgressivoBadge { get; set; }
            public DateTime? DataRilascio { get; set; }
            public DateTime? DataTimbraturaFull
            {
                get
                {
                    if (!String.IsNullOrEmpty(this.DataTimbratura))
                    {
                        try
                        {
                            string _gg = this.DataTimbratura.Substring(0, 2);
                            string _mm = this.DataTimbratura.Substring(2, 2);
                            string _aa = this.DataTimbratura.Substring(4, 4);

                            int gg;
                            int mm;
                            int aa;

                            gg = int.Parse(_gg);
                            mm = int.Parse(_mm);
                            aa = int.Parse(_aa);

                            return new DateTime(aa, mm, gg, this.OraTimbratura, this.MinutoTimbratura, 0);
                        }
                        catch (Exception ex)
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            public string Insediamento
            {
                get
                {
                    if (!string.IsNullOrEmpty(this.Cod_1) &&
                        !string.IsNullOrEmpty(this.CodiceTerminale))
                    {
                        return String.Format("{0}{1}", this.Cod_1, this.CodiceTerminale);
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            public string Terminale
            {
                get
                {
                    if (!string.IsNullOrEmpty(this.Rete) &&
                        !string.IsNullOrEmpty(this.CodiceTerminale) &&
                        !string.IsNullOrEmpty(this.Linea) &&
                        !string.IsNullOrEmpty(this.In_Di_Ri))
                    {
                        return String.Format("{0}{1}{2}{3}", this.Cod_1, this.CodiceTerminale, this.Linea, this.In_Di_Ri);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        public class GetTimbratureOspitiResponse
        {
            public bool Esito { get; set; }
            public string Errore { get; set; }
            public GetTimbratureOspitiRequest Request { get; set; }
            public List<GetTimbratureOspitiElement> Response { get; set; }
            public List<string> RispostaCics { get; set; }
        }

        public static string areaSistema2(string funzione, string matricola, int livelloUtente, bool richiama = false)
        {
            string flg_chiamata = richiama ? "S" : "N";

            return "P956,3XX," + funzione + "GAPWEB" + matricola.PadLeft(10, '0') + DateTime.Now.ToString("ddMMyyyy") + "".PadRight(5, '0') + flg_chiamata + "".PadRight(6, ' ') + "".PadRight(25, 'a') + livelloUtente.ToString() + DateTime.Now.ToString("HH:mm:ss").PadLeft(14, ' ') + ",";
        }

        public static string areaSistema(string funzione, string matricola, int livelloUtente, bool richiama = false)
        {
            string flg_chiamata = richiama ? "S" : " ";

            return "P956,3XX," + funzione + "GAPWEB" + matricola.PadLeft(10, '0') + DateTime.Now.ToString("ddMMyyyy") + "".PadRight(5, '0') + flg_chiamata + "".PadRight(6, ' ') + "".PadRight(25, 'a') + livelloUtente.ToString() + DateTime.Now.ToString("HH:mm:ss").PadLeft(14, ' ') + ",";
        }

        public static object populateObject(object o)
        {
            Random r = new Random();
            PropertyInfo[] propertyInfo = o.GetType().GetProperties();
            for (int i = 0; i < propertyInfo.Length; i++)
            {
                PropertyInfo info = propertyInfo[i];

                string strt = info.PropertyType.Name;
                Type t = info.PropertyType;
                try
                {
                    dynamic value = null;

                    if (t == typeof(string) || t == typeof(String))
                    {
                        value = "asdf";
                        info.SetValue(o, value, null);
                    }
                    else if (t == typeof(Int16) || t == typeof(Int32) || t == typeof(Int64))
                    {
                        value = (Int16)r.Next(999);

                        info.SetValue(o, value, null);
                    }
                    else if (t == typeof(Int16?))
                    {
                        Int16? v = (Int16)r.Next(999);
                        info.SetValue(o, v, null);
                    }
                    else if (t == typeof(Int32?))
                    {
                        Int32? v = (Int32)r.Next(999);
                        info.SetValue(o, v, null);
                    }
                    else if (t == typeof(Int64?))
                    {
                        Int64? v = (Int64)r.Next(999);
                        info.SetValue(o, v, null);
                    }
                    else if (t == typeof(DateTime) || t == typeof(DateTime?))
                    {
                        value = DateTime.Now;
                        info.SetValue(o, value, null);
                    }
                    else if (t == typeof(double) || t == typeof(float) || t == typeof(Double))
                    {
                        value = 17.2;
                        info.SetValue(o, value, null);
                    }
                    else if (t == typeof(char) || t == typeof(Char))
                    {
                        value = 'a';
                        info.SetValue(o, value, null);
                    }
                    else
                    {
                        //throw new NotImplementedException ("Tipo não implementado :" + t.ToString () );
                        //object temp = info.GetValue(o);
                        //if (temp == null)
                        //{
                        //    temp = Activator.CreateInstance(t);
                        //    info.SetValue(o, temp, null);
                        //}
                        //populateObject(temp);
                    }

                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            return o;
        }

        public static Boolean StatoD(MyRai_Eccezioni_Richieste ec, int co)
        {
            it.rai.servizi.svildigigappws.WSDigigapp service = new it.rai.servizi.svildigigappws.WSDigigapp()
            {
                Credentials = new System.Net.NetworkCredential("srvruofpo", "zaq22?mk")
            };
            dayResponse response = service.getEccezioni(ec.MyRai_Richieste.matricola_richiesta,
                       ec.data_eccezione.ToString("ddMMyyyy"), "BU", 75);

            String r = co + ". Matr:" + ec.MyRai_Richieste.matricola_richiesta + " Cod:" + ec.cod_eccezione + " data val.:" + ((DateTime)ec.data_validazione_primo_livello).ToString("ddMMyyyy HHmm") + " data eccezione:" + ec.data_eccezione.ToString("ddMMyyyy") + " id ecc:" + ec.id_eccezioni_richieste + " - ndoc:" + ec.numero_documento + " Stato:";

            var ecc = response.eccezioni.Where(x => ec.numero_documento == Convert.ToInt32(x.ndoc)).FirstOrDefault();

            if (ecc == null) r += " NON TROVATA";
            else r += ecc.stato_eccezione;

            Log(r);

            return (ecc != null && ecc.stato_eccezione == "D");
        }

        public static string ErroreInValidationResponse(validationResponse resp)
        {
            if (resp.esito == false) return resp.errore;
            string errore = null;
            foreach (var e in resp.eccezioni)
            {
                if (e.errore == null || e.errore.codice == null || e.errore.descrizione == null) continue;
                if (e.errore.codice != "0000" && e.errore.codice != "A100") errore += "Ecc " + e.ndoc + ":" + e.errore.descrizione.Trim() + "\r\n";
            }
            return errore;
        }

        public static void LogEliminate(string msg, string filename)
        {
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var directory = System.IO.Path.GetDirectoryName(path);
            var logfile = System.IO.Path.Combine(directory, "log", filename);
            System.IO.File.AppendAllText(logfile, msg + "\r\n");
        }

        public static void Log(string msg)
        {
            Console.WriteLine(msg);


            System.IO.File.AppendAllText("D:\\My Documents\\eric722\\report.txt", msg + "\r\n");
        }

        public static int calcolaMinuti(DateTime D)
        {
            int minuti = (D.Hour * 60) + D.Minute;
            return minuti;
        }

        public static int calcolaMinuti(string orarioHHMM)
        {
            int minuti = 0;
            string[] array = new string[2];

            if (orarioHHMM.IndexOf(':') > 0)
                array = orarioHHMM.Split(':');
            else
            {
                array[0] = orarioHHMM.Substring(0, 2);
                array[1] = orarioHHMM.Substring(2, 2);
            }

            minuti = Int32.Parse(array[0]) * 60 + Int32.Parse(array[1]);

            return minuti;
        }

        public static string CalcolaQuantitaOreMinuti(string dallemin, string allemin)
        {
            int dalle = Convert.ToInt32(dallemin);
            int alle = Convert.ToInt32(allemin);
            int diff = alle - dalle;
            int h = (int)diff / 60;
            int min = diff - (h * 60);
            return h.ToString().PadLeft(2, '0') + ":" + min.ToString().PadLeft(2, '0');
        }

        public static Boolean IsEccezioneAQuarti(string codice)
        {
            var db = new digiGappEntities();
            var ecc = db.L2D_ECCEZIONE.Where(x => x.cod_eccezione.Trim().ToUpper() == codice.Trim().ToUpper()).FirstOrDefault();
            return (ecc != null && ecc.flag_macroassen == "Q");
        }

        //public static object GetPropValue(object src, string propName)
        //{
        //    return src.GetType().GetProperty(propName).GetValue(src, null);
        //}
        //private static void someMethod(string myTypeString, List<string> typeList)
        //{
        //    foreach (var type in typeList.Where(x => (DateTime)GetPropValue(x, myTypeString) > DateTime.Now))
        //    {
        //        //do my loop
        //    }
        //}

        private static string areaSistema3 ( string chiamante, string matricolaTarget)
        {
            //return "P956,DST,P684930   DET,676140";
            return "P956,DST," + chiamante + "   DET," + matricolaTarget;
        }

        /// <summary>
        /// Cancella il modulo memorizzato su CICS
        /// </summary>
        /// <param name="chiamante"></param>
        /// <param name="matricolaTarget"></param>
        /// <returns></returns>
        private static string areaSistema4 ( string chiamante , string matricolaTarget )
        {
            //return "P956,DST,P684930   DET,676140";
            return "P956,DST," + chiamante + "   CAN," + matricolaTarget;
        }


        private static void VerificaGiornateUtentiSirio()
        {
            try
            {
                MyRaiService1Client service = new MyRaiService1Client( );

                DateTime dtStart = new DateTime( 2019 , 8 , 1 );
                DateTime dtEnd = new DateTime( 2019 , 8 , 31 );

                try
                {
                    var resp = service.presenzeGiornaliere( "103650" , "9FGA1" , dtStart.ToString( "ddMMyyyy" ) );

                    if ( resp.esito == false && !String.IsNullOrEmpty( resp.errore ) )
                    {
                        throw new Exception( "Esito false" );

                    }

                    if ( resp.dati == null || resp.dati.Count( ) == 0 )
                    {
                        throw new Exception( "Non ci sono dati disponibili" );
                    }

                    foreach ( var item in resp.dati )
                    {
                        var presenzeMese = service.GetSchedaPresenzeMese( ( item.matricola.Length > 6 ) ? item.matricola.Substring( 1 ) : item.matricola , dtStart , dtEnd );

                        if ( presenzeMese.esito )
                        {
                            if ( presenzeMese.Giorni != null &&
                                presenzeMese.Giorni.Any( ) )
                            {
                                foreach ( var giorno in presenzeMese.Giorni )
                                {
                                    DateTime data = giorno.data;
                                    
                                }
                            }
                        }



                    }
                }
                catch ( Exception ex )
                {
                }
            }
            catch(Exception ex)
            {

            }
        }

        static void Main2(string[] args)
        {
            //MyRaiService1Client cl = new MyRaiService1Client( );
            //var res = cl.GetSchedaPresenzeMese( "114763" , new DateTime( 2019 , 08 , 01 ) , new DateTime( 2019 , 12 , 31 ) );

            //DateTime dt123 = new DateTime( 2019 , 07 , 14 );
            //var tt = cl.recuperaUtente( "0930509" , dt123.ToString( "ddMMyyyy" ) );

            //var tt = cl.recuperaUtente( "0930509" , DateTime.Now.ToString( "ddMMyyyy" ) );

            //var miaprova = cl.GetLista_Eccezioni_Giornalisti( "103650" , "DDE20" , 8 , 2019 );

            //var risposta = cl.GetModuloDetassazione( "P103650" , "103650" );

            //ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics( );
            //string S = areaSistema4( "P684930" , "684930" );
            //string s2;
            //s2 = ( string ) c.ComunicaVersoCics( S );
            //s2 = s2.Replace( Convert.ToChar( 0x0 ).ToString( ) , " " );

            //return;

            //MyRaiService1Client cl = new MyRaiService1Client( );

            //var perLeo = cl.SetSceltaDetassazione( "P103650" , "103650" , DateTime.Now , "1C" , 0 );

            //var risposta = cl.GetModuloDetassazione( "P103650" , "103650" );
            //return;

            //DateTime data = DateTime.Now;

            //string versoCics = "P956,DST," + "P103650" + "   UPD," + "103650" + data.ToString( "yyyyMMdd" ) + "0C0N                           11  ";

            //ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics();
            //string s2;
            //s2 = ( string )c.ComunicaVersoCics( versoCics );
            //s2 = s2.Replace( Convert.ToChar( 0x0 ).ToString(), " " );

            //string risultato1 = s2.Substring( 74 );

            //return;

            //MyRaiService1Client clienttest = new MyRaiService1Client( );

            //var risposta = clienttest.GetPianoFerieAnno( "3MB00" , 2019 , false );
            ////var risposta2 = clienttest.recuperaUtente( "0910751" , "01072019" );

            //WSDigigapp service2 = new WSDigigapp();
            //service2.Credentials = new System.Net.NetworkCredential("srvruofpo", "zaq22?mk");

            //var risultato2 = service2.getPianoFerieSedeGapp( "103650" , "01012019" , null , null, "3MB00" , 75 );

            //ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics( );
            //string S = areaSistema3( "P103650" , "103650" );
            //string s2;
            //s2 = ( string ) c.ComunicaVersoCics( S );
            //s2 = s2.Replace( Convert.ToChar( 0x0 ).ToString( ) , " " );

            //string risultato1 = s2.Substring( 74 , 2 );
            //string risultato2 = s2.Substring( 76 , 2 );

            //return;
            //List<Report_Item> rList = new List<Report_Item>( );
            //DateTime dt = new DateTime( 2019 , 7 , 1 );

            //try
            //{
            //    var resp = clienttest.presenzeGiornaliere( "103650" , "DDE30" , dt.ToString( "ddMMyyyy" ) );

            //    if ( resp.esito == false && !String.IsNullOrEmpty( resp.errore ) )
            //    {
            //        return;
            //    }

            //    if ( resp.dati == null || resp.dati.Count( ) == 0 )
            //    {
            //        return;
            //    }

            //    foreach ( var item in resp.dati )
            //    {
            //        DateTime dtEnd = dt.AddMonths( 1 ).AddDays( -1 );

            //        var presenzeMese = clienttest.GetSchedaPresenzeMese( ( item.matricola.Length > 6 ) ? item.matricola.Substring( 1 ) : item.matricola , dt , dtEnd );

            //        List<Report_Lista_Item> listPOH = new List<Report_Lista_Item>( );
            //        List<Report_Lista_Item> listROH = new List<Report_Lista_Item>( );
            //        List<Report_Lista_Item> listCAR = new List<Report_Lista_Item>( );

            //        if ( presenzeMese.esito )
            //        {
            //            if ( presenzeMese.Giorni != null &&
            //                presenzeMese.Giorni.Any( ) )
            //            {
            //                foreach ( var giorno in presenzeMese.Giorni )
            //                {
            //                    if ( giorno.MicroAssenze != null && giorno.MicroAssenze.Any( ) )
            //                    {
            //                        foreach ( var ma in giorno.MicroAssenze )
            //                        {
            //                            if ( !String.IsNullOrEmpty( ma.nome ) && ma.nome.ToUpper( ).Contains( "POH" ) )
            //                            {
            //                                string[] _temp = ma.quantita.Split( '.' );

            //                                string ore = _temp[0];
            //                                string min = _temp[1];

            //                                int hh = int.Parse( ore );
            //                                int minuti = int.Parse( min );

            //                                if ( hh > 1 )
            //                                {
            //                                    // converti in minuti
            //                                    int tempM = ( hh * 60 );
            //                                    // somma i minuti
            //                                    minuti += tempM;
            //                                    // sottrai alle ore i minuti appena convertiti
            //                                    hh -= ( tempM / 60 );
            //                                }

            //                                if ( hh == 1 )
            //                                {
            //                                    // converti in minuti
            //                                    int tempM = 60;
            //                                    // somma i minuti
            //                                    minuti += tempM;
            //                                    // sottrai alle ore i minuti appena convertiti
            //                                    hh = 0;
            //                                }

            //                                listPOH.Add( new Report_Lista_Item( )
            //                                {
            //                                    Giorno = giorno.data ,
            //                                    Minuti = minuti
            //                                } );
            //                            }

            //                            if ( !String.IsNullOrEmpty( ma.nome ) && ma.nome.ToUpper( ).Contains( "ROH" ) )
            //                            {
            //                                string[] _temp = ma.quantita.Split( '.' );

            //                                string ore = _temp[0];
            //                                string min = _temp[1];

            //                                int hh = int.Parse( ore );
            //                                int minuti = int.Parse( min );

            //                                if ( hh > 1 )
            //                                {
            //                                    // converti in minuti
            //                                    int tempM = ( hh * 60 );
            //                                    // somma i minuti
            //                                    minuti += tempM;
            //                                    // sottrai alle ore i minuti appena convertiti
            //                                    hh -= ( tempM / 60 );
            //                                }

            //                                if ( hh == 1 )
            //                                {
            //                                    // converti in minuti
            //                                    int tempM = 60;
            //                                    // somma i minuti
            //                                    minuti += tempM;
            //                                    // sottrai alle ore i minuti appena convertiti
            //                                    hh = 0;
            //                                }

            //                                listROH.Add( new Report_Lista_Item( )
            //                                {
            //                                    Giorno = giorno.data ,
            //                                    Minuti = minuti
            //                                } );
            //                            }

            //                            if ( !String.IsNullOrEmpty( ma.nome ) && ma.nome.ToUpper( ).Contains( "CAR" ) )
            //                            {
            //                                string[] _temp = ma.quantita.Split( '.' );

            //                                string ore = _temp[0];
            //                                string min = _temp[1];

            //                                int hh = int.Parse( ore );
            //                                int minuti = int.Parse( min );

            //                                if ( hh > 1 )
            //                                {
            //                                    // converti in minuti
            //                                    int tempM = ( hh * 60 );
            //                                    // somma i minuti
            //                                    minuti += tempM;
            //                                    // sottrai alle ore i minuti appena convertiti
            //                                    hh -= ( tempM / 60 );
            //                                }

            //                                if ( hh == 1 )
            //                                {
            //                                    // converti in minuti
            //                                    int tempM = 60;
            //                                    // somma i minuti
            //                                    minuti += tempM;
            //                                    // sottrai alle ore i minuti appena convertiti
            //                                    hh = 0;
            //                                }

            //                                listCAR.Add( new Report_Lista_Item( )
            //                                {
            //                                    Giorno = giorno.data ,
            //                                    Minuti = minuti
            //                                } );
            //                            }
            //                        }

            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            //catch ( Exception ex )
            //{
            //    return;
            //}

            //return;


            //var o = clienttest.GetReport_POH_ROH( "103650" , "DDE30" , 7 , 2019 );

            //var t = clienttest.GetSchedaPresenzeMese( "676140" , new DateTime( 2019 , 7 , 1 ) , new DateTime( 2019 , 7 , 31 ) );

            //return;

            //List<mensa> lista = new List<mensa>();

            //using (var dbb = new digiGappEntities())
            //{
            //     lista = dbb.Database.SqlQuery<mensa>("SELECT locationid,transactionid,Badge, CONVERT(VARCHAR(10), transactiondatetime, 103) as data, count(*) as scontrini FROM[digiGapp].[dbo].[MyRai_MensaXML]" +
            //    " where badge <> '00000000' group by locationid, transactionid, Badge, CONVERT(VARCHAR(10), transactiondatetime, 103) having count(*) > 1 order by data").ToList();
            //}

            //using (var dbb = new digiGappEntities())
            //{
            //    foreach (var item in lista)
            //    {
            //        DateTime D;
            //        DateTime.TryParseExact(item.data, "dd/MM/yyyy", null, DateTimeStyles.None, out D);

            //        var items = dbb.MyRai_MensaXML.Where(aa => aa.Badge == item.badge && aa.LocationID == item.locationid && aa.TransactionID == item.transactionid
            //                    && aa.TransactionDateTime.Year == D.Year && aa.TransactionDateTime.Month == D.Month && aa.TransactionDateTime.Day == D.Day).ToList();
            //        if (items.Count() > 1)
            //        {
            //            foreach (var it in items.OrderBy(xx => xx.id).Skip(1))
            //            {
            //                dbb.MyRai_MensaXML.Remove(it);
            //                dbb.SaveChanges();
            //            }
            //        }

            //    }
            //}



            return;
            DateTime dt1 = new DateTime( 2018 , 10 , 1 );
            DateTime dt2 = new DateTime( 2018 , 10 , 23 );

            MyRaiService1Client clienttest35456 = new MyRaiService1Client ( );


            //MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client cl = new MyRaiService1Client( );
            //cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential( CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[0] , CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[1] );
            //var tewebew = clienttest.GetSchedaPresenzeMese( "527786" , new DateTime( 2019 , 6 , 1 ) , new DateTime( 2019 , 6 , 30 ) );



//            var risultatoTest = clienttest.GetReport_POH_ROH( "103650" , "DDE30" , 6 , 2019 );
//            var risultatoTest2 = clienttest.GetReport_POH_ROH( "103650" , "DDE30" , 7 , 2019 );

//            return;

//            var xUtente = clienttest.recuperaUtente( "103650" , "31072019" );
//            var xUtente2 = clienttest.recuperaUtente( "0103650" , "31072019" );
//            var risultato = clienttest.recuperaUtente( "897573" , "11072019" );
//            var risultato2bis = clienttest.recuperaUtente( "0792353" , "18072019" );
//            var risultato3 = clienttest.recuperaUtente( "103650" , "11072019" );
//            var risultato4 = clienttest.recuperaUtente( "0103650" , "11072019" );
//            return;

//            string[] sediTest = new string[] {
//"1AB01",
//"1AB10",
//"1AB40",
//"1AB41",
//"1AB50",
//"1AB51",
//"1AB52",
//"1AB61",
//"1AB71",
//"1AB80",
//"1ABB0",
//"1ABB1",
//"1ABC1",
//"1ABX0",
//"1AD00",
//"1AV00",
//"1AV01",
//"1AV10",
//"1AV11",
//"1AV20",
//"1AV21",
//"1AV30",
//"1AV40",
//"1AV50",
//"1AV60",
//"1BA00",
//"1BA01",
//"1CA00",
//"1CA01",
//"1CA10",
//"1CA20",
//"1CA30",
//"1CB10",
//"1FA00",
//"1FA01",
//"1FA10",
//"1FA20",
//"1FA50",
//"1FA70",
//"1FA80",
//"1FA90",
//"1FAA0",
//"1FAC0",
//"1FAD0",
//"1FAF0",
//"1FAI0",
//"1FAL0",
//"1LC00",
//"1LK00",
//"1LP00",
//"1LP10",
//"1LP30",
//"1LQ00",
//"1LQ10",
//"1NA00",
//"1NA01",
//"1QA00",
//"1QA01",
//"1QA10",
//"1QA20",
//"1QA30",
//"1QA40",
//"1QA50",
//"1QA60",
//"1QA70",
//"1QA90",
//"1SA00",
//"1SA01",
//"1SA02",
//"1SA10",
//"1SA20",
//"1SA21",
//"1SA30",
//"1SA40",
//"1SA50",
//"1SA60",
//"1SA61",
//"1SA70",
//"1SA80",
//"1SA90",
//"1SB00",
//"1SB10",
//"1SB20",
//"2TA00",
//"2TA01",
//"2TA10",
//"2TA30",
//"2TA40",
//"2TA41",
//"2TA50",
//"2TA60",
//"2TA70",
//"2TA80",
//"2TA90",
//"2TA91",
//"2TF00",
//"2TF01",
//"2TF02",
//"2TF10",
//"2TH00",
//"2TN00",
//"2TN40",
//"2TP00",
//"3CA00",
//"3CB10",
//"3CD00",
//"3CH4A",
//"3CH4B",
//"3CHE0",
//"3CI10",
//"3CI20",
//"3CI30",
//"3CI40",
//"3CI50",
//"3CI60",
//"3CM02",
//"3CM03",
//"3CM09",
//"3CM10",
//"3CM11",
//"3CM14",
//"3CM16",
//"3CM18",
//"3CP00",
//"3CQ00",
//"3CQ01",
//"3CQ30",
//"3CQ40",
//"3CQ50",
//"3CQ60",
//"3CQ70",
//"3DA00",
//"3DA01",
//"3DA10",
//"3DA11",
//"3DA12",
//"3DA20",
//"3DA30",
//"3DA32",
//"3DA40",
//"3DA41",
//"3DG00",
//"3DG02",
//"3DG12",
//"3DG20",
//"3DP00",
//"3DP01",
//"3DP10",
//"3DP20",
//"3DP30",
//"3DP60",
//"3FN10",
//"3MA00",
//"3MA01",
//"3MA11",
//"3MA12",
//"3MB00",
//"3MB01",
//"3MB02",
//"3MB10",
//"3OP00",
//"3OP30",
//"4CA02",
//"4CB80",
//"4CBC0",
//"4CD00",
//"4CJ00",
//"4CJ80",
//"4CLA0",
//"4CS00",
//"4CT00",
//"4DD40",
//"4DT00",
//"4DT01",
//"4EK00",
//"4ES00",
//"4ET00",
//"4ET10",
//"4FR20",
//"4KA02",
//"4TA00",
//"4TB00",
//"4TC00",
//"4TC01",
//"4TF00",
//"4TF01",
//"4TG00",
//"4TG10",
//"4TG11",
//"4TH00",
//"4TH01",
//"4TM00",
//"4TM01",
//"4VM00",
//"4VZ00",
//"4VZ01",
//"4VZ20",
//"5AP00",
//"5AP60",
//"5DA20",
//"5DM00",
//"5DP40",
//"5DP50",
//"5DQ00",
//"5DQ20",
//"5DQ40",
//"5DQ50",
//"5DQ60",
//"5DQ70",
//"5DQ80",
//"5EA10",
//"5EB00",
//"5ED00",
//"5FF00",
//"5FF20",
//"5FP60",
//"5LL00",
//"5LL01",
//"5LL02",
//"5LL03",
//"5LL04",
//"5LL10",
//"5LL20",
//"5PA01",
//"5PD40",
//"5PD50",
//"5PD60",
//"5PD70",
//"5PD80",
//"5PD90",
//"5PE00",
//"5PE01",
//"5PE10",
//"5PE30",
//"5PE40",
//"5PE60",
//"5PG00",
//"5PG10",
//"5PH00",
//"5PN00",
//"5PN10",
//"5PR00",
//"5PR10",
//"5PR20",
//"5PR30",
//"5SE00",
//"5SE01",
//"5SE10",
//"5SE30",
//"5SE40",
//"5SE70",
//"5SE80",
//"5SE90",
//"5SEA0",
//"5SEB0",
//"5SEC0",
//"5TA00",
//"5TA01",
//"5TA20",
//"5TD00",
//"5TD01",
//"5TD10",
//"5TD20",
//"5TD30",
//"5VA00",
//"5VA01",
//"5VF00",
//"5VG00",
//"5VH00",
//"5VH02",
//"5VN00",
//"5VN01",
//"5VN02",
//"5VN12",
//"6CP20",
//"6CP30",
//"8AC00",
//"8AC02",
//"8AC12",
//"8AE00",
//"8AG00",
//"8AG10",
//"8AG20",
//"8AH00",
//"8AK00",
//"8CA00",
//"8CC00",
//"8CC02",
//"8CC12",
//"8CC22",
//"8CC32",
//"8CE00",
//"8CE10",
//"8CE20",
//"8CK00",
//"8DA00",
//"8DC00",
//"8DC02",
//"8DC12",
//"8DC22",
//"8DC32",
//"8DE00",
//"8DG00",
//"8DG10",
//"8DR00",
//"8EL00",
//"8GG00",
//"8LA00",
//"8LC00",
//"8LC10",
//"8LC12",
//"8LE00",
//"8LG00",
//"8LK00",
//"8LK10",
//"8NG00",
//"9AA02",
//"9AC02",
//"9AC20",
//"9AE00",
//"9AG00",
//"9AG10",
//"9AK00",
//"9AU02",
//"9AU10",
//"9AV00",
//"9BC00",
//"9BC02",
//"9BC10",
//"9BC12",
//"9BE00",
//"9BG00",
//"9BG10",
//"9BH00",
//"9BV00",
//"9DE00",
//"9DG00",
//"9DH00",
//"9DK00",
//"9EC00",
//"9EC02",
//"9FGA1",
//"9FGA3",
//"9FGA4",
//"9FGB0",
//"9FGB1",
//"9FGB2",
//"9FGB3",
//"9FGB7",
//"9FGC0",
//"9FGD0",
//"9FGD1",
//"9FGD2",
//"9FGE0",
//"9FGE1",
//"9FGE2",
//"9KC00",
//"9KC10",
//"9KC12",
//"9LD00",
//"9LD01",
//"9LD10",
//"9LD20",
//"9LD50",
//"9LD60",
//"9LD70",
//"9LD80",
//"9LD90",
//"9LDA0",
//"9LDB0",
//"9LDC0",
//"9LDD0",
//"9LDF0",
//"9LDG0",
//"9LDH0",
//"9LDI0",
//"9LDK0",
//"9LE30",
//"9LE40",
//"9LE70",
//"9LE80",
//"9LE90",
//"9LEA0",
//"9LEB0",
//"9PC00",
//"9PC02",
//"9PC10",
//"9PC12",
//"9PC32",
//"9PC42",
//"9PD00",
//"9PD02",
//"9PD10",
//"9PE00",
//"9PE20",
//"9PF00",
//"9PF30",
//"9PF60",
//"9PH00",
//"9PH10",
//"9PK00",
//"9PV00",
//"9PV02",
//"9RC00",
//"9RC02",
//"9RE00",
//"9RG00",
//"9RG10",
//"9RK00",
//"DAA00",
//"DAA10",
//"DAC00",
//"DAC01",
//"DCA00",
//"DDA00",
//"DDC00",
//"DDC10",
//"DDC20",
//"DDCA0",
//"DDD01",
//"DDD02",
//"DDD10",
//"DDD12",
//"DDD20",
//"DDD30",
//"DDD40",
//"DDD50",
//"DDD60",
//"DDD80",
//"DDE10",
//"DDE20",
//"DDE30",
//"DDE40",
//"DDE50",
//"DEA00",
//"DEA01",
//"DEA10",
//"DEA20",
//"DEA30",
//"DEC00",
//"DEC10",
//"DFA00",
//"DFB00",
//"DTO00",
//"DTO11",
//"EAA00",
//"EAA11",
//"EAB00",
//"EAC00",
//"QAA00",
//"QAA01",
//"QAC00",
//"QAC01",
//"QAD00",
//"QAD01",
//"QAD10",
//"QAE00",
//"QAE01",
//"QAE20",
//"QAE30",
//"QAE40",
//"SBA00",
//"SBA01",
//"SBA10",
//"SBA11",
//"SBA20",
//"SBA21",
//"SBA30",
//"SBA40",
//"SBA50",
//"SBA60",
//"SBA70",
//"SBA80",
//"SBB00",
//"SBB10",
//"SBB20",
//"SBB30",
//"SBB40",
//"SBB50",
//"SGA10",
//"WBA00",
//"WBA01",
//"WBA02"
//            };

//            var db = new digiGappEntities();
//            var output1 = clienttest.GetDettagli( sediTest.Take(484).ToArray() , new DateTime( 2019 , 05 , 01 ) , new DateTime( 2019 , 07 , 31 ) , MyRaiServiceInterface.MyRaiServiceReference1.EnumWorkflowRichieste.TuttiISettori , MyRaiServiceInterface.MyRaiServiceReference1.EnumStatiRichiesta.InApprovazione );

//            var output2 = clienttest.GetDettagli( sediTest.Skip(80).Take( 80 ).ToArray( ) , new DateTime( 2019 , 05 , 01 ) , new DateTime( 2019 , 07 , 31 ) , MyRaiServiceInterface.MyRaiServiceReference1.EnumWorkflowRichieste.TuttiISettori , MyRaiServiceInterface.MyRaiServiceReference1.EnumStatiRichiesta.InApprovazione );

//            var output3 = clienttest.GetDettagli( sediTest.Skip( 160 ).Take( 80 ).ToArray( ) , new DateTime( 2019 , 05 , 01 ) , new DateTime( 2019 , 07 , 31 ) , MyRaiServiceInterface.MyRaiServiceReference1.EnumWorkflowRichieste.TuttiISettori , MyRaiServiceInterface.MyRaiServiceReference1.EnumStatiRichiesta.InApprovazione );

//            var output4 = clienttest.GetDettagli( sediTest.Skip( 240 ).Take( 80 ).ToArray( ) , new DateTime( 2019 , 05 , 01 ) , new DateTime( 2019 , 07 , 31 ) , MyRaiServiceInterface.MyRaiServiceReference1.EnumWorkflowRichieste.TuttiISettori , MyRaiServiceInterface.MyRaiServiceReference1.EnumStatiRichiesta.InApprovazione );

//            var output5 = clienttest.GetDettagli( sediTest.Skip( 320 ).Take( 80 ).ToArray( ) , new DateTime( 2019 , 05 , 01 ) , new DateTime( 2019 , 07 , 31 ) , MyRaiServiceInterface.MyRaiServiceReference1.EnumWorkflowRichieste.TuttiISettori , MyRaiServiceInterface.MyRaiServiceReference1.EnumStatiRichiesta.InApprovazione );

//            var output6 = clienttest.GetDettagli( sediTest.Skip( 400 ).Take( 80 ).ToArray( ) , new DateTime( 2019 , 05 , 01 ) , new DateTime( 2019 , 07 , 31 ) , MyRaiServiceInterface.MyRaiServiceReference1.EnumWorkflowRichieste.TuttiISettori , MyRaiServiceInterface.MyRaiServiceReference1.EnumStatiRichiesta.InApprovazione );
            
//            var output7 = clienttest.GetDettagli( sediTest.Skip( 480 ).Take( 8 ).ToArray( ) , new DateTime( 2019 , 05 , 01 ) , new DateTime( 2019 , 07 , 31 ) , MyRaiServiceInterface.MyRaiServiceReference1.EnumWorkflowRichieste.TuttiISettori , MyRaiServiceInterface.MyRaiServiceReference1.EnumStatiRichiesta.InApprovazione );

//            var result213464 = clienttest.GetAnalisiEccezioni( "0030316" , new DateTime( 2019 , 1 , 1 ) , new DateTime( 2019 , 7 , 12 ) , "LN20" , null , null );



//            var pppppppppppppppp = clienttest.GetAnalisiEccezioni( "862939" , dt1 , dt2 , "LN25" , "" , "" );
//            //ServiceReference1.MyRaiService1Client test = new ServiceReference1.MyRaiService1Client();
//            //var u = test.GetLista_Eccezioni_Giornalisti("103650", "9RC02", 5, 2019);

//            //var sti = clienttest.presenzeGiornaliere( "103650" , "DDE30" , "03072019" );
//            //DateTime dt1 = new DateTime( 2019 , 1 , 1 );
//            //DateTime dt2 = new DateTime( 2019 , 7 , 3 );
//            //var getA = clienttest.GetAnalisiEccezioni( "103650" , dt1 , dt2 , "FE", "", "" );

//            return;

//            DateTime data = new DateTime( 2019 , 06 , 11 , 0 , 0 , 0 );
//            var marco = clienttest.GetNoteRichieste( "527786" , data );
//            return;


//            var resultTest = clienttest.GetReport_POH_ROH ( "451598" , "DDE30" , 6 , 2018 );
//            var resultTest2 = clienttest.GetReport_Carenza_MP ( "451598" , "DDE30" , 6 , 2018 );
//            var resultTest3 = clienttest.GetReport_STR_STRF ( "451598" , "DDE30" , 6 , 2018 );
//            var resultTest4 = clienttest.GetReport_Reperibilita ( "451598" , "DDE30" , 6 , 2018 );

//            return;

           

//          //  WSDigigapp service = new WSDigigapp();
//          //  service.Credentials = new System.Net.NetworkCredential("srvruofpo", "zaq22?mk");
            

//            var batch = db.MyRai_PianoFerieBatch.ToList();
//            var dbg = db.MyRai_PianoFerieGiorni.ToList();
//            var pfs = db.MyRai_PianoFerieSedi.ToList();

//            var sedigapp = db.MyRai_PianoFerieSedi.Select(sss => sss.sedegapp).Distinct().OrderBy(sss=>sss).ToList();
//            int co = 0;
//            foreach (var sede in sedigapp)
//            {
//                var row= pfs.Where(item => item.sedegapp == sede).OrderBy(item => item.numero_versione).FirstOrDefault();
//                int q = pfs.Where(item => item.sedegapp == sede).Count();
//                int f = pfs.Where(item => item.sedegapp == sede && item.data_firma !=null).Count();

//                if (q > 1)
//                {
//                    Console.WriteLine("sede " + sede + " record:" + q + " - firmati:"+f + " - ultima vers:" +  (row.data_firma!=null ?" firmata" : " da firmare")    );
//                }
//                if (row.data_firma != null)
//                {
//                    var matr = db.MyRai_PianoFerie.Where(item => item.sedegapp == row.sedegapp).Select(item => item.matricola).ToList();
//                    foreach (string m in matr)
//                    {
//                        var gg = dbg.Where(item => item.matricola == m).ToList();
//                        foreach (var giorno in gg)
//                        {
//                            co++;
//                            if (!batch.Any(item => item.matricola == m && item.data_eccezione == giorno.data))
//                            { 
//                                Console.WriteLine( "Non inserito da batch: " + m + giorno.data);
//                            }
//                        }
//                    }
//                }
//            }

            
//            foreach (var bi in batch.Where (item=>item.data_inserimento_gapp != null))
//            {
//                if (! dbg.Any(item => item.matricola == bi.matricola && bi.data_eccezione == item.data && item.eccezione==bi.codice_eccezione))
//                {
//                    Console.WriteLine("Non presente : " + bi.matricola + " " + bi.data_eccezione);
//                }
//            }
//            return;


//            //MyRaiService1Client client32 = new MyRaiService1Client();
//            //var risultato1 = client32.GetReport_Carenza_MP("527786", "DDE30", 1, 2019);
//            //var risultato2 = client32.GetReport_POH_ROH("527786", "DDE30", 2, 2019);
//            //var risultato3 = client32.GetReport_STR_STRF("527786", "DDE30", 3, 2019);
//            //var risultato4 = client32.GetReport_Reperibilita("527786", "DDE30", 12, 2018);
//            //client32.Close();

            
//            //MyRaiService1Client client32 = new MyRaiService1Client();
            
//            //var test32 = client32.GetInfoMensa("527786", "527786", new DateTime(2018, 10, 25));

//            //client32.Close();

//            return;

//            //var lll = GetInfoMensa("527786", "527786", DateTime.Now);
//            //return;

//            using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, "RAI", null, ContextOptions.Negotiate,
//              "srvruofpo","zaq22?mk"))
//            {

//                //string nomeDom= System.Security.Principal.WindowsIdentity.GetCurrent().Name;
//                //if (nomeDom.Contains("\\"))
//                //    matricola = nomeDom.Split('\\')[1];
//                //else
//                //    matricola = nomeDom;

//                UserPrincipal user = UserPrincipal.FindByIdentity(pc, IdentityType.SamAccountName, "001045");
//                UserPrincipal user2 = UserPrincipal.FindByIdentity(pc, IdentityType.SamAccountName, "P001045");
//            }
//            return;


//                SessionResult s = new SessionResult();
//            s.ImportCsvFile("ppx3038.csv");

            
//            var resultPerOra = db.MyRai_LogAzioni.Where(xx => xx.operazione == "SESSION_START")
//            .GroupBy(item => new { y = item.data.Year, m = item.data.Month, d = item.data.Day, h = item.data.Hour })
//            .Select(
//              grp => new SessionResult
//              {
//                  d = grp.Key.d,
//                  m = grp.Key.m,
//                  y = grp.Key.y,
//                  h = grp.Key.h,
//                  tot = grp.Count()
//              }).OrderBy(z => z.y).ThenBy(z => z.m).ThenBy(z => z.d).ThenBy(z => z.h).ToList();



//            var resultPerGiorno = db.MyRai_LogAzioni.Where(xx => xx.operazione == "SESSION_START")
//            .GroupBy(item => new { y = item.data.Year, m = item.data.Month, d = item.data.Day })
//            .Select(
//              grp => new SessionResult
//              {
//                  d = grp.Key.d,
//                  m = grp.Key.m,
//                  y = grp.Key.y,
//                  tot = grp.Count()
//              }).OrderBy(z => z.y).ThenBy(z => z.m).ThenBy(z => z.d).ToList();


//            MyRaiService1Client client = new MyRaiService1Client();


//            var result = client.InserisciNotaSegreteria("527786", dt, null, null, null, "", "test");

//            client.Close();

//            return;



//            WSDigigapp service = new WSDigigapp();
//            service.Credentials = new System.Net.NetworkCredential("srvruofpo", "zaq22?mk");



            




//            List<string> sssedi = new List<string>();
//            sssedi.Add("DDE30");
//            var x = service.getEccezioniDaValidare("527786", "01012019", "27022019", "", "1", sssedi.ToArray(), 75);


//            return;

//            //InserisciPatentiGAVU();
//            //return;



//            Boolean? b = null;
//            bool a = (bool)b;

//            ProvvedimentiCause p = new ProvvedimentiCause();
//            var Response= p.GetProvvedimentiCause();
//            return;



//            string track = "12345678901234567890123420190208STR100011009999111111SEDE1222222SEDE2333333SEDE3";
//            TrackToClassSolver so = new TrackToClassSolver();
//            ClassePadreTest c = so.GetClassFromTrack<ClassePadreTest>(track);
//            return;
            //ParallelTest pt = new ParallelTest();
            //pt.Call();

            //return;
            //var db = new digiGappEntities();
            //byte[] contenuto = System.IO.File.ReadAllBytes("d:\\leo2.pdf");
            //var p = db.MyRai_Moduli.Where(x => x.id == 31).FirstOrDefault();
            //p.bytes_content = contenuto;
            //db.SaveChanges();
            //return;

            //List<string> files = new List<string>() {"a0x","TI","TD","a7x","mxx" };
            //foreach ( string f in files)
            //{
            //    var modulo = db.MyRai_Moduli.Where(a => a.destinatario == f).FirstOrDefault();
            //    byte[] contenuto = System.IO.File.ReadAllBytes(f + ".pdf");
            //    modulo.bytes_content = contenuto;
            //}
            //db.SaveChanges();



            //var listPDF = db.MyRai_Moduli.ToList();
            //foreach (var p in listPDF)
            //{
            //    System.IO.File.WriteAllBytes(p.destinatario + ".pdf", p.bytes_content);
            //}


            //using (var db = new digiGappEntities())
            //{
            //    DIGIRESP_Archivio_PDF pdf = new DIGIRESP_Archivio_PDF();


            //    pdf.attivo = true;
            //    pdf.data_inizio = new DateTime(2018, 12, 23);
            //    pdf.data_fine = new DateTime(2018, 12, 31);
            //    pdf.sede_gapp = "DDD10";
            //    pdf.numero_versione = 1;
            //    pdf.matricola_stampa = "ESSWEB";
            //    pdf.data_stampa = DateTime.Now;
            //    pdf.stato_pdf = "S_OK";
            //    pdf.tipologia_pdf = "R";
            //    pdf.pdf = System.IO.File.ReadAllBytes("d:\\a.pdf");
            //    db.DIGIRESP_Archivio_PDF.Add(pdf);
            //    db.SaveChanges();

            //}
            //var db = new digiGappEntities();
            //var list = db.MyRai_LogErrori.ToList();

            //string s= Newtonsoft.Json.JsonConvert.SerializeObject(list);

            //MyRaiService1Client client = new MyRaiService1Client();

            //var x = client.PresenzeSettimanali("103650", "21012019", "23012019");

            //client.Close();

            //return;


            MyRaiService1Client client2 = new MyRaiService1Client();

            string[] sedi = new string[] {
            "5LL1041",
            "1AV40",
            "5TD3022",
            "5PG10",
            "1FA1036",
            "1QA10",
            "9PK00",
            "1NA01",
            "1FAI015",
            "QAE00",
            "5VA0012",
            "2TA10",
            "1AV30",
            "1FAI018",
            "5VA0015",
            "1QA01",
            "1QA00",
            "SBB10",
            "2TF10",
            "1FAC052",
            "2TF02",
            "8LE00",
            "2TA00",
            "2TA01",
            "1AV20",
            "5PD70",
            "5AA04",
            "SBB00",
            "WBA01",
            "WBA00",
            "2TF00",
            "2TF01",
            "1FA20",
            "1FA5048",
            "1CB10",
            "5TD2012",
            "5PE1021",
            "1CA40",
            "9PA22",
            "5TA00",
            "5PR30",
            "5PD60",
            "2TP00",
            "1SA70",
            "9BG10",
            "DDD50",
            "1FAD044",
            "9LD90",
            "1FA1032",
            "1CA30",
            "1SA10",
            "5PR20",
            "5PD50",
            "1SA61",
            "5TD3023",
            "1FAI011",
            "1FA1035",
            "9LD80",
            "5LL20",
            "1FAI014",
            "5VA0011",
            "1CA20",
            "5PR10",
            "5PD40",
            "9PV00",
            "DDC10",
            "1SA50",
            "5VA0014",
            "1FAL0",
            "5LL10",
            "8LA00",
            "8LA01",
            "1FAD042",
            "DDA00",
            "9AV00",
            "SBA80",
            "SBB40",
            "DAC00",
            "1BA0023",
            "5TD2014",
            "EAA11",
            "1SA40",
            "9AK00",
            "8LG00",
            "5LL04",
            "5LL03",
            "5LL02",
            "5FP60",
            "5LL00",
            "1SA60",
            "5TD30",
            "5TD2013",
            "6CP30",
            "9BC12",
            "9BC10",
            "1BA01",
            "1BA00",
            "1SA30",
            "1BA0022",
            "1FAA0",
            "8AK00",
            "5PE60",
            "5LL2052",
            "DDCA0",
            "1BA0025",
            "1FAA027",
            "9BC02",
            "9BC01",
            "5EA10",
            "1QA90",
            "1SA21",
            "1SA20",
            "DAA01",
            "9PH00",
            "DFB00",
            "9LDC0",
            "2TA90",
            "2TA91",
            "5FF0011",
            "5EA00",
            "5TA01",
            "5PE40",
            "SBB20",
            "3DA1034",
            "1FA9035",
            "4FR20",
            "1AV21",
            "9LDB0",
            "5PE1023",
            "2TA80",
            "2TN00",
            "5PE30",
            "9LD70",
            "9LDA0",
            "5PE1024",
            "9PE00",
            "QAE20",
            "9BC00",
            "8LC30",
            "SBA20",
            "EAA00",
            "1FA70",
            "9LD60",
            "EAB00",
            "1ABB0",
            "1BA0021",
            "5PN10",
            "1FAA023",
            "5LL2053",
            "9BG00",
            "5PN00",
            "SBA70",
            "3CM11",
            "1FAL001",
            "1BA0024",
            "8LC20",
            "3CM18",
            "1FA1031",
            "9LD50",
            "3MB10",
            "1AV11",
            "1AV10",
            "DDD60",
            "1FA2022",
            "1FAI017",
            "1SA02",
            "1FA8047",
            "9PE20",
            "1FA10",
            "QAE01",
            "8LC12",
            "8LC10",
            "5FF0012",
            "3MB00",
            "5EB00",
            "9LD40",
            "1AV01",
            "1AV00",
            "9PA12",
            "1AB0021",
            "SBA50",
            "5TD20",
            "1FA8045",
            "1FA01",
            "1FA00",
            "DEA10",
            "8LC00",
            "DDC00",
            "3DA41",
            "3DA40",
            "3DA42",
            "DDD40",
            "9PA01",
            "SBA30",
            "SBA40",
            "5TD10",
            "1FA7013",
            "DEA00",
            "DEA01",
            "1QA70",
            "9BV00",
            "1FAI0",
            "5VH02",
            "5VH01",
            "5VH00",
            "8AH00",
            "1CA10",
            "DDD30",
            "5PR00",
            "EAC00",
            "9PD10",
            "5TD01",
            "5TD00",
            "1AB72",
            "1QA60",
            "DDC20",
            "1FAA025",
            "3DA20",
            "1CA01",
            "1CA00",
            "DDD20",
            "1FA5044",
            "1FAI013",
            "1FA8043",
            "SBA21",
            "2TA60",
            "DAC01",
            "9AE00",
            "1AB61",
            "1AB60",
            "1FA8046",
            "DEA20",
            "3MB01",
            "DDD12",
            "5FF0013",
            "DDD80",
            "2TA50",
            "DDE50",
            "8AE00",
            "9PF30",
            "DDD10",
            "1QA40",
            "1FA1034",
            "DDD02",
            "DDD01",
            "SBB50",
            "2TA40",
            "2TA41",
            "DDE40",
            "5FF00",
            "1QA30",
            "9PC42",
            "1FA7012",
            "1FAD046",
            "1AB01",
            "5LL2054",
            "1SA01",
            "1SA00",
            "DDE30",
            "1QA20",
            "9PC32",
            "1FAA021",
            "5LL2051",
            "5PH00",
            "1FAF046",
            "9AA02",
            "9AA01",
            "2TA70",
            "8LK10",
            "QAE30",
            "2TA20",
            "DEA30",
            "DDE20",
            "1FAC051",
            "1FAI012",
            "5PE20",
            "1FA90",
            "1FAD043",
            "3DA1033",
            "9AU10",
            "SBA60",
            "5LL01",
            "5VA01",
            "5VA00",
            "8LK00",
            "5FF0014",
            "5TD3024",
            "5PE10",
            "1FA80",
            "9PC12",
            "5VF00",
            "9PC10",
            "1FAD048",
            "9AU02",
            "5TD3021",
            "3CM02",
            "DEC10",
            "1QA50",
            "3CM09",
            "3MA12",
            "5PE01",
            "5PE00",
            "5TA20",
            "1SB10",
            "9PC02",
            "9PC00",
            "1FAF0",
            "6CP20",
            "DEC00",
            "9BH00",
            "3MA01",
            "3MA00",
            "1ABC1",
            "5PG00",
            "5ED00",
            "1SB00",
            "DFA00",
            "9AG10",
            "8AG20",
            "5FF20",
            "DAA00",
            "1ABB1",
            "5TD2011",
            "1FAD0",
            "9AC40",
            "9AG00",
            "8AG10",
            "3DA1032",
            "5PD90",
            "9PH10",
            "1FAD045",
            "2TH00",
            "1FA9036",
            "9PF00",
            "9BE00",
            "1FA1037",
            "DDE3005",
            "5VG00",
            "8AG00",
            "1FAA028",
            "DDE10",
            "5PD80",
            "5PA01",
            "1FAI016",
            "5VA0013",
            "2TN40",
            "DCA00",
            "3DA11",
            "3DA10",
            "9AC20",
            "SBA11",
            "SBA10",
            "9PD02",
            "9PD00",
            "1SA80",
            "5EA21",
            "3DA01",
            "3DA00",
            "3MB02",
            "SBA01",
            "SBA00",
            "1AV60",
            "1AB10",
            "1AB41",
            "1AB40",
            "1FAC0",
            "9PV02",
            "5PE1022",
            "9AC02",
            "8AC12",
            "1AV50",
            "SBB30",
            "1FAD041",
            "3DA1031",
            "1FA50",
            "2TA30",
            "QAE40",
            "1SA90",
            "9PF60",
            "1FA1033",
            "8AC02",
            "8AC00"
            };

            try
            {
                var sett = client2.GetNoteRichiestePerSedeGapp("103650", sedi, "Segreteria");
            }
            catch (Exception ex)
            {

            }


            return;
            //WebClient w = new WebClient();
            //w.UseDefaultCredentials = true;
            //w.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            //string a= w.DownloadString("http://raiperme.intranet.rai.it");

            //MyRaiService1Client client = new MyRaiService1Client();

            //var sett = client.SetLettura( 4, "527786", "Lotti Cristiano" );


            //var x = client.GetNoteRichieste( "103650", new DateTime( 2018, 11, 26 ) );


            //GetTimbratureOspiti("103650", "00124290", "000000" );

            //MyRaiService1Client client = new MyRaiService1Client();

            //var x = client.GetAnalisiEccezioni( "086752", new DateTime( 2018, 10, 1 ), new DateTime( 2018, 10, 31 ), "FE", "", "" );
            //var x = client

            //GetAnalisiEccezioni

            //CercaStorniUMHAutoApprovati();
            //InserisciPatentiGAVU();

            //string elenco = "";
            //var ab = myRaiCommonTasks.CommonTasks.getAbilitazioni();

            //var lista = ab.ListaAbilitazioni.Where( w => w.MatrLivello1.Any() ).ToList();

            //if ( lista != null && lista.Any() )
            //{
            //	lista.ForEach( l =>
            //	{
            //		string matricole = "";
            //		if ( l.MatrLivello1 != null && l.MatrLivello1.Any() )
            //		{
            //			l.MatrLivello1.ForEach( m =>
            //			{
            //				matricole += m.Matricola + "|";
            //			} );
            //		}

            //		elenco += String.Format( "Matricola {0}, Codice sede {1}, descrizione sede {2} \r\n", matricole, l.Sede, l.DescrSede );
            //	} );
            //}

            //System.IO.File.WriteAllText( AppDomain.CurrentDomain.BaseDirectory + "\\ElencoApprovatoriLivello1.txt", elenco );
            return;

            //MyRaiService1Client client = new MyRaiService1Client();

            //var risposta = client.GetStatoEccezioniGiornate( "865795", new DateTime( 2018, 10, 1 ), new DateTime( 2018, 10, 31 ) );

            //var risposta2 = client.GetStatoEccezioniGiornate( "103650", new DateTime( 2018, 11, 1 ), new DateTime( 2018, 11, 30 ) );

            //var risposta3 = client.GetStatoEccezioniGiornate( "332693", new DateTime( 2018, 11, 1 ), new DateTime( 2018, 11, 30 ) );
            //return;
            //int anno = 2018;

            //MyRaiService1Client client = new MyRaiService1Client();

            //var x = client.GetVisualizzazioneGiornata( "103650", DateTime.Now, null );

            //GetAnalisiEccezioniResponse response = client.GetAnalisiEccezioni( "505065",
            //																	new DateTime( ( int )anno, 1, 1 ),
            //																	new DateTime( ( int )anno, 12, 31 ),
            //																	"POH",
            //																	"ROH",
            //																	null
            //																	);


            //if ( response != null && response.DettagliEccezioni != null )
            //{
            //	foreach ( var d in response.DettagliEccezioni )
            //	{
            //		d.data = new DateTime( ( int )anno, d.data.Month, d.data.Day );
            //	}
            //}

            //var pluto = response.DettagliEccezioni.Where( x => x.data.Month == 10 && x.eccezione == "POH" )
            //				.Select( x => new 
            //				{
            //					Giorno = x.data,
            //					Minuti = x.minuti
            //				} ).ToList();


            return;
            //ControllaRMTR();
            //return;

            //mi();
            //return;

            //MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client wcf1 = new MyRaiServiceInterface. MyRaiServiceReference1.MyRaiService1Client();

            //wcf1.GetPresenzeSettimanaliProtected("162077",
            //		 "14102018", "20102018",Utente.DataInizioValidita(),datainizio));

            //VerificaStatoRichiestaInGapp();
            //return;


            //AggiornaPDF();
            // return;
            //// VerificaStorni();
            // return;

            // MyRaiService1Client client = new MyRaiService1Client();

            // var risultato = client.PresenzeSettimanali( "880590", "01082018", "31082018" );

            // return;

            // var result = client.presenzeGiornaliere( "486695", "QAE40", DateTime.Today.AddDays( -32 ).ToString( "ddMMyyyy" ) );

            // client.Close();

            // return;

            //	MyRaiService1Client client = new MyRaiService1Client();

            //	var result = client.InserisciNotaSegreteria( "0103650", DateTime.Now, null, null, null, null, "test" );

            //	client.Close();

            //MyRaiService1Client client = new MyRaiService1Client();

            //var response = client.GetSituazioneDebitoria( "676140", "676140" );

            //var x = client.GetNoteSegreteria( "315025", DateTime.Now );


            //VisualizzazioneGiornataResponse x = client.SetVisualizzazioneGiornata( "909317", false, "103650", "Francesco", DateTime.Now, null );

            //VisualizzazioneGiornataResponse y = client.SetVisualizzazioneGiornata( "909317", true, "103650", "Francesco", DateTime.Now, null );

            //VisualizzazioneGiornataResponse z = client.GetVisualizzazioneGiornata( "909317", DateTime.Now, true );

            //VisualizzazioneGiornataResponse k = client.GetVisualizzazioneGiornata( "909317", DateTime.Now, null );

            //VisualizzazioneGiornataResponse j = client.GetVisualizzazioneGiornata( "909317", DateTime.Now, false );

            //return;

            //try
            //{
            //	DirectoryInfo di = new DirectoryInfo( "D:\\Compressi\\Compressi\\" );
            //	FileInfo[] files = di.GetFiles( "*.pdf" );

            //	if ( files != null && files.Any() )
            //	{
            //		foreach ( var myFile in files )
            //		{
            //			using ( var db = new digiGappEntities() )
            //			{
            //				DIGIRESP_Archivio_PDF pdf = new DIGIRESP_Archivio_PDF();
            //				string nomeFile = myFile.Name.Replace( myFile.Extension, "" );

            //				pdf.attivo = true;
            //				pdf.data_inizio = new DateTime( 2018, 7, 30 );
            //				pdf.data_fine = new DateTime( 2018, 7, 31 );
            //				pdf.sede_gapp = nomeFile;
            //				pdf.numero_versione = 1;
            //				pdf.matricola_stampa = "ESSWEB";
            //				pdf.data_stampa = DateTime.Now;
            //				pdf.stato_pdf = "S_OK";
            //				pdf.tipologia_pdf = "R";
            //				pdf.pdf = System.IO.File.ReadAllBytes( myFile.FullName );
            //				db.DIGIRESP_Archivio_PDF.Add( pdf );
            //				db.SaveChanges();
            //			}
            //		}
            //	}
            //}
            //catch ( Exception ex )
            //{
            //	throw new Exception( ex.Message );
            //}

            //try
            //{
            //	using ( var db = new digiGappEntities() )
            //	{
            //		var myF = db.DIGIRESP_Archivio_PDF.Where( f => f.ID > 3455 ).ToList();

            //		if ( myF != null && myF.Any() )
            //		{
            //			foreach(var f in myF)
            //			{
            //				System.IO.File.WriteAllBytes("D:\\Compressi\\Compressi\\copy_" + f.sede_gapp + ".pdf", f.pdf );
            //			}
            //		}
            //	}
            //}
            //catch ( Exception ex )
            //{
            //	throw new Exception( ex.Message );
            //}

            //byte[] bytes = System.IO.File.ReadAllBytes( filename );

            //foreach (string file in Directory.EnumerateFiles("D:\\Compressi\\Compressi\\", "*.pdf"))
            //{
            //	string contents = File.ReadAllText(file);
            //}

            //pdf.attivo = true;
            //pdf.data_inizio = new DateTime(2018, 7, 30);
            //pdf.data_fine = new DateTime(2018, 7, 31);
            //pdf.sede_gapp = "1FA00";
            //pdf.numero_versione = 1;
            //pdf.matricola_stampa = "ESSWEB";
            //pdf.data_stampa = DateTime.Now;
            //pdf.stato_pdf = "S_OK";
            //pdf.tipologia_pdf = "R";
            //pdf.pdf = System.IO.File.ReadAllBytes( "D:\\Compressi\\Compressi\\1FA00.pdf" );
            //db.DIGIRESP_Archivio_PDF.Add(pdf);

            //var pdf2 = new DIGIRESP_Archivio_PDF();
            //pdf2.attivo = true;
            //pdf2.data_inizio = new DateTime(2018, 5, 21);
            //pdf2.data_fine = new DateTime(2018, 5, 27);
            //pdf2.sede_gapp = "3CM02";
            //pdf2.numero_versione = 1;
            //pdf2.matricola_stampa = "593945";
            //pdf2.data_stampa = DateTime.Now;
            //pdf2.stato_pdf = "S_OK";
            //pdf2.tipologia_pdf = "R";
            //pdf2.pdf = System.IO.File.ReadAllBytes("d:\\3cm02.pdf");
            //db.DIGIRESP_Archivio_PDF.Add(pdf2);

            //db.SaveChanges();

            return;


            //if (pdf != null)
            //{
            //	System.IO.File.WriteAllBytes("D:\\" + idPDF.ToString() + ".pdf", pdf.pdf);
            //}
            //return;

            //string fileName = "C:\\Users\\eam0708\\Desktop\\e.xlsx";
            //var workbook = new XLWorkbook(fileName);
            //var ws1 = workbook.Worksheet(1);
            //List<ec> list = new List<ec>();

            //var listaEcc= db.L2D_ECCEZIONE.ToList();
            //for (int row = 2; row < 10000; row++)
            //{
            //	string codiceEcc = ws1.Row(row).Cell(3).Value.ToString();
            //	var eccezioneL2 = listaEcc.Where(x => x.cod_eccezione.Trim() == codiceEcc.Trim()).FirstOrDefault();
            //	int idTipoAss = 3; //sconosciuta
            //	if (eccezioneL2 != null)
            //	{
            //		if (eccezioneL2.unita_misura == "G") idTipoAss = 2;
            //		if (eccezioneL2.unita_misura == "H") idTipoAss = 1;
            //	}
            //	list.Add(new ec() {
            //		codice = ws1.Row(row).Cell (3).Value.ToString(),
            //		descrizione = ws1.Row(row).Cell(4).Value.ToString(),
            //		cose  = ws1.Row(row).Cell(5).Value.ToString(),
            //		vincoli = ws1.Row(row).Cell(8).Value.ToString(),
            //		note = ws1.Row(row).Cell(9).Value.ToString(),
            //		idTipoAssenza=idTipoAss
            //	});
            //}
            //foreach (var ecc in listaEcc)
            //{
            //	var eccExcel = list.Where(x => x.codice.Trim().ToLower() == ecc.cod_eccezione.Trim().ToLower()).FirstOrDefault();
            //	if (eccExcel != null)
            //	{
            //		myRaiData.MyRai_Regole_SchedeEccezioni reg = new MyRai_Regole_SchedeEccezioni();
            //		reg.codice = eccExcel.codice;
            //		reg.data_inizio_validita = DateTime.Now;
            //		reg.descrittiva = eccExcel.descrizione;
            //		reg.criteri_inserimento = "";
            //		reg.trattamento_economico = "";
            //		reg.note = eccExcel.note;
            //		reg.definizione = eccExcel.cose;
            //		reg.id_tipo_assenza = eccExcel.idTipoAssenza;
            //		reg.presupposti_documentazione = eccExcel.vincoli;
            //		db.MyRai_Regole_SchedeEccezioni.Add(reg);

            //		db.SaveChanges();
            //	}
            //}
            //return;

            //ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics();
            //string matricola = "103650";
            //string S = areaSistema( "3PF", matricola, 80 );

            //matricola = matricola.PadLeft( 7, '0' );

            //S += matricola + "01012018";

            //string s2;

            //s2 = ( string )c.ComunicaVersoCics( S.PadRight( 8000 ) );

            //s2 = s2.Replace( Convert.ToChar( 0x0 ).ToString(), " " );

            //string sedeGapp = s2.Substring( 326, 5 );


            //ComunicaCics.ComunicaVersoCics c = new ComunicaCics.ComunicaVersoCics();
            //string response = ( string )c.ComunicaVersoCics( "P956,FTR,807777    ,000                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                " );
            //int a = 10;









            // HttpClient hh;
            // string urlRequest  = "http://svilraiplace3.intranet.rai.it";
            // string urlComplete = "http://svilraiplace3.intranet.rai.it/wp-json/raiplace_rest/user_theme/p103650/4";

            // var pairs = new List<KeyValuePair<string, string>>();
            // //{
            // //    new KeyValuePair<string, string>("login", "abc")
            // //};
            // CredentialCache credCache = new CredentialCache();
            // credCache.Add(new Uri(urlRequest),"Negotiate", CredentialCache.DefaultNetworkCredentials);

            // //var content = new FormUrlEncodedContent(pairs);
            // //  //  System.Net.Http.HttpClient h = new System.Net.Http.HttpClient();
            // //    var client = new System.Net.Http.HttpClient (new HttpClientHandler(){ 
            // //        UseCookies=true,
            // //     Credentials=credCache,
            // //    // AllowAutoRedirect=true,
            // //     CookieContainer= new CookieContainer (), PreAuthenticate=true
            // //    }) { BaseAddress = new Uri(urlRequest) };

            // //    client.DefaultRequestHeaders.Add("keystring", "59881203878321920");
            // //    // call sync
            // //    var resss = client.PostAsync("/wp-json/raiplace_rest/user_theme/p103650/4", content).Result; 

            // WebClientApiRaiPlace w = new WebClientApiRaiPlace("59881203878321920", "http://svilraiplace3.intranet.rai.it/wp-json/raiplace_rest/user_theme/p103650/4");
            // string risp = w.ExecuteCall();
            //  //  w.Credentials = credCache;

            //     //w.Headers.Add("keystring", "59881203878321920");

            //    // byte[] b = w.DownloadData(urlComplete);
            //     string resp = w.DownloadString(urlComplete);
            //     Console.Write(resp);
            //     return;


            // Uri uri = new Uri(urlComplete);

            // HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uri);

            //// CredentialCache cr = new CredentialCache();
            //// cr.Add (new Uri("http://svilraiplace3.intranet.rai.it/wp-json/raiplace_rest/user_theme/p103650/4"), 
            ////  "Basic",
            //// //    new NetworkCredential("ERIC722","23232323","RAI")) ;
            ////CredentialCache.DefaultNetworkCredentials);

            //// req.Credentials = cr;
            // //string credentials = Convert.ToBase64String(
            // //       //Encoding.ASCII.GetBytes("srvruofpo" + ":" + "zaq22?mk"));
            // //       Encoding.ASCII.GetBytes("ERIC722" + ":" + "23232323"));
            // //req.Headers[HttpRequestHeader.Authorization] = string.Format(
            // //    "Basic {0}", credentials);
            // req.UseDefaultCredentials = true;
            // req.AllowAutoRedirect = true;
            // req.Credentials = credCache;
            // req.CookieContainer = new CookieContainer();
            //// req.Credentials = CredentialCache.DefaultCredentials;
            // req.Method = "POST";
            // req.Headers.Add("keystring", "59881203878321920");
            // // Get the response.
            // //req.AllowAutoRedirect = true;
            // //req.CookieContainer = new CookieContainer();
            // using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
            // {
            //     StreamReader sr = new StreamReader(res.GetResponseStream());
            //     Console.WriteLine(sr.ReadToEnd());
            // }


            // List<persona> L = new List<persona>();
            // persona p = new persona() { cognome = "rossi", nome = "paolo" };
            // L.Add(p);

            // persona p1 = new persona() { cognome = "rossi", nome = "gino" };
            // L.Add(p1);
            // persona p2 = new persona() { cognome = "rossi", nome = "max" };
            // L.Add(p2);
            // persona p3 = new persona() { cognome = "rossi", nome = "roberto" };
            // L.Add(p3);

            // ParameterExpression pa=Expression.Parameter(typeof(bool),"x");

            // Expression<Func<persona, bool>> e1 = x => x.nome.StartsWith("g");
            // Expression<Func<persona, bool>> e2 = x => x.cognome.StartsWith("d");


            // int c = L.AsQueryable().Where(e1).Count();
            // ParameterExpression param = e1.Parameters[0];

            //     // simple version
            //  var total=     Expression.Lambda<Func<persona, bool>>(
            //         Expression.And(e1.Body, e2.Body), param);
            //  var total2= Expression.Lambda<Func<persona, bool>>(
            //      Expression.AndAlso(
            //         e1.Body,
            //              Expression.Invoke(e2, param)), param);
            //  int c2 = L.AsQueryable().Where(total).Count();

            // return;
            // //WSlocalhost.WSDigigapp s = new WSlocalhost.WSDigigapp();
            // //s.aggiornaEccezioni("", new WSlocalhost.Eccezione[]{ new WSlocalhost.Eccezione()},75);
            // //return;

            // //MyRaiServiceReference1.MyRaiService1Client c = new MyRaiService1Client();
            // //var rert= c.GetEccezioniComplessive();

            // //var resp= c.CambiaStato(2000555, MyRaiWindowsService1.ServiceReference1.EnumStatiRichiesta.InApprovazione,
            // //    "103650", "");


            // //persona cp = new persona();
            // //cp = (persona)populateObject(cp);
            // //persona p = new persona() { cognome = "rossi", nome = "paolo" };


            // //persona q = new persona() { cognome ="bianchi", nome="mario"};

            // //List<persona> L = new List<persona>();
            // //L.Add(p);
            // //L.Add(q);

            // //persona rr = L.First();
            // //rr.nome = "pepep";




            // //WSlocalhost.WSDigigapp serviceLocal = new WSlocalhost.WSDigigapp();
            // //WSlocalhost.Eccezione e = new WSlocalhost.Eccezione();
            // //e.qta="0.25";
            // //e.dalle="08:30";
            // //e.alle="10:15";
            // //e.cod="PRQ";
            // //e.data="25082017";
            // //e.matricola="0103650";
            // //e.ndoc="039251";
            // //e.importo = "";
            // //e.flg_storno = "*";
            // //e.tipo_eccezione = "C";
            // //e.stato_eccezione = "";
            // //serviceLocal.aggiornaEccezioni("103650", new WSlocalhost.Eccezione[] { e }, 75);




            //// CheckStatiD();
            // return;






            // //MyRaiService1Client cl = new MyRaiService1Client();

            // //var test = cl.GetDettagli(new string[] { "DDE30" }, DateTime.Now.AddDays(-19), DateTime.Now, EnumWorkflowRichieste.TuttiISettori, EnumStatiRichiesta.TuttiGliStati);

            // //Pdf prova = new Pdf();
            // //prova = cl.recuperaPdf("DDE30", Convert.ToDateTime("26/06/2017"));
            // //byte[] pdfr = prova.pdf_R;

            // ////Cambiastato
            // //CambiaStatoResponse respss = cl.CambiaStato(2000193, EnumStatiRichiesta.Approvata, "eric714", "cambia stato");





            // //var r5t = cl.recuperaUtente("103650", "22062017");

            // //var resp = cl.getDipendentiPeriodo(
            // //    "103650",
            // //    "DDE30",
            // //    Convert.ToDateTime("10/05/2017"),
            // //    Convert.ToDateTime("23/05/2017"));




            // ////GetDettagli
            // //DettaglioRichiesta[] ris = cl.GetDettagli(new string[] { "DDE30" },
            // //    Convert.ToDateTime("14/04/2017"),
            // //    Convert.ToDateTime("14/07/2017"),
            // //    EnumWorkflowRichieste.TuttiISettori,
            // //    EnumStatiRichiesta.InApprovazione);


            // ////GetRiepilogo
            // //Riepilogo p = cl.GetRiepilogo(new string[] { "DDE30" },
            // //   Convert.ToDateTime("14/04/2017"),
            // //    Convert.ToDateTime("14/07/2017"),
            // //   EnumWorkflowRichieste.TuttiISettori);

            // //return;



            // WebReference.firmaStandard ws = new WebReference.firmaStandard();
            // ws.UseDefaultCredentials = true;
            // ws.Credentials = System.Net.CredentialCache.DefaultCredentials;
            // //// ws.signDigest("p103650",
            // ////229686
            // ////  digiGapp2015

            // firmaRemota.remoteSignature r = new firmaRemota.remoteSignature();
            // firmaRemota.RemoteSignatureCredentials cred = new firmaRemota.RemoteSignatureCredentials();
            // cred.userid = "P103650";
            // ////cred.password = "digiGapp2015";
            // cred.password = "Password01";
            // cred.extAuth = "033193";


            // String sessionToken = r.openSignatureSession(cred);
            // firmaRemota.SignatureFlags d = new firmaRemota.SignatureFlags();

            // //d.graphicalSignature = false;

            // var response = r.signPDF(cred, System.IO.File.ReadAllBytes(@"C:\Users\eric722\Desktop\TEST.pdf"),
            //     false, "SHA256", null, d, "Vincenzo", 1, 10, 10, 200, 50, "P103650", "reason", "Roma", "ddMMyyyy", "Firmato da Vincenzo Bifano", 10);
            // System.IO.File.WriteAllBytes("D:\\test.pdf", response);

            // //var response2 = r.signPDF(cred, System.IO.File.ReadAllBytes(@"C:\Users\eric722\Desktop\TEST2.pdf"),
            // //    false, "SHA256",null, d, "Vincenzo", 1, 215, 764, 200, 15, "P103650", "reason", "Roma", "ddMMyyyy", "text", 20);
            // //System.IO.File.WriteAllBytes("D:\\test2.pdf", response2);

            // r.closeSignatureSession(cred, sessionToken);


            // //Cambiastato
            // //CambiaStatoResponse resp = cl.CambiaStato(2000193, EnumStatiRichiesta.InProgressSegreteria,"eric722","test nota");



            // //GetStorni
            // //string[] st = "038482,037649,038217,038213,038288,038289,038338,037650,038214,038219,038264,038215,038343,038344,038345,038346,037651,038216,038218,038265,038266,038279,038257,038252,038341,037652,038267,038258,038260,038261,038253,038254,038342,037653,038268,037654,037655,038269,038270,038272,038273,038274,038276,038349,038350,038351,038280,038281,038284,038287,038312,038313,038314,038317,038352,038271,038322,038283,038327,038316,038318,038319,038354,038321,038282,038323,038324,038325,038333,038326,038286,038328,038329,038332,038334,038335,038355,038356,038336,038337,038347,038348,038364,038357,038361,038263,038362,038363,038365,038366,038367,038359,038369,038370,038389,038376,038378,038394,038380,038371,038382,038383,038388,038368,038384,038387,038377,038379,038395,038421,038385,038386,038392,038393,038396,038403,038404,038405,038411,038415,038493,038494,038495,038418,038422,038406,038412,038496,038399,038423,038434,038435,038413,038416,038497,038400,038424,038436,038414,038417,038498,038419,038443,038433,038278,038437,038445,038446,038499,038447,038456,038457,038438,038472,038500,038473,038474,038477,038475,038514,038486,038505,038501,038476,038508,038510,038488,038487,038506,038502,038470,038511,038489,038490,038491".Split(',');

            // //DatiStorno[] respo= cl.GetStorni(st);



        }


    }

    public class TrackToClassSolver
    {
        public T GetClassFromTrack<T>(string track)
        {
            if (String.IsNullOrWhiteSpace(track)) return default(T);

            var Response = (T)Activator.CreateInstance(typeof(T));
            var Rawproperty = Response.GetType().GetProperty("Raw");
            if (Rawproperty != null) Rawproperty.SetValue(Response, track, null);

            try
            {
                return GetClass<T>(track, Response);
            }
            catch (Exception ex)
            {
                var ErrorProperty = Response.GetType().GetProperty("Error");
                if (ErrorProperty != null) ErrorProperty.SetValue(Response, ex.ToString(), null);
                return Response;
            }
        }

        private T GetClass<T>(string track, T Response)
        {
            
            PropertyInfo[] props = Response.GetType().GetProperties();

            int? MaxIndex = props.SelectMany(a => a.GetCustomAttributes(true).Select(x => (x as TrackToClassAttributes).Index)).Max();


            int CursorPosition = 0;
            for (int i = 0; i <= MaxIndex; i++)
            {
                foreach (PropertyInfo p in props)
                {
                    TrackToClassAttributes TrAtt = (TrackToClassAttributes)p.GetCustomAttributes(true).FirstOrDefault();
                    if (TrAtt == null || TrAtt.Index != i) continue;

                    if (p.PropertyType.IsArray || (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(List<>)))
                    {
                        Type t = p.PropertyType.GetGenericArguments()[0];// Type.GetType(p.PropertyType.FullName.Replace("[]", ""));
                        object child = Activator.CreateInstance(t);
                        var pr = child.GetType().GetProperties();

                        int totalLength = 0;
                        foreach (var pro in pr)
                        {
                            //pro.GetCustomAttributes(true).ForEach(at =>
                            //{
                            //    TrackToClassAttributes ta = at as TrackToClassAttributes;
                            //    if (ta != null)
                            //        totalLength += ta.Length;
                            //});
                        }

                        var listInstance = (IList)typeof(List<>)
                           .MakeGenericType(t)
                           .GetConstructor(Type.EmptyTypes)
                           .Invoke(null);

                        while (CursorPosition < track.Length)
                        {
                            MethodInfo method = this.GetType().GetMethod("GetClassFromTrack");
                            MethodInfo generic = method.MakeGenericMethod(t);
                            object re = generic.Invoke(this, new object[] { track.Substring(CursorPosition, totalLength) });
                            CursorPosition += totalLength;
                            listInstance.Add(re);
                        }
                        if (p.PropertyType.IsArray)
                        {
                            var arr = Array.CreateInstance(t, listInstance.Count);
                            for (int k = 0; k < listInstance.Count; k++)
                            {
                                arr.SetValue(listInstance[k], k);
                            }

                            p.SetValue(Response, arr, null);
                        }
                        else
                            p.SetValue(Response, listInstance, null);


                    }
                    else
                    {
                        List<TrackToClassAttributes> L = new List<TrackToClassAttributes>();
                        //p.GetCustomAttributes(true).ForEach(at => { L.Add(at as TrackToClassAttributes); });

                        String Segment = "";
                        if (TrAtt.AbsoluteStart != 0)
                        {
                            Segment = track.Substring(TrAtt.AbsoluteStart, TrAtt.Length);
                            CursorPosition = TrAtt.AbsoluteStart + TrAtt.Length;
                        }
                        else
                        {
                            Segment = track.Substring(CursorPosition, TrAtt.Length);
                            CursorPosition += TrAtt.Length;
                        }

                        if (p.PropertyType == typeof(DateTime))
                        {
                            TrackToClassAttributes dt = (TrackToClassAttributes)p.GetCustomAttributes(true).Where(x => (x as TrackToClassAttributes).DateFormat != null).FirstOrDefault();
                            DateTime D;
                            DateTime.TryParseExact(Segment, dt.DateFormat, null, DateTimeStyles.None, out D);
                            p.SetValue(Response, D, null);
                        }
                        else
                            p.SetValue(Response, Convert.ChangeType(Segment, p.PropertyType), null);
                        break;

                    }
                }
            }
            var EsitoProperty = Response.GetType().GetProperty("Esito");
            if (EsitoProperty != null) EsitoProperty.SetValue(Response, true, null);
            return Response;
        }
    }

    public class DecodificaTracciato
    {
        public string Error { get; set; }
        public Boolean Esito { get; set; }
        public string Raw { get; set; }
    }
    public class ClassePadreTest : DecodificaTracciato
    {
        [TrackToClassAttributes(Index = 0, Length = 8, AbsoluteStart = 24, DateFormat = "yyyyMMdd")]
        public DateTime data { get; set; }


        [TrackToClassAttributes(Index = 1, Length = 3)]
        public string eccezione { get; set; }


        [TrackToClassAttributes(Index = 2, Length = 4)]
        public string Dalle { get; set; }


        [TrackToClassAttributes(Index = 3, Length = 4)]
        public string Alle { get; set; }


        [TrackToClassAttributes(Index = 4, Length = 4)]
        public int Quantita { get; set; }


        [TrackToClassAttributes(Index = 5)]
        public List<ClasseFigliaTest> Figlie { get; set; }

    }
    public class ClasseFigliaTest
    {
        [TrackToClassAttributes(Index = 0, Length = 6)]
        public string Matricola { get; set; }


        [TrackToClassAttributes(Index = 1, Length = 5)]
        public string Sede { get; set; }
    }

    public class TrackToClassAttributes : Attribute
    {
        public int Index { get; set; }
        public int Length { get; set; }
        public int AbsoluteStart { get; set; }
        public string DateFormat { get; set; }

    }

}