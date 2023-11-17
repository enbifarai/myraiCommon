using System.Collections.Generic;
using myRaiData;
using myRai.Data.CurriculumVitae;

namespace myRaiCommonModel
{
    public class MPGStanza
    {
        public TSVPrenStanza PrenStanza { get; set; }
        public List<TSVPrenSlot> PrenSlots { get; set; }
        public List<MPGDipendente> PrenElencoDips { get; set; }

    }

    public class MPGDipendente
    {
        public TSVPrenElencoDip PrenDip { get; set; }
        public myRai.Data.CurriculumVitae.TDipendenti Dip { get; set; }
        public myRai.Data.CurriculumVitae.DSedeContabile SedeContabile { get; set; }
        public DCategoria  Categoria { get; set; }
        public TSVIntervScheda Intervista { get; set; }
        public TSVPrenPrenota Prenotazione { get; set; }
        public DServizio Servizio { get; set; }
    }
    
    public class MPGRicerca
    {
        
    }

    public class MPGGruppo
    {
        public string Codice { get; set; }
        public string CodSedeCont { get; set; }
        public List<MPGDipendente> AvailableDip { get; set; }
        public List<MPGStanza> Stanze { get; set; }
        public List<MPGDipendente> PrenDip{ get; set; }
    }

    public class MyExcelSheet
    {
        public MyExcelSheet() { Lrows = new List<CellRow>(); }
        public string Nome { get; set; }
        public List<CellRow> Lrows { get; set; }
    }
    public class CellRow
    {
        public CellRow() { Lvalori = new List<string>(); }
        public List<string> Lvalori { get; set; }
    }
}
