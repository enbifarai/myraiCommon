using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiCommonModel
{
    public class CercaDipendenteModel
    {
        public CercaDipendenteModel()
        {
            EscludiCessati = true;
        }
        public string NominativoDipendente { get; set; }

        public string Matricola { get; set; }

        public string Query { get; set; }
        public string[] Azienda { get; set; }
        public string[] Categoria { get; set; }
        public string NascitaDa { get; set; }
        public string NascitaA { get; set; }
        public string[] Sesso { get; set; }
        public string CodiceFiscale { get; set; }
        public DateTime? AssunzioneDa { get; set; }
        public DateTime? AssunzioneA { get; set; }
        public DateTime? CessazioneDa { get; set; }
        public DateTime? CessazioneA { get; set; }
        public string[] TipiContratto { get; set; }
        public string[] Sedi { get; set; }
        public string[] Servizi { get; set; }
        public string[] Sezioni { get; set; }
        public bool EscludiCessati { get; set; }
        public string Codice { get; set; }
        public string TipoAccordo { get; set; }

        public string Provenienza { get; set; }

        public string Action { get; set; }
        public string[] Dinamiche { get; set; }
        public string ActionText { get; set; }
        public List<HrisInfoAggiuntive> FiltriAggiuntivi { get; set; }
        public List<RicercaDinamicaItem> RicercaDinamicaItems { get; set; }
    }
    public class RicercaDinamicaItem
    {
        public string ItemText { get; set; }
        public string ItemCode { get; set; }
    }
    public class CercaDipendenteVM
    {
        public CercaDipendenteModel Filtri { get; set; }

        public List<CercaDipendentiItem> CercaDipendentiResult { get; set; }

        public string Provenienza { get; set; }

        public string Action { get; set; }

        public string ActionText { get; set; }

        public string[] CampidaEsportare { get; set; }
        
        public string NomeRicerca { get; set; }

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
        public DateTime? DATA_CESSAZIONE { get; set; }
        public string CONTRATTO { get; set; }
        public string SEDE { get { return CezanneHelper.GetDes(this.COD_SEDE, this.DES_SEDE); } set { } }
        public string COD_SEDE { get; set; }
        public string DES_SEDE { get; set; }
        public string SERVIZIO { get { return CezanneHelper.GetDes(this.COD_SERVIZIO, this.DES_SERVIZIO); } set { } }
        public string COD_SERVIZIO { get; set; }
        public string DES_SERVIZIO { get; set; }
        public string COD_UNITAORG { get; set; }
        public string DES_UNITAORG { get; set; }
        public string SEZIONE { get { return CezanneHelper.GetDes(this.COD_UNITAORG, this.DES_UNITAORG); } set { } }
        public bool SmartWorker { get; set; }
        public string CF { get; set; }
        public string SESSO { get; set; }
        public int AnnoNascita { get; set; }
        public string AZIENDA { get; set; }
        public string CATEGORIA { get; set; }
        public bool IsNoDip { get; set; }
        public List<HrisInfoAggiuntive> FiltriAggiuntivi { get; set; }
    }

    public class ParametriEsportazione
    {
        public ParametriEsportazione()
        {
            var abilHra = AuthHelper.EnabledSubFunc(CommonHelper.GetCurrentUserMatricola(), "HRA");
            if (abilHra.Contains("F02"))
            {
                CanExpSede = true;
                CanExpServizio = true;
                CanExpQualifica = true;
                CanExpRuolo = true;
                CanExpStruttura = true;
            }
            if (abilHra.Contains("F08"))
                CanExpStato = true;
        }

        public bool Matricola { get; set; }
        public bool Nominativo { get; set; }

        public bool CanExpSede { get; set; }
        public bool CanExpServizio { get; set; }
        public bool CanExpQualifica { get; set; }
        public bool CanExpRuolo { get; set; }
        public bool CanExpStruttura { get; set; }
        public bool CanExpStato { get; set; }

        public bool Sede { get; set; }
        public bool Servizio { get; set; }
        public bool Qualifica { get; set; }
        public bool Ruolo { get; set; }
        public bool Struttura { get; set; }
        public bool Stato { get; set; }
    }
    public class ExportDipendenti
    {
        public string CodCampo { get; set; }
        public string DesCampo { get; set; }
        public bool CheckDefault { get; set; }
    }
    public class HrisInfoAggiuntive
    {
      public string Valore { get; set; }
      public string  Descrizione { get; set; }
      public string  Matricola { get; set; }
      public string  NomeFiltro { get; set; }
       
    }
}

