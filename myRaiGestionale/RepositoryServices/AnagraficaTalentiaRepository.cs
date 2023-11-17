using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using myRaiHelper.GenericRepository;
using myRaiData.Incentivi;
using myRaiCommonModel;
using System.Linq.Expressions;
using myRaiHelper;
using ComunicaCics;
using System.Web.Mvc;
using myRaiCommonManager;

namespace myRaiGestionale.RepositoryServices
{
    //Query!!!!!
    public class AnagraficaTalentiaRepository : BaseRepository<IncentiviEntities>
    {


        //public bool ModificaImmatricolazione(ImmatricolazioniVM model)
        //{

        //}


        //public bool InserisciNuovoDipendente(AnagraficaTalentiaModel model)
        //{
        //    bool esito = false;
        //    var operazione = "I";
        //    bool result_cics = false;
        //    try
        //    {
        //        var codiceFiscale_Esistente = base.GetAllSelect<ANAGPERS,string>(x => x.CSF_CFSPERSONA).ToList();
        //        var currentDate = DateTime.Now;
        //        GetCampiFirma(out codUser, out codTermid, out timestamp);
        //        if (model != null)
        //        {                     
        //            var idPersonaOID = base.GeneraOid<ANAGPERS>(x => x.ID_PERSONA);
        //            //var idEventoOID = base.GeneraOid<XR_IMM_IMMATRICOLAZIONI>(w => w.ID_EVENTO);
        //            //if (model.RpLavorativoForCics == "TI" && !model.SelectedCategoria.StartsWith("A"))
        //            //{
        //            //    RpLavorativoForCics = "1";
        //            //    DataFine = new DateTime(9999, 12, 31);
        //            //}
        //            //else if (tipoContratto == "TI" && qualifica.StartsWith("A"))
        //            //{
        //            //    RpLavorativoForCics = "3";
        //            //    DataFine = new DateTime(9999, 12, 31);
        //            //}
        //            //else if (tipoContratto == "TD" && qualifica.StartsWith("A"))
        //            //    RpLavorativoForCics = "4";
        //            //else if (tipoContratto == "TD" && !qualifica.StartsWith("A"))
        //            //    RpLavorativoForCics = "2";
        //            //else if (tipoContratto == "TI" && qualifica.StartsWith("A"))
        //            //{
        //            //    RpLavorativoForCics = "3";
        //            //    DataFine = new DateTime(9999, 12, 31);
        //            //}
        //            //else if (tipoContratto == "CO" && qualifica == "J21")
        //            //    RpLavorativoForCics = "6";
        //            //else if (tipoContratto == "CO" && qualifica == "J01" || qualifica == "J11")
        //            //    RpLavorativoForCics = "7";
        //            //var dataFineForCics = " ";
        //            //if (dataFineContratto.Value.ToShortDateString().Equals("31/12/9999"))
        //            //{
        //            //    dataFineForCics = "        ";
        //            //}
        //            var pMatricola=(CommonHelper.GetCurrentUserPMatricola());
        //            string rispostaCics =ImmatricolazioneCicsManager.ChiamataCics(model, pMatricola, dataFineForCics,operazione);
        //            if(rispostaCics.Length < 10)
        //            {
        //                if(rispostaCics.Equals( "ACK99")){return esito;}
        //            }

