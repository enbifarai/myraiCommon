using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MyRaiWindowsService1
{
    public class Logger
    {
        public static void Log(string message)
        {

            try
            {
                var location = System.Reflection.Assembly.GetEntryAssembly().Location;
                var directoryPath = Path.GetDirectoryName(location);
                var logPath = System.IO.Path.Combine(directoryPath, "log");
                if (!System.IO.Directory.Exists(logPath)) System.IO.Directory.CreateDirectory(logPath);

                string filelog = System.IO.Path.Combine(logPath, "log_" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
                System.IO.File.AppendAllText(filelog, DateTime.Now.ToString("HH:mm:ss ") + message + "\r\n");
            }
            catch (Exception ex)
            {

            }
        }
    }
}
