using myRaiData.Incentivi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplo
{
    public static class PoliticheRetributive
    {
        private static void ScriviFile(string msg, string nomeFile = "")
        {
            if (!String.IsNullOrEmpty(msg))
            {
                string filelog1 = "";
                var location = System.Reflection.Assembly.GetEntryAssembly().Location;

                var directoryPath = Path.GetDirectoryName(location);
                var logPath = Path.Combine(directoryPath, nomeFile);
                if (!Directory.Exists(logPath))
                    Directory.CreateDirectory(logPath);

                filelog1 = Path.Combine(logPath, "ConsoleLog_" + DateTime.Now.ToString("yyyyMMdd") + ".txt");

                System.IO.File.AppendAllText(filelog1, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " " + msg + "\r\n");
            }
        }


        public static void AssociaTemplate()
        {
            Console.WriteLine("Avvio AssociaTemplate");
            ScriviFile("Avvio AssociaTemplate");
            string line = "";
            int i = 0;
            int aggiornato = 0;
            int nontrovati = 0;
            // carica tutte le campagne 2022
            List<int> campagna = new List<int>();
            campagna.Add(113524410);
            campagna.Add(465852180);
            campagna.Add(997716921);
            campagna.Add(1396886732);
            IncentiviEntities db = new IncentiviEntities();

            Console.WriteLine("Lettura file");

            try
            {
                StreamReader file = new StreamReader(@"C:\RAI\CaricamentoTemplateProdTv.txt");

                while ((line = file.ReadLine()) != null)
                {
                    i++;
                    Console.WriteLine("Riga " + i);
                    string riga = line;
                    List<string> rigaSplittata = riga.Split(',').ToList();
                    string matricola = rigaSplittata[0].Trim();
                    string template = rigaSplittata[1];

                    if (String.IsNullOrEmpty(template))
                    {
                        ScriviFile($"nessun template per la matricola {matricola}");
                        continue;
                    }

                    template = template.Trim();
                    int myTemplate = int.Parse(template);

                    var item = db.XR_PRV_DIPENDENTI.Where(w => w.ID_CAMPAGNA != null &&
                                                        campagna.Contains(w.ID_CAMPAGNA.Value) &&
                                                        w.MATRICOLA == matricola &&
                                                        w.ID_TEMPLATE == null).FirstOrDefault();

                    if (item != null)
                    {
                        aggiornato++;
                        item.ID_TEMPLATE = myTemplate;
                        ScriviFile($"Aggiornamento del record {item.ID_DIPENDENTE} template utilizzato {myTemplate}");
                    }
                    else
                    {
                        nontrovati++;
                        ScriviFile($"record non trovato per la matricola {matricola} template utilizzato {myTemplate}");
                    }
                }
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore, " + ex.ToString());
                ScriviFile("Errore, " + ex.ToString());
            }
            Console.WriteLine("Aggiornati " + aggiornato);
            Console.WriteLine("Non trovati " + nontrovati);
            Console.WriteLine("Terminato, premi un tasto");
            ScriviFile("Fine UpdateInfoDipendente");
        }

        public static void VerificaAssociativaTemplate()
        {
            Console.WriteLine("Avvio VerificaAssociativaTemplate");
            ScriviFile("Avvio VerificaAssociativaTemplate");
            string line = "";
            int i = 0;
            int trovato = 0;
            int nontrovati = 0;
            // carica tutte le campagne 2022
            List<int> campagna = new List<int>();
            campagna.Add(113524410);
            campagna.Add(465852180);
            campagna.Add(997716921);
            campagna.Add(1396886732);
            IncentiviEntities db = new IncentiviEntities();

            Console.WriteLine("Lettura file");

            try
            {
                StreamReader file = new StreamReader(@"C:\RAI\CaricamentoTemplateProdTv.txt");

                while ((line = file.ReadLine()) != null)
                {
                    i++;
                    Console.WriteLine("Riga " + i);
                    string riga = line;
                    List<string> rigaSplittata = riga.Split(',').ToList();
                    string matricola = rigaSplittata[0].Trim();
                    string template = rigaSplittata[1];

                    if (String.IsNullOrEmpty(template))
                    {
                        ScriviFile($"nessun template per la matricola {matricola}");
                        continue;
                    }

                    template = template.Trim();
                    int myTemplate = int.Parse(template);

                    var item = db.XR_PRV_DIPENDENTI.Where(w => w.ID_CAMPAGNA != null &&
                                                        campagna.Contains(w.ID_CAMPAGNA.Value) &&
                                                        w.MATRICOLA == matricola &&
                                                        w.ID_TEMPLATE != null &&
                                                        w.ID_TEMPLATE == myTemplate).FirstOrDefault();

                    if (item != null)
                    {
                        trovato++;                        
                        ScriviFile($"{matricola},{myTemplate},{item.ID_DIPENDENTE}");
                    }
                    else
                    {
                        nontrovati++;
                        ScriviFile($"{matricola},{myTemplate} non trovato");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore, " + ex.ToString());
                ScriviFile("Errore, " + ex.ToString());
            }
            Console.WriteLine($"Cercati {i} elementi");
            Console.WriteLine($"Trovati {trovato} elementi");
            Console.WriteLine($"Mancanti {nontrovati} elementi");
            Console.WriteLine("Terminato, premi un tasto");
            Console.ReadLine();
            ScriviFile("Fine VerificaAssociativaTemplate");
        }
    }
}
