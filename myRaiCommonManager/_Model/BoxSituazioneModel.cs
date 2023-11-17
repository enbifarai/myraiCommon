using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel
{
    public class BoxSituazioneModel
    {
        public string Titolo { get; set; }
        public string CifraPrincipale { get; set; }
        public string AllaData { get; set; }
        public string TestoPulsante { get; set; }
        public string HrefPulsante { get; set; }
        public string ClasseIcona { get; set; }
        public string ColoreIcona { get; set; }
        public bool Visibile { get; set; }
        /// <summary>
        /// Alternativo a CifraPrincipale
        /// se valorizzato verrà stampato al posto di
        /// CifraPrincipale.
        /// </summary>
        public string MessaggioPrincipale { get; set; }
    }
}