        //            if (rispostaCics.Substring(151,3).Equals("000")) { result_cics = true; }
        //            //Inserisco i dati sulla tabella storica
        //            var result = base.Add(new XR_IMM_IMMATRICOLAZIONI()
        //            {
        //                ID_PERSONA = Convert.ToInt32(idPersonaOID),
        //                COD_CITTA = model.LuogoDiNascita,
        //                COD_SESSO = model.Genere.ToString(),
        //                DES_NOMEPERS = model.Nome,
        //                DES_SECCOGNOME = model.SecondoCognome,
        //                DES_COGNOMEPERS = model.Cognome,
        //                DES_CFSPERSONA = model.CodiceFiscale,
        //                COD_IMPRESA = model.SelectedAzienda,
        //                COD_SEDE = model.SelectedSede,
        //                COD_MATDIP = model.Matricola,
        //                P_MATRICOLA = pMatricola,
        //                COD_QUALIFICA = model.SelectedCategoria,
        //                COD_SERVIZIO = model.SelectedServizio,
        //                COD_SEZIONE = model.SelectedSezione,
        //                COD_TERMID = codTermid,
        //                COD_TPCNTR = model.SelectedRapplavoro,
        //                COD_USER = codUser,
        //                TMS_TIMESTAMP = timestamp,
        //                DTA_IMM_CREAZIONE = currentDate,
        //                DTNASCITA = model.DataNascita,
        //                DTA_INIZIO = model.DataInizio,
        //                DTA_FINE =model.DataFine.Value,
        //                ESITO_CICS = result_cics,
        //                TIPO_OPERAZIONE = "I",
        //                RISPOSTA_CICS = rispostaCics
        //            });


        //            //Se è tutto ok inserisco dati d'immatricolazione su Talentia

        //            //var anagrafica_personale = base.Add(new ANAGPERS()
        //            //{
        //            //    ID_PERSONA = Convert.ToInt32(idPersonaOID),
        //            //    COD_CITTA = model.LuogoDiNascita,
        //            //    COD_SESSO = model.Genere.ToString(),
        //            //    DES_COGNOMEPERS = model.Cognome,
        //            //    DES_NOMEPERS = model.Nome,
        //            //    DES_SECCOGNOME = model.SecondoCognome,
        //            //    CSF_CFSPERSONA = model.CodiceFiscale,
        //            //    DTA_NASCITAPERS = model.DataNascita,
        //            //    COD_USER = codUser,
        //            //    COD_TERMID = codTermid,
        //            //    TMS_TIMESTAMP = timestamp,
        //            //    COD_MATDIP = model.Matricola,
        //            //    COD_PLANNING = "A",
        //            //    IND_DOMICILIO = "N",
        //            //    IND_PATENTE = "N",
        //            //    FLG_ALLERGIES = "N",
        //            //    FLG_DIABETIC = "N",
        //            //    COD_STCIV = "---",
        //            //    IND_RECAPITO = "N",
        //            //});
        //            //var ass_qual=base.Add(new ASSQUAL()
        //            //{
        //            //    ID_ASSQUAL = base.GeneraOid<ASSQUAL>(x => x.ID_ASSQUAL),
        //            //    COD_QUALIFICA = codiceQualifica,
        //            //    DTA_INIZIO = model.DataInizio,
        //            //    DTA_FINE = model.DataFine.Value,
        //            //    ID_PERSONA = idPersonaOID,
        //            //    COD_USER = codUser,
        //            //    COD_TERMID = codTermid,
        //            //    TMS_TIMESTAMP = timestamp,
        //            //    COD_EVQUAL = "ND",
        //            //    IND_AUTOM = "N",
        //            //});
        //            //var trasf_sede=base.Add(new TRASF_SEDE()
        //            //{
        //            //    COD_USER = codUser,
        //            //    COD_TERMID = codTermid,
        //            //    TMS_TIMESTAMP = timestamp,
        //            //    ID_TRASF_SEDE = base.GeneraOid<TRASF_SEDE>(x => x.ID_TRASF_SEDE),
        //            //    ID_PERSONA = idPersonaOID,
        //            //    COD_IMPRESA = model.SelectedAzienda,
        //            //    COD_SEDE = codiceSede,
        //            //    DTA_FINE = model.DataFine.Value,
        //            //    DTA_INIZIO = model.DataInizio,
        //            //    COD_EVTRASF = "ND"
        //            //});
        //            //var ass_tpContratti=base.Add(new ASSTPCONTR()
        //            //{
        //            //    ID_ASSTPCONTR = base.GeneraOid<ASSTPCONTR>(x => x.ID_ASSTPCONTR),
        //            //    COD_USER = codUser,
        //            //    COD_TERMID = codTermid,
        //            //    TMS_TIMESTAMP = timestamp,
        //            //    DTA_FINE = model.DataFine.Value,
        //            //    DTA_INIZIO = model.DataInizio,
        //            //    ID_PERSONA = idPersonaOID,
        //            //    COD_EVCNTR = "ND",
        //            //    COD_TPCNTR = codiceTPCNTR,
        //            //});
        //            //var incarichi_lav=base.Add(new INCARLAV()
        //            //{
        //            //    COD_USER = codUser,
        //            //    COD_TERMID = codTermid,
        //            //    TMS_TIMESTAMP = timestamp,
        //            //    DTA_FINE = model.DataFine.Value,
        //            //    DTA_INIZIO = model.DataInizio,
        //            //    ID_INCARLAV = base.GeneraOid<INCARLAV>(x => x.ID_INCARLAV),
        //            //    ID_PERSONA = idPersonaOID,
        //            //    IND_INCPRINC = "Y",
        //            //    PRC_PERCOCCUPAZ = 100,
        //            // //   ID_UNITAORG = codiceUnitaOrg,
        //            //});
        //            //var servizi =base.Add(new XR_SERVIZIO()
        //            //{
        //            //    DTA_FINE = model.DataFine.Value,
        //            //    DTA_INIZIO = model.DataInizio,
        //            //    COD_EVSERVIZIO = "ND",
        //            //    ID_XR_SERVIZIO = base.GeneraOid<XR_SERVIZIO>(x => x.ID_XR_SERVIZIO),
        //            //    ID_PERSONA = idPersonaOID,
        //            //    COD_USER = codUser,
        //            //    COD_TERMID = codTermid,
        //            //    TMS_TIMESTAMP = timestamp,
        //            //    COD_SERVIZIO = codiceServizio,
        //            //});
        //            //base.Save(anagrafica_personale);
        //            //base.Save(ass_qual);
        //            //base.Save(trasf_sede);
        //            //base.Save(ass_tpContratti);
        //            //base.Save(incarichi_lav);
        //            //base.Save(servizi);
        //            base.Save(result);
        //            esito = true;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    return esito;
        //}


