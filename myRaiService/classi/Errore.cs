using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiService
{
    public class Errore
    {

        public string codice { get; set; }
        public string descrizione { get; set; }

        public Errore()
        {

        }

        public Errore(string codice)
        {
            this.codice = codice;
            this.descrizione = getDescrizioneErrore(codice);
        }

        private string getDescrizioneErrore(string codice)
        {
            myRaiData.digiGappEntities context = new myRaiData.digiGappEntities();

            myRaiData. DIGIGAPP_ERROR_MESSAGE errore;

            try
            {
                errore = context.DIGIGAPP_ERROR_MESSAGE.Where(f => f.Codice == codice).FirstOrDefault();
                return errore.Descrizione;
            }
            catch (Exception e)
            {
                return "Descrizione errore (codice :" + codice + ") non trovata";
            }
        }

    }
}