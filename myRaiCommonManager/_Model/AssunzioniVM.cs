using System;
using System.Collections.Generic;
using System.Linq;
using myRaiData.Incentivi;
using System.Web.Mvc;
using myRaiHelper;
using System.ComponentModel.DataAnnotations;

namespace myRaiCommonModel
{
    public class AssunzioniVM
    {
        public AssunzioniVM()
        {
            this.ISTAT = new List<SelectListItem>();
        }
        public int TabAttivo { get; set; }
        public int IdPraticaDematerializzazione { get; set; }
        public bool IsForPianoFormativo { get; set; }
        public string descrizioneSezione { get; set; }
        public string Cittadinanza { get; set; }
        public string codSezione { get; set; }
        public string Provincia { get; set; }
        public int IdEvento { get; set; }
        public int IdPersona { get; set; }
        public int IdFileContratto { get; set; }
        public string Nome { get; set; }
        public string AssicurazioneInfortuni { get; set; }
        public string[] Indennita { get; set; }
        public Indennita[] ListIndennita { get; set; }
        public string Cognome { get; set; }
        public string Insediamento { get; set; }
        public string SecondoCognome { get; set; }
        public string Indirizzo { get; set; }
        public string CodAssunzione { get; set; }
        public int Avanzamento { get; set; }
        public DateTime DataNascita { get; set; }
        public DateTime? DataApprovazione { get; set; }
        public DateTime? DataFirma { get; set; }
        public DateTime? DataConclusionePratica { get; set; }
        public string OrarioSettimanaleContratto { get; set; }

        public string CodiceFiscale { get; set; }
        public char? Genere { get; set; }
        public string Note { get; set; }

        public string TipoOperazione { get; set; }
        public string Matricola { get; set; }
        public string MatricolaUtenteLoggato { get; set; }
        public string LuogoDiNascita { get; set; }
        public string CodiceISTAT { get; set; }
        public string Nazione { get; set; }
        public DateTime? DataFine { get; set; }
        public DateTime DataInizio { get; set; }
        public DateTime DataCreazione { get; set; }
        public DateTime? DataContrattoCreazione { get; set; }
        public bool esitoCics { get; set; }
        public bool esito { get; set; }
        public bool? esito_ripristino { get; set; }
        public string SelectedAzienda { get; set; }

        public string SelectedSede { get; set; }

        public string SelectedCategoria { get; set; }

        public string SelectedServizio { get; set; }
        public string SelectedSezione { get; set; }
        public int StatoImmatricolazione { get; set; }
        public string SelectedFormaContratto { get; set; }
        public string SelectedStatoCivile { get; set; }
        public string DescrizioneStatoCivile { get; set; }
        public string DescrizioneFormaContratto { get; set; }
        public string DescrizioneTipoAssunzione { get; set; }
        public string DataInizioFine { get; set; }
        public string SelectedCausaleAssunzione { get; set; }
        public string SelectedModalitaReclutamento { get; set; }
        public string SelectedTipoAssunzione { get; set; }
        public string RpLavorativoForCics { get; set; }
        public string SelectedMansione { get; set; }
        public string JsonAssunzioni { get; set; }
        public string SelectedTipoMinimo { get; set; }
        public bool CollocamentoObbligatorio { get; set; }
        public List<SelectListItem> ISTAT { get; set; }