        //public List<AnagraficaTalentiaModel> GetListImmatricolazioni()
        //{
        //    var immatricolazioni= base.GetAll<XR_IMM_IMMATRICOLAZIONI>().AsEnumerable().Select(s => AnagraficaTalentiaModel.Create(s)).ToList();
        //    return immatricolazioni;
        //}

        //public bool AddManyRows(IEnumerable<ANAGPERS> entityAnag, IEnumerable<ASSQUAL> entityAssQual, IEnumerable<TRASF_SEDE> entitiySede, IEnumerable<ASSTPCONTR> entityContratto, IEnumerable<INCARLAV> entityIncarlav, IEnumerable<XR_SERVIZIO> entityServizio  ) 
        //{
        //    bool inserito;
        //    try
        //    {
        //        base.AddMany<ANAGPERS>(entityAnag);
        //        base.AddMany(entityAssQual);
        //        base.AddMany<TRASF_SEDE>(entitiySede);
        //        base.AddMany<ASSTPCONTR>(entityContratto);
        //        base.AddMany<INCARLAV>(entityIncarlav);
        //        base.AddMany<XR_SERVIZIO>(entityServizio);
        //        inserito = true;
        //    }catch(Exception ex)
        //    {
        //        var msg = String.Empty;
        //        var fail =  new Exception(msg,ex);
        //        inserito = false;
        //    }
        //    return inserito;
        //}
        public AnagraficaTalentiaRepository(IncentiviEntities db) : base(db)
        {
        }
    }


}