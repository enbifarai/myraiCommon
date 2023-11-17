using MyRaiServiceInterface.it.rai.servizi.digigappws;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myRaiServiceTestClient
{
    public class ParallelTest
    {
        public void Call()
        {
            List<DayTest> LT = new List<DayTest>();
            MyRaiServiceInterface.it.rai.servizi.digigappws.WSDigigapp service = new WSDigigapp();
            service.Credentials = new System.Net.NetworkCredential("srvruofpo", "zaq22?mk");

            for (int i = 0; i < 1; i++)
            {
                LT.Add(new DayTest() { data = DateTime.Today.AddDays(i) });
            }

            Parallel.ForEach(LT, new ParallelOptions { MaxDegreeOfParallelism = 10 },
                     daytest =>
                     {
                         daytest.response = service.getEccezioni("103650", daytest.data.ToString("ddMMyyyy"), "BU", 80);
                     });

        }
    }
    public class DayTest
    {
        public DateTime data { get; set; }
        public dayResponse response { get; set; }
    }
}
