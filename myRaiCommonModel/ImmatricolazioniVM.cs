using System;
using System.Collections.Generic;
using System.Linq;
using myRaiData.Incentivi;
using System.Web.Mvc;
using myRaiHelper;
using System.ComponentModel.DataAnnotations;

namespace myRaiCommonModel
{
    public class ImmatricolazioniVM
    {
        public ImmatricolazioniVM()
        {
            this.ISTAT = new List<SelectListItem>();
        }
        public bool IsForPianoFormativo { get; set; }
        public string descrizioneSezione { get; set; }
        public string Cittadinanza { get; set; }
        public string codSezione { get; set; }
        public string Provincia { get; set; }
        public int IdEvento { get; set; }
        public int IdPersona { get; set; }
        public string Nome { get; set; }
        public string Cognome { get; set; }
        public string SecondoCognome { get; set; }
        public DateTime DataNascita { get; set; }

        public string CodiceFiscale { get; set; }
        public char? Genere { get; set; }

        public string TipoOperazione { get; set; }
        public string Matricola { get; set; }
        public string LuogoDiNascita { get; set; }
        public string CodiceISTAT { get; set; }
        public string Nazione { get; set; }
        public DateTime? DataFine { get; set; }
        public DateTime DataInizio { get; set; }
        public DateTime DataCreazione { get; set; }
        public bool esitoCics { get; set; }
        public bool esito { get; set; }
        public bool? esito_ripristino { get; set; }
        public string SelectedAzienda { get; set; }

        public string SelectedSede { get; set; }

        public string SelectedCategoria { get; set; }

        public string SelectedServizio { get; set; }
        public string SelectedSezione { get; set; }
        public int StatoImmatricolazione { get; set; }
        public string SelectedRapplavoro { get; set; }
        public string RpLavorativoForCics { get; set; }

        public List<SelectListItem> ISTAT { get; set; }

        public string Azienda { get; set; }
        public string Sede { get; set; }
        public string Servizio { get; set; }
        public string Categoria { get; set; }
        public string RappLavoro { get; set; }
        public string Sezione { get; set; }
        public bool esistePianoFormativo { get; set; }
        //TODO enumerazione tipo operazione.
        public bool VerificaDatiContrattuali()
        {
            bool esito = false;

            if (this.DataFine == null)
            {
                this.DataFine = new DateTime(9999, 12, 31);
                esito = true;
            }
            if (this.DataInizio != null && this.DataFine != null && this.SelectedRapplavoro != null && this.SelectedSede != null && this.SelectedCategoria != null && this.SelectedServizio != null && this.SelectedSezione != null && this.SelectedAzienda != null)
            {
                esito = true;
                return esito;
            }

            return esito;
        }
        public static ImmatricolazioniVM ConverToImmatricolazioneVM(XR_IMM_IMMATRICOLAZIONI entity)
        {

            return new ImmatricolazioniVM()
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
                SelectedRapplavoro = entity.COD_TIPORLAV,
                SelectedSede = entity.COD_SEDE,
                SelectedServizio = entity.COD_SERVIZIO,
                Genere = Convert.ToChar(entity.COD_SESSO)
               
            };
        }
        public XR_IMM_IMMATRICOLAZIONI ConvertToXR_IMM_IMMATRICOlAZIONI()
        {

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
                COD_TIPORLAV = this.SelectedRapplavoro,
                TMS_TIMESTAMP = DateTime.Now,
                DTA_IMM_CREAZIONE = DateTime.Now,
                DTA_NASCITAPERS = this.DataNascita,
                DTA_INIZIO = this.DataInizio,
                DTA_FINE = this.DataFine.Value,
                TIPO_OPERAZIONE = this.TipoOperazione,
                COD_CITTAD = this.Cittadinanza
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
                COD_TIPORLAV = this.SelectedRapplavoro,
                TMS_TIMESTAMP = DateTime.Now,
                DTA_IMM_CREAZIONE = DateTime.Now,
                DTA_NASCITAPERS = this.DataNascita,
                DTA_INIZIO = this.DataInizio,
                DTA_FINE = this.DataFine.Value,
                COD_CITTAD = this.Cittadinanza,
                ANAGPERS = anagrafica,
            };
            return imm;
        }
    }

}





