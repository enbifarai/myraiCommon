using myRaiCommonModel.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiCommonModel
{
    public class RicercaPianoFormativo : _IdentityData
    {

        public string Nome { get; set; }
        public string Cognome { get; set; }

        public DateTime? DataInizioApprendistato { get; set; }
        public DateTime? DataFineApprendistato { get; set; }
        public DateTime? DataInizioContratto { get; set; }
        public DateTime? DataImmatricolazione { get; set; }
        public string DescrizioneServizio { get; set; }

        public string SelectedSezione { get; set; }
        public string SelectedTutor { get; set; }
        public string codSezione { get; set; }

        public bool IsChecked { get; set; }
        public string DescrizioneRuolo { get; set; }
        public int IdJobAssign { get; set; }
        public string TipoRuolo { get; set; }
        public string CodiceRequisito { get; set; }
        public bool IsRuoloAggreg { get; set; }
        public bool Cessato { get; set; }





        public static RicercaPianoFormativo ConverToImmatricolazioneVM(ImmatricolazioniVM entity)
        {

            return new RicercaPianoFormativo()
            {
                //IdEvento = entity.IdEvento,
                IdPersona = entity.IdPersona,
                Nome = entity.Nome,
                Cognome = entity.Cognome,
                DataInizioApprendistato = entity.DataCreazione,
                DataFineApprendistato = entity.DataFine.Value,
                Matricola = entity.Matricola,

            };
        }

    }
    public class ElencoPianiFormativi
    {
        public string tab { get; set; }
        public int totDaPianificare { get; set; }
        public int totPianificati { get; set; }
        public PaginatedList<RicercaPianoFormativo> daPianificare { get; set; }
        public PaginatedList<RicercaPianoFormativo> pianificati { get; set; }
    }
}
