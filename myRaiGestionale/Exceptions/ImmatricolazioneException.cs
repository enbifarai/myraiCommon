using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiGestionale.Exceptions
{
    public class ImmatricolazioneException : Exception
    {
        public const string COD_FISC_ESISTENTE = "Codice Fiscale esistente";

        public const string COD_FISC_ERROR = "Codice Fiscale non corretto";

        public const string OMOCODIA = "Conferma che si tratta di omocodia?";

        public ImmatricolazioneException(string message) : base(message)
        {
        }
    }

}