        public string Azienda { get; set; }
        public string Sede { get; set; }
        public string Servizio { get; set; }
        public string Categoria { get; set; }
        public string RappLavoro { get; set; }
        public string Sezione { get; set; }
        public string Mansione { get; set; }
        public bool esistePianoFormativo { get; set; }
        public int InServizio { get; set; }
        public bool TabAnagrafica { get; set; }
        public bool TabDatiContrattuali { get; set; }
        public bool TabAllegati { get; set; }
        public bool Completo { get; set; }
        //TODO enumerazione tipo operazione.
        public bool VerificaDatiContrattuali()
        {
            bool esito = false;

            if (this.DataFine == null)
            {
                this.DataFine = new DateTime(9999, 12, 31);
                esito = true;
            }
            if (this.DataInizio != null && this.DataFine != null && this.SelectedTipoAssunzione != null && this.SelectedSede != null && this.SelectedCategoria != null && this.SelectedServizio != null && this.SelectedSezione != null && this.SelectedAzienda != null)
            {
                esito = true;
                return esito;
            }

            return esito;
        }
        public static AssunzioniVM ConverToImmatricolazioneVM(XR_IMM_IMMATRICOLAZIONI entity)
        {
            if (!string.IsNullOrWhiteSpace(entity.JSON_ASSUNZIONI))
            {
                AssunzioniVM model = Newtonsoft.Json.JsonConvert.DeserializeObject<AssunzioniVM>(entity.JSON_ASSUNZIONI);
                model.MatricolaUtenteLoggato = CommonHelper.GetCurrentUserMatricola();
                if (model.Avanzamento == 0)
                {
                    if (model.TabAnagrafica)
                    {
                        model.Avanzamento = 20;
                    }
                    if (model.TabDatiContrattuali)
                    {
                        model.Avanzamento = 40;
                    }
                    if (model.TabAllegati)
                    {
                        model.Avanzamento = 60;
                    }
                    if (model.Completo)
                    {
                        model.Avanzamento = 80;
                    }
                }

                //model.Avanzamento = 60;
                return model;
            }
            else
            {
                return new AssunzioniVM()
                {
                    IdEvento = entity.ID_EVENTO,
                    IdPersona = entity.ID_PERSONA.GetValueOrDefault(),
                    Nome = entity.DES_NOMEPERS,
                    Cognome = entity.DES_COGNOMEPERS,
                    SecondoCognome = entity.DES_SECCOGNOME,
                    CodiceISTAT = entity.COD_CITTA,
                    LuogoDiNascita = entity.COD_CITTA,
                    DataNascita = entity.DTA_NASCITAPERS,
                    DataInizio = entity.DTA_INIZIO,
                    DataFine = entity.DTA_FINE,
                    CodiceFiscale = entity.CSF_CFSPERSONA,
                    Matricola = entity.COD_MATDIP,
                    DataCreazione = entity.DTA_IMM_CREAZIONE,
                    esitoCics = entity.ESITO_CICS,
                    TipoOperazione = entity.TIPO_OPERAZIONE,
                    SelectedSezione = entity.COD_SEZIONE,
                    SelectedAzienda = entity.COD_IMPRESA,
                    SelectedCategoria = entity.COD_QUALIFICA,
                    SelectedTipoAssunzione = entity.COD_TIPORLAV,
                    SelectedFormaContratto = entity.COD_TIPORLAV,
                    SelectedSede = entity.COD_SEDE,
                    SelectedServizio = entity.COD_SERVIZIO,
                    Genere = Convert.ToChar(entity.COD_SESSO),
                    SelectedMansione = entity.COD_RUOLO,
                    MatricolaUtenteLoggato = CommonHelper.GetCurrentUserMatricola(),
                    Avanzamento = 0

                };
            }

        }
        public XR_IMM_IMMATRICOLAZIONI ConvertToXR_IMM_IMMATRICOlAZIONI()
        {
            XR_IMM_IMMATRICOLAZIONI imm = new XR_IMM_IMMATRICOLAZIONI()
            {
                ID_EVENTO = this.IdEvento,
                ID_PERSONA = this.IdPersona,
                COD_CITTA = this.LuogoDiNascita,
                COD_SESSO = this.Genere.ToString(),
                DES_NOMEPERS = this.Nome,
                DES_SECCOGNOME = this.SecondoCognome,
                DES_COGNOMEPERS = this.Cognome,
                CSF_CFSPERSONA = this.CodiceFiscale,
                COD_IMPRESA = this.SelectedAzienda,
                COD_SEDE = this.SelectedSede,
                COD_MATDIP = this.Matricola,
                COD_QUALIFICA = this.SelectedCategoria,
                COD_SERVIZIO = this.SelectedServizio,
                COD_SEZIONE = this.SelectedSezione,
                COD_TIPORLAV = this.SelectedTipoAssunzione,
                COD_TERMID = this.SelectedFormaContratto,
                TMS_TIMESTAMP = DateTime.Now,
                DTA_IMM_CREAZIONE = DateTime.Now,
                DTA_NASCITAPERS = this.DataNascita,
                DTA_INIZIO = this.DataInizio,
                DTA_FINE = (this.DataFine.HasValue ? this.DataFine.Value : new DateTime(2999, 12, 31)),
                TIPO_OPERAZIONE = this.TipoOperazione,
                COD_CITTAD = this.Cittadinanza,
                COD_RUOLO = this.SelectedMansione,
                JSON_ASSUNZIONI = Newtonsoft.Json.JsonConvert.SerializeObject(this)
            };
            return imm;
        }

