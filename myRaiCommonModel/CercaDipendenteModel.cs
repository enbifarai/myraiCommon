using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiCommonModel
{
    public class CercaDipendenteModel
    {
        public string NominativoDipendente { get; set; }

        public string Matricola { get; set; }

        public string Query { get; set; }
        public string[] Azienda { get; set; }
        public string[] Categoria { get; set; }
        public int NascitaDa { get; set; }
        public int NascitaA { get; set; }
        public string[] Sesso { get; set; }
        public string CodiceFiscale { get; set; }
        public int AssunzioneDa { get; set; }
        public int AssunzioneA { get; set; }
        public int CessazioneDa { get; set; }
        public int CessazioneA { get; set; }
        public string[] TipiContratto { get; set; }
        public string[] Sedi { get; set; }
        public string[] Servizi { get; set; }
        public string[] Sezioni { get; set; }

        public string Provenienza { get; set; }

        public string Action { get; set; }

        public string ActionText { get; set; }
    }

    public class CercaDipendenteVM
    {
        public CercaDipendenteModel Filtri { get; set; }
        
        public List<CercaDipendentiItem> CercaDipendentiResult { get; set; }

        public string Provenienza { get; set; }

        public string Action { get; set; }

        public string ActionText { get; set; }
    }

    public class L2D_CATEGORIA
    {
        public string cod_categoria { get; set; }
        public string desc_categoria { get; set; }
        public string Aggregato_CCL { get; set; }
        public string CCL { get; set; }
        public string Qualifica_CCL { get; set; }
    }

    public class CercaDipendentiItem
    {
        public int ID_PERSONA { get; set; }
        public string MATRICOLA { get; set; }
        public string NOME { get; set; }
        public string COGNOME { get; set; }
        public string SECONDO_COGNOME { get; set; }
        public string NOMINATIVO { get { return (this.COGNOME.Trim() + " " + (!String.IsNullOrWhiteSpace(this.SECONDO_COGNOME) ? this.SECONDO_COGNOME + " " : "").Trim() + this.NOME.Trim()); } set { } }
        public DateTime? DATA_ASSUNZIONE { get; set; }
        public string CONTRATTO { get; set; }
        public string SEDE { get { return CezanneHelper.GetDes(this.COD_SEDE, this.DES_SEDE); } set { } }
        public string COD_SEDE { get; set; }
        public string DES_SEDE { get; set; }
        public string SERVIZIO { get { return CezanneHelper.GetDes(this.COD_SERVIZIO, this.DES_SERVIZIO); } set { } }
        public string COD_SERVIZIO { get; set; }
        public string DES_SERVIZIO { get; set; }
        public bool SmartWorker { get; set; }
        public string CF { get; set; }
    }
}

