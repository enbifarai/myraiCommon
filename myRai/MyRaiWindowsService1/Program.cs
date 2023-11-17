using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using MyRaiWindowsService1.it.rai.servizi.svildigigappws;

namespace MyRaiWindowsService1
{
    static class Program
    {
        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        static void Main(string[] args)
        {
           

            if (Environment.UserInteractive)
            {

               
                //CheckMailDocDaFirmare();
                //return;

                //Notifiche.GetNotificheMensiliDipendente();
                //return;
                MyRaiWindowsService1 s = new MyRaiWindowsService1();
                s.MyStart();

                System.Threading.Thread.Sleep(99999999);
                return;


                if (args.Length == 1 && args[0].ToLower()=="startservice")
                {
                    s.MyStart();
                    System.Threading.Thread.Sleep(99999999);
                    return;
                }

                if (args.Length == 3 && args[0].ToLower() == "startup")
                {
                    string sede = args[1];
                    string optStampa = args[2];
                    if (sede != "*")
                    {
                        if (!s.GetSediGappPDF().Contains(sede.ToUpper()))
                        {
                            Console.WriteLine("Sede non abilitata : " + sede);
                            return;
                        }
                        s.StartupSede(sede, optStampa);
                    }
                    else
                    {
                        List<string> sedi = s.GetSediGappPDF();
                        foreach (string sed in sedi)
                        {
                            s.StartupSede(sed, optStampa);
                        }

                    }
                    return;
                }

                if (args.Length == 5 && args[0].ToLower()=="pdf" )
                {
                    DateTime d1;
                    bool b = DateTime.TryParseExact(args[1], "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out d1);
                    if (!b)
                    {
                        Console.WriteLine("Bad date : " + args[1]);
                        return;
                    }
                    DateTime d2;
                    bool b2 = DateTime.TryParseExact(args[2], "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out d2);
                    if (!b2)
                    {
                        Console.WriteLine("Bad date : " + args[2]);
                        return;
                    }
                    if (args[3] != "*" && !s.GetSediGappPDF().Contains(args[3].ToUpper()))
                    {
                        Console.WriteLine("Sede non abilitata : " + args[3]);
                        return;
                    }
                    if (args[3] != "*")
                    {
                        Console.WriteLine("\r\n\r\nProcessing " + args[3] + " " + args[1] + " " + args[2]);
                        int pdf = s.ProcessaSedeGapp(args[3], d1, d2, args[4]);
                        Console.WriteLine("Restituito " + pdf);
                    }
                    else
                    {
                        foreach (string sede in s.GetSediGappPDF())
                        {
                            Console.WriteLine("\r\n\r\nProcessing " + sede + " " + args[1] + " " + args[2]);
                            int pdf = s.ProcessaSedeGapp(sede, d1, d2, args[4]);
                            Console.WriteLine("Restituito " + pdf);
                        }
                    }
                    return;
                }

                Console.WriteLine("Argomento non valido.\r\n\r\n");
                Console.WriteLine("myRaiWindowsService1.exe startservice \r\n(Esegue il servizio in console)\r\n");
                Console.WriteLine("myRaiWindowsService1.exe startup ro|ss|sc sedegapp|* \r\n(Startup sede) \r\n");
                Console.WriteLine("myRaiWindowsService1.exe pdf ddmmyyyy ddmmyyyy  sedegapp|*  ro|sc|ss \r\n(Stampa pdf per il periodo)\r\n");
                Console.WriteLine("Nota:   ro: readonly (stampa pdf) - sc:stampa e convalida - ss:stampa soltanto");
               

               
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
            { 
                new MyRaiWindowsService1() 
            };
                ServiceBase.Run(ServicesToRun);
            }
        }

       
    }
}
