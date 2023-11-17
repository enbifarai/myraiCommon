using myRaiData.Incentivi;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiCommonModel
{
    public class AnagNoDipRapportoModel : _IdentityData
    {
        public AnagNoDipRapportoModel()
        {
            IsNew = true;
            DataInizio = DateTime.Today;
            DataFine = DataInizio.AddDays(1);
        }
        public AnagNoDipRapportoModel(XR_NDI_TIPO_SOGGETTO dbRec)
        {
            IsNew = false;
            IdPersona = dbRec.ID_PERSONA;
            Codice = dbRec.COD_TIPO_SOGGETTO;
            Descrizione = dbRec.XR_NDI_TB_TIPO_RAPPORTO.DES_TIPOLOGIA;
            Societa = dbRec.COD_IMPRESA;
            DataInizio = dbRec.DTA_INIZIO;
            DataFine = dbRec.DTA_FINE;
            MatricolaCollegata = dbRec.COD_MAT_COLLEGATA;
            DesSocieta = dbRec.CODIFYIMP.DES_RAGSOC;
        }
        public int IdAnag { get; set; }
        public int Id { get; set; }
        public string Codice { get; set; }
        public string Descrizione { get; set; }
        public DateTime DataInizio { get; set; }
        public DateTime DataFine { get; set; }
        public string Societa { get; set; }
        public string DesSocieta { get; set; }

        public string MatricolaCollegata { get; set; }

        public bool IsNew { get; set; }
    }

    public class AnagNoDipIndirizzi : BaseAnagraficaData
    {
        public AnagNoDipIndirizzi() : base()
        {
            Residenza = new IndirizzoModel();
            Domicilio = new IndirizzoModel();
            DataInizio = DateTime.Today;
            DataFine = new DateTime(9999, 12, 31);
            IsNew = true;
        }
        public AnagNoDipIndirizzi(XR_NDI_RESIDENZA_DOMICILIO dbRec) : base()
        {
            DataInizio = dbRec.DTA_INIZIO;
            DataFine = dbRec.DTA_FINE;

            Residenza = new IndirizzoModel()
            {
                Tipologia = IndirizzoType.Residenza,
                IdPersona = dbRec.ID_PERSONA,
                Decorrenza = dbRec.DTA_INIZIO,
                DataFine = dbRec.DTA_FINE,
                CodCitta = dbRec.COD_CITTARES,
                Indirizzo = dbRec.DES_INDIRRES.TitleCase(),
                Citta = String.Format("{0}, {1}", dbRec.TB_COMUNE_RESIDENZA.DES_CITTA.TitleCase(), dbRec.TB_COMUNE_RESIDENZA.COD_PROV_STATE),
                CAP = dbRec.CAP_CAPRES,
                CodStato = dbRec.TB_COMUNE_RESIDENZA.COD_SIGLANAZIONE,
                Stato = dbRec.TB_COMUNE_RESIDENZA.TB_NAZIONE.DES_NAZIONE
            };

            Domicilio = new IndirizzoModel()
            {
                Tipologia = IndirizzoType.Domicilio,
                IdPersona = dbRec.ID_PERSONA,
                Decorrenza = dbRec.DTA_INIZIO,
                DataFine = dbRec.DTA_FINE,
                CodCitta = dbRec.COD_CITTADOM,
                Indirizzo = dbRec.DES_INDIRDOM.TitleCase(),
                Citta = String.Format("{0}, {1}", dbRec.TB_COMUNE_DOMICILIO.DES_CITTA.TitleCase(), dbRec.TB_COMUNE_DOMICILIO.COD_PROV_STATE),
                CAP = dbRec.CAP_CAPDOM,
                CodStato = dbRec.TB_COMUNE_DOMICILIO.COD_SIGLANAZIONE,
                Stato = dbRec.TB_COMUNE_DOMICILIO.TB_NAZIONE.DES_NAZIONE
            };

            LastModifiedTime = dbRec.TMS_TIMESTAMP;
            IsNew = false;
        }
        public int IdAnag { get; set; }
        public DateTime DataInizio { get; set; }
        public DateTime DataFine { get; set; }
        public bool IsNew { get; set; }
        public IndirizzoModel Residenza { get; set; }
        public IndirizzoModel Domicilio { get; set; }
    }

    public class AnagNoDipIbanModel : IbanModel
    {
        public AnagNoDipIbanModel() : base()
        {
            this.Tipologia = IbanType.NonDefinito;
        }
        public AnagNoDipIbanModel(XR_DATIBANCARI dbRec) : base(dbRec)
        {
            Tipologia = IbanType.NonDefinito;
        }

        public int IdAnag { get; set; }
    }

    public class AnagraficaNoDipModel : _IdentityData
    {
        public AnagraficaNoDipModel()
        {
            DatiAnagrafici = new AnagraficaDatiAnag();
            DatiBancari = new List<AnagNoDipIbanModel>();
            ResidenzaDomicilio = new List<AnagNoDipIndirizzi>();
            Rapporti = new List<AnagNoDipRapportoModel>();
            DataInizio = DateTime.Today;
            DataFine = new DateTime(9999, 12, 31);
        }

        public AnagraficaNoDipModel(XR_NDI_ANAG dbRec)
        {
            DataInizio = DateTime.Today;
            DataFine = new DateTime(9999, 12, 31);

            IdAnag = dbRec.ID_ANAG;
            IdPersona = dbRec.ID_PERSONA;
            Matricola = dbRec.MATRICOLA;

            DataInizio = dbRec.DTA_INIZIO;
            DataFine = dbRec.DTA_FINE;

            IsDipendente = dbRec.IND_DIPENDENTE;

            DatiAnagrafici = new AnagraficaDatiAnag(dbRec.ANAGPERS);
            DatiAnagrafici.Matricola = dbRec.MATRICOLA;

            DatiBancari = new List<AnagNoDipIbanModel>();
            if (dbRec.ANAGPERS.XR_DATIBANCARI != null)
            {
                foreach (var dbIban in dbRec.ANAGPERS.XR_DATIBANCARI)
                {
                    var iban = new AnagNoDipIbanModel(dbIban);
                    iban.IdAnag = IdAnag;
                    DatiBancari.Add(iban);
                }
            }

            ResidenzaDomicilio = new List<AnagNoDipIndirizzi>();
            if (dbRec.ANAGPERS.XR_NDI_RESIDENZA_DOMICILIO != null)
            {
                foreach (var res in dbRec.ANAGPERS.XR_NDI_RESIDENZA_DOMICILIO)
                {
                    var resDom = new AnagNoDipIndirizzi(res);
                    resDom.IdAnag = IdAnag;
                    ResidenzaDomicilio.Add(resDom);
                }
            }

            Rapporti = new List<AnagNoDipRapportoModel>();
            if (dbRec.ANAGPERS.XR_NDI_TIPO_SOGGETTO != null)
            {
                foreach (var recRapp in dbRec.ANAGPERS.XR_NDI_TIPO_SOGGETTO)
                {
                    var rapp = new AnagNoDipRapportoModel(recRapp);
                    rapp.IdAnag = IdAnag;
                    Rapporti.Add(rapp);
                }
            }
        }


        public int IdAnag { get; set; }
        public DateTime DataInizio { get; set; }
        public DateTime DataFine { get; set; }
        public bool IsDipendente { get; set; }
        public AnagraficaDatiAnag DatiAnagrafici { get; set; }
        public List<AnagNoDipIndirizzi> ResidenzaDomicilio { get; set; }
        public List<AnagNoDipRapportoModel> Rapporti { get; set; }
        public List<AnagNoDipIbanModel> DatiBancari { get; set; }
    }

    public class AnagNoDipRicerca
    {
        public bool HasFilter { get; set; }
        public string Cognome { get; set; }
        public string Nome { get; set; }
        public string CodiceFiscale { get; set; }
        public string Matricola { get; set; }
    }
}
