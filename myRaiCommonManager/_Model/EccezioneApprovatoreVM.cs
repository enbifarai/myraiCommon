using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel
{
	public class EccezioneApprovatoreVM
	{
        public EccezioneApprovatoreVM()
        {
            string datachiusura = UtenteHelper.GetDateBackPerEvidenze();
            DateTime DC;
            Boolean conv = DateTime.TryParseExact(datachiusura, "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out DC);
            this.DataChiusura = DC;
        }
		public MyRaiServiceInterface.it.rai.servizi.digigappws.Eccezione Eccezione { get; set; }

		public bool Visualizzato { get; set; }

        public string Visualizzatore { get; set; }

        public int POH { get; set; }

		public int ROH { get; set; }

        public myRaiData.MyRai_AttivitaCeiton attivitaCeiton { get; set; }

        public bool RichiedeVisto { get; set; }

        public bool VistatoPositivo { get; set; }
        public bool VistatoNegativo { get; set; }

        public DateTime DataChiusura { get; set; }
	}
}