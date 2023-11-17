using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace MyRaiWindowsNotificheServices
{
	static class Program
	{
		/// <summary>
		/// Punto di ingresso principale dell'applicazione.
		/// </summary>
		static void Main ( string[] args )
		{
			if ( Environment.UserInteractive )
			{
				MyRaiWindowsNotificheServices s = new MyRaiWindowsNotificheServices();
				s.MyStart();

				System.Threading.Thread.Sleep( 99999999 );
				return;
			}
			else
			{
				ServiceBase[] ServicesToRun;
				ServicesToRun = new ServiceBase[] 
				{ 
					new MyRaiWindowsNotificheServices() 
				};
				ServiceBase.Run( ServicesToRun );
			}
		}
	}
}