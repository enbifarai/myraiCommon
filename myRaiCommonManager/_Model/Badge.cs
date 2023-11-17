using System;

namespace myRaiCommonModel
{
    public class Badge
    {
        public string Code { get; set; }

        public DateTime StartDate { get; set; } /*
                                                    Data di inizio validità del badge. Attenzione!!! Questa data deve essere posteriore
                                                    alla data di assunzione dell’anagrafica a cui il badge è associato
                                                */

        public DateTime? EndDate { get; set; } // Data di fine validità del badge. Se uguale a null il badge non ha una data di fine

        public string IdPolicy { get; set; } = "-999999"; /*
                                                            Id delle policy di accesso del badge. (il valore di default “-999999” corrisponde alla
                                                            policy vuota). Il valore può contenere più di un idpolicy in formato comma separated.
                                                          */

        public string Notes { get; set; }

        public string FiscalCode { get; set; }

        public string RegId { get; set; }
    }
}
