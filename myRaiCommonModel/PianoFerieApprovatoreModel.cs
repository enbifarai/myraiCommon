using myRaiHelper;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using System;
using System.Collections.Generic;
using System.Linq;

namespace myRaiCommonModel
{
    public class PianoFerieApprovatoreModel
    {
        public PianoFerieApprovatoreModel()
        {
            PianoFerieDipendenti = new List<GiorniPianoFeriePerDip>();
        }
        public pianoFerieSedeGapp response { get; set; }
        public List<Sede> Sedi { get; set; }

        public string SedeSelezionataCodice { get; set; }
        public string SedeSelezionataDesc { get; set; }
        public string SedeSelezionataRepartoDescrittiva { get; set; }

        public DateTime? SedeSelezionataApprovataData { get; set; }
        public DateTime? SedeSelezionataFirmataData { get; set; }

        public List<GiorniPianoFeriePerDip> PianoFerieDipendenti { get; set; }

        public List<SedeRepartiForSelect> SediRepartiList { get; set; }
        public List<string> EsentatiPianoFerie { get; set; }

        public bool AlmenoUnUtenteRichiede2021 { get; set; }

        public List<string> MatricoleCheRichiedonoAnnoSuccessivo { get; set; }
    }

    public class SedeRepartiForSelect
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class GiorniPianoFeriePerDip
    {
        public GiorniPianoFeriePerDip()
        {
            Gapp_FE = new List<DateTime>();
            Gapp_PF = new List<DateTime>();
            Gapp_PR = new List<DateTime>();
        }
        private string _mat;
        public string Matricola
        {
            get { return _mat; }
            set
            {
                _mat = value;
                var db = new myRaiData.digiGappEntities();
                MyPianoferieDB= db.MyRai_PianoFerie.Where(x => x.matricola == _mat && x.anno == DateTime.Now.Year).FirstOrDefault();
                if (MyPianoferieDB != null && MyPianoferieDB.Id_pdf_pianoferie_inclusa != null)
                {
                    MyPianoferieSediDB_relativo = db.MyRai_PianoFerieSedi.Where(x => x.id == MyPianoferieDB.Id_pdf_pianoferie_inclusa).FirstOrDefault();
                } 
            }
        }
        public List<DateTime> GiorniFerie { get; set; }
        public List<DateTime> GiorniRR { get; set; }
        public List<DateTime> GiorniRN { get; set; }
        public List<DateTime> GiorniPR { get; set; }
        public List<DateTime> GiorniPF { get; set; }
        public List<DateTime> GiorniPX { get; set; }

        public List<DateTime> GiorniConNote { get; set; }

        public string NotaSuPianoFerieDaResp { get; set; }

        public List<DateTime> Gapp_FE { get; set; }
        public List<DateTime> Gapp_PF { get; set; }
        public List<DateTime> Gapp_PR { get; set; }

        public DateTime? DataApprovazione { get; set; }

        public bool NessunInvioPianoFerie { get; set; }
        
        public bool NonInviato { get; set; }
        public myRaiData.MyRai_PianoFerie MyPianoferieDB { get; set; }
        public myRaiData.MyRai_PianoFerieSedi MyPianoferieSediDB_relativo { get; set; }
    }
}