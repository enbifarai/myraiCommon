using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace myRaiCommonModel
{
    public class EsperienzeLavorativeViewModel : _IdentityData
    {
        public EsperienzeLavorativeViewModel()
        {
            DataInizio = new DataModel();
            DataFine = new DataModel();
        }
      
        [MaxLength(250)]
        public string Attivita { get; set; }
        [MaxLength(250)]
        public string Azienda { get; set; }
        public DataModel DataInizio { get; set; }
        public DataModel DataFine { get; set; }
        public string DataInizioStrEL { get; set; }
        public string DataFineStrEL { get; set; }
        public bool Apprendistato { get; set; }
        public string CodiceCitta { get; set; }
        public string DescrizioneCitta { get; set; }
        public string idEspLavLocal { get; set; }


    }
}
