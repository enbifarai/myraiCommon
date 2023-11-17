using System;

namespace myRaiCommonModel
{
    public class Persona
    {
        public string Surname { get; set; }

        public string Name { get; set; }

        public string BirthDate { get; set; }

        public string FiscalCode { get; set; } /*
                                                Codice fiscale della persona.
                                                Questo campo, come RegId può essere usato come chiave primaria: se un’anagrafica con lo stesso codice fiscale è già presente nel DB, non viene
                                                effettuato l’inserimento della stessa ma l’update.
                                               */

        public string RegId { get; set; } /* 
                                            Matricola della persona (utile solo in caso di importazione di dipendenti – Classe = D).
                                            Questo campo, come FiscalCode può essere usato come chiave primaria: se un’anagrafica con la stessa matricola è già presente nel DB, 
                                            non viene effettuato l’inserimento della stessa ma l’update. Almeno uno dei due campi FiscalCode e RegId è obbigatorio
                                          */

        public DateTime HiringDate { get; set; } /* 
                                                    Data di assunzione. Se l’anagrafica è già presente nel DB, la sua data di
                                                    assunzione viene aggiornata.Attenzione!!! Gli eventuali badge legati a questa
                                                    anagrafica dovranno avere una data di inizio posteriore alla data di assunzione.
                                                  */

        public DateTime? DismissalDate { get; set; } = null; // Data di licenziamento. Se l’anagrafica è già presente nel DB, la sua data di licenziamento viene aggiornata   

        public string Email { get; set; }

        public string ClassCode { get; set; } // Codice della classe anagrafica da importare. Valori ammessi: T_INDETER, T_DETER, COLLAB, CONSIGL, INTERIN, STAGE, COLLAB_ART, AGENTE, CONSUL_INF

        public string CompanyCode { get; set; } // Codice dell’azienda dell’anagrafica. Valori ammessi: RAI, RAIWAY, RAICINEMA, RAICOM, RAIPUBBLICITA

        public string classe_vede_orario { get; set; } = "SI"; // flag che descrive la macro classe o gruppo a cui appartiene il possessore del badge.Valori ammessi: SI, NO

        public string sede_contabile { get; set; } = "0000";  // Valore che definisce la sede contabile o la sede di riferimento dell’anagrafica

        public int contatore_badge { get; set; } = 1; // Valore che definisce il contatore attuale per i badge
    }
}