        public XR_IMM_IMMATRICOLAZIONI ConvertToXR_IMM_IMMATRICOLAZIONEWithRelation()
        {
            var jnag = new JNAGPERS()
            {
                ID_PERSONA = this.IdPersona,
                IND_VALCFS = "Y", //NC - Da verificare
                COD_MATDIP = this.Matricola
            };
            var anagrafica = new ANAGPERS()
            {

                ID_PERSONA = this.IdPersona,
                DES_COGNOMEPERS = this.Cognome,
                COD_CITTA = this.LuogoDiNascita,
                COD_SESSO = this.Genere.ToString(),
                DES_NOMEPERS = this.Nome,
                DES_SECCOGNOME = this.SecondoCognome,
                CSF_CFSPERSONA = this.CodiceFiscale,
                DTA_NASCITAPERS = this.DataNascita,
                //COD_MATDIP = this.Matricola,


                JNAGPERS = jnag
            };

            var imm = new XR_IMM_IMMATRICOLAZIONI()
            {
                ID_EVENTO = this.IdEvento,
                ID_PERSONA = this.IdPersona,
                COD_CITTA = this.LuogoDiNascita,
                COD_SESSO = this.Genere.ToString(),
                DES_NOMEPERS = this.Nome,
                DES_SECCOGNOME = this.SecondoCognome,
                DES_COGNOMEPERS = this.Cognome,
                CSF_CFSPERSONA = this.CodiceFiscale,
                COD_IMPRESA = this.SelectedAzienda,
                COD_SEDE = this.SelectedSede,
                COD_MATDIP = this.Matricola,
                COD_QUALIFICA = this.SelectedCategoria,
                COD_SERVIZIO = this.SelectedServizio,
                COD_SEZIONE = this.SelectedSezione,
                COD_TIPORLAV = this.SelectedTipoAssunzione,
                TMS_TIMESTAMP = DateTime.Now,
                DTA_IMM_CREAZIONE = DateTime.Now,
                DTA_NASCITAPERS = this.DataNascita,
                DTA_INIZIO = this.DataInizio,
                DTA_FINE = (this.DataFine.HasValue ? this.DataFine.Value : new DateTime(2999, 12, 31)),
                COD_CITTAD = this.Cittadinanza,
                ANAGPERS = anagrafica,
                COD_RUOLO = this.SelectedMansione,
                JSON_ASSUNZIONI = Newtonsoft.Json.JsonConvert.SerializeObject(this)
            };
            return imm;
        }
        //public static int GetAvanzamento()
        //{
        //    int res = 0;

        //    if (TabAnagrafica)
        //    {
        //        res = 25;
        //    }
        //    if (TabDatiContrattuali)
        //    {
        //        res = 50;
        //    }
        //    if (TabAllegati)
        //    {
        //        res = 75;
        //    }
        //    if (Completo)
        //    {
        //        res = 100;
        //    }


        //    return res;
        //}
    }
    public class Assunzione_FileUploadVM
    {
        public List<XR_TB_FILES> Allegati { get; set; }
        public bool Principale { get; set; }
        public bool InModifica { get; set; }
    }
    public class Indennita
    {
        public int Id { get; set; }
        public int Prog_Evento { get; set; }
        public string Cod_Evindennita { get; set; }
        public int Id_Persona { get; set; }
        public decimal Percentuale { get; set; }
        public string Mod_Pagamento { get; set; }
        public string Cod_Indennita { get; set; }
        public string SottoCod_indennita { get; set; }
        public decimal Importo { get; set; }
        public DateTime Data_Inizio { get; set; }
        public DateTime Data_Fine { get; set; }
        public string Cod_User { get; set; }
        public string Cod_TermId { get; set; }
    }

}





