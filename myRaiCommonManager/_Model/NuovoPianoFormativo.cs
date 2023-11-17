using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiCommonModel
{
    public class NuovoPianoFormativo
    {
        public bool Modifica { get; set; }
        public List<StudioModel> Titoli { get; set; }
        public List<EsperienzeLavorativeViewModel> Esperienze { get; set; }
        public List<TutorPianoFormativoVM> Tutor { get; set; }
        public DatiApprendistato DatiApprendistato { get; set; }
    
    }
}
