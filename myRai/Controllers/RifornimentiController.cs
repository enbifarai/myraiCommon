using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data;
using System.Globalization;
using System.IO;
using myRaiData;
using iTextSharp.text.pdf;
using myRaiCommonTasks;
using myRaiHelper;
using myRaiCommonModel;
using Logger = myRaiHelper.Logger;

//Solo i dirigenti o le matricole con almeno un rif negli ultimi 3 anni possono vedere la sezione Rifornimenti
//Solo gli assegnatari di autovetture possono inserire o modificare le schede carburante
//Schede Carburante e i relativi Rifornimenti possono essere inseriti, modificati o cancellati solo se la data odierna rispetto l'anno selezionato è nel range tra data Decorrenza/Scadenza del servizio  
//Schede Carburante e i relativi Rifornimenti possono essere inseriti, modificati o cancellati solo se hanno lo stato di lavorazione impostato su "Inserito" altrimenti soltanto letti
//La data dei Rifornimenti inseriti nella scheda carburante dev'essere sempre compresa nell'anno contabile di riferimento e mai futura alla data di inserimento.

namespace myRai.Controllers
{
    public class RifornimentiController : BaseCommonController
    {
        public ActionResult Index()
        {
            try
            {
                string matricola = CommonHelper.GetCurrentUserMatricola();
                if (checkAuth_VedereRifornimenti(matricola) == false)
                { return View("NonAbilitatoError2"); }

                SituazRifornimentiViewModel modello = new SituazRifornimentiViewModel();
                modello.RiepilogoAnno = getModel_RiepilogoRifornimenti(DateTime.Now.Year, matricola);
                modello.RifornimentiAnno = getModel_StoricoRifornimenti(DateTime.Now.Year, matricola);


                var db = new HRPADBEntities();
                var dbgapp = new digiGappEntities();
                DateTime D = DateTime.Now;
                var row= db.T_SkCarburantiAssegnatari.Where(x => x.matricola_SkCarburantiAssegnatari == matricola && x.scadenza_SkCarburantiAssegnatari>=D ).FirstOrDefault();
                if (row != null && row.MessaggioPersonale_SkCarburantiAssegnatari != null && row.MessaggioPersonale_SkCarburantiAssegnatari.Trim() != "")
                {
                    var parametro = dbgapp.MyRai_ParametriSistema.Where(x => x.Chiave == row.MessaggioPersonale_SkCarburantiAssegnatari).FirstOrDefault();
                    if (parametro != null)
                    {
                        if (!String.IsNullOrWhiteSpace(parametro.Valore2))
                        {
                            DateTime Dlimite;
                            if (DateTime.TryParseExact(parametro.Valore2, "dd/MM/yyyy", null, DateTimeStyles.None, out Dlimite))
                            {
                                if (D<=Dlimite) modello.MessaggioAssegnatario = parametro.Valore1;
                            }
                        }
                        else
                            modello.MessaggioAssegnatario = parametro.Valore1;
                    }
                }

                return View(modello);
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        public ActionResult getFile_Ricevuta(string idDoc)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(idDoc))
                { throw new InvalidOperationException("Id allegato non valido"); }

                string fileType;
                string fileMime = "";
                byte[] fileContent;
                using (HRPADBEntities db = new HRPADBEntities())
                {
                    var ricevuta = new HRPADBEntities().T_SkCarburantiDoc
                                    .FirstOrDefault(n => n.Chiave_SkCarburantiDoc == idDoc);

                    if (ricevuta == null)
                    { throw new InvalidOperationException("Allegato non disponibile"); }

                    if (ricevuta.TipoDoc_SkCarburantiDoc == null || ricevuta.Documento_SkCarburantiDoc == null)
                    { throw new InvalidOperationException("Allegato danneggiato"); }

                    fileType = ricevuta.TipoDoc_SkCarburantiDoc.ToLower();
                    fileContent = ricevuta.Documento_SkCarburantiDoc;
                }
                Response.AddHeader("Content-Disposition", "inline; filename=" + "Ricevuta." + fileType);
                switch (fileType)
                {
                    case "pdf":
                        fileMime = "application/pdf";
                        break;
                    case "jpg":
                        fileMime = "image/jpeg";
                        break;
                    case "png":
                        fileMime = "image/png";
                        break;
                    case "bmp":
                        fileMime = "image/bmp";
                        break;
                    default:
                        fileMime = "application/octet-stream";
                        break;
                }
                return File(fileContent, fileMime);
            }
            catch (InvalidOperationException ex)
            {
                return Content("<script language='javascript' type='text/javascript'>alert('" + ex.Message + "');</script>");
            }
            catch (Exception)
            {
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult getView_StoricoRifornimenti(string targetYear)
        {
            try
            {
                int annoRif = checkParse_AnnoRif(targetYear);
                string matricola = CommonHelper.GetCurrentUserMatricola();
                return PartialView("~/Views/Rifornimenti/subpartial/StoricoRifornimenti.cshtml", getModel_StoricoRifornimenti(annoRif, matricola));
            }
            catch (Exception)
            {
                return Content("<script language='javascript' type='text/javascript'>alert('Errore: impossibile caricare lista rifornimenti carburante');</script>");
            }
        }

        [HttpPost]
        public ActionResult getView_RiepilogoRifornimenti(string targetYear)
        {
            try
            {
                int annoRif = checkParse_AnnoRif(targetYear);
                string matricola = CommonHelper.GetCurrentUserMatricola();
                return PartialView("~/Views/Rifornimenti/subpartial/RiepilogoRifornimenti.cshtml", getModel_RiepilogoRifornimenti(annoRif, matricola));
            }
            catch (Exception)
            {
                return Content("<script language='javascript' type='text/javascript'>alert('Errore: impossibile caricare dati riepilogativi');</script>");
            }
        }

        [HttpPost]
        public ActionResult getView_InserisciSchedaCarburante(string targetYear)
        {
            try
            {
                int annoRif = checkParse_AnnoRif(targetYear);
                string matricola = CommonHelper.GetCurrentUserMatricola();
                DateTime startServizio = new DateTime();
                DateTime endServizio = new DateTime();
                bool AuthToEditSkCarb = checkAuth_InsertEditSkCarb(annoRif, out startServizio, out endServizio);

                if (AuthToEditSkCarb == false)
                { throw new Exception("E' possibile inserire le schede carburante solo tra il " + startServizio.ToShortDateString() + "e il " + endServizio.ToShortDateString()); }

                SchedaCarburante model = new SchedaCarburante() { Anno = annoRif };
                return PartialView("~/Views/Rifornimenti/subpartial/InserisciSchedaCarburante.cshtml", model);
            }
            catch (InvalidOperationException ex)
            {
                return Content("<script language='javascript' type='text/javascript'>swal('" + ex.Message + "');</script>");
            }
            catch (Exception)
            {
                return Content("<script language='javascript' type='text/javascript'>swal('Impossibile inserire la scheda carburante'); $('#containerModaleInsertSkCarb').modal('hide'); CambiaAnno(0); </script>");
            }
        }

        [HttpPost]
        public ActionResult getView_ModificaSchedaCarburante(string targetYear, string dataRifToEdit)
        {
            try
            {
                int annoRif = checkParse_AnnoRif(targetYear);
                DateTime startServizio = new DateTime();
                DateTime endServizio = new DateTime();
                bool AuthToEditSkCarb = checkAuth_InsertEditSkCarb(annoRif, out startServizio, out endServizio);
                if (AuthToEditSkCarb == false)
                { throw new Exception("E' possibile modificare le schede carburante solo tra il " + startServizio.ToShortDateString() + "e il " + endServizio.ToShortDateString()); }

                DateTime dateRif = DateTime.Parse(dataRifToEdit);
                string matricola = CommonHelper.GetCurrentUserMatricola();
                SchedaCarburante model = getModel_ModificaSchedaCarburante(annoRif, matricola, dateRif);
                return PartialView("~/Views/Rifornimenti/subpartial/InserisciSchedaCarburante.cshtml", model);
            }
            catch (InvalidOperationException ex)
            {
                return Content("<script language='javascript' type='text/javascript'>swal('" + ex.Message + "'); $('#containerModaleInsertSkCarb').modal('hide'); CambiaAnno(0); </script>");
            }
            catch (Exception)
            {
                return Content("<script language='javascript' type='text/javascript'>swal('Impossibile modificare la scheda carburante'); $('#containerModaleInsertSkCarb').modal('hide'); CambiaAnno(0); </script>");
            }
        }

        [HttpPost]
        [OutputCache(NoStore = true, Duration = 0)]
       
        public ActionResult getView_EditSkCarb_FormSubmitManager(SchedaCarburante modello)
        {
            try
            {
                string matricola = CommonHelper.GetCurrentUserMatricola();
                string fileExtension = null;
                byte[] fileBytes = null;
                modello.idxEvidenziaRif = 999;

                bool isNew = String.IsNullOrEmpty( modello.IdSkCarb );

                if (modello == null)
                { return Content("<script language='javascript' type='text/javascript'>swal('Errore: richiesta non valida'); $('#containerModaleInsertSkCarb').modal('hide'); CambiaAnno(0);</script>"); }

                if (ModelState.IsValid == false) { throw new InvalidOperationException("Dati inseriti non corretti: correggere e riprovare"); }

                if (modello.RifornimentiInseriti == null || modello.RifornimentiInseriti.Count == 0) { throw new InvalidOperationException("È necessario inserire almeno un rifornimento"); }

                if (modello.RifornimentiInseriti.Count > 9) { throw new InvalidOperationException("Troppi rifornimenti inseriti, massimo 9"); }

                //Validazione file allegato se la scheda carburante è nuova o è stato modificato in una scheda carburante preesistente
                if (modello.IdSkCarb == null && (modello.FileNameSkCarb == null || modello.FileContentSkCarb == null)) { throw new InvalidOperationException("Allegare la scansione della scheda carburante"); }

                if (modello.FileNameSkCarb != null)
                {
                    fileExtension = modello.FileNameSkCarb.Substring(modello.FileNameSkCarb.LastIndexOf(".") + 1).ToLower();
                    if (RifornimentiConfig.EstensioniFilePermesse.Contains("." + fileExtension) == false) { throw new InvalidOperationException("Allegato non valido: formato file non valido"); }
                }

                if (modello.FileContentSkCarb != null)
                {
                    fileBytes = Convert.FromBase64String(modello.FileContentSkCarb.Substring(modello.FileContentSkCarb.IndexOf(',') + 1));
                    if (fileBytes.Length == 0 || fileBytes.Length > RifornimentiConfig.BytesMaxSkCarbAllegato) { throw new InvalidOperationException("Allegato non valido (dimensione massima " + RifornimentiConfig.BytesMaxSkCarbAllegato / 1000000 + "MB)"); }
                }

                using (HRPADBEntities db = new HRPADBEntities())
                {
                    //Validazione data dei rifornimenti
                    DateTime dataDecorrenza = new DateTime();
                    DateTime dataTermine = new DateTime();
                    if (checkAuth_InsertEditSkCarb(modello.Anno, out dataDecorrenza, out dataTermine) == false) { throw new InvalidOperationException("Dal " + dataTermine.ToShortDateString() + "non è più possibile modificare le schede carburanti dell'anno " + modello.Anno); }

                    for (int i = 0; i < modello.RifornimentiInseriti.Count(); i++)
                    {
                        modello.idxEvidenziaRif = i;
                        DateTime dataRif = modello.RifornimentiInseriti[i].Data.GetValueOrDefault().Add(modello.RifornimentiInseriti[i].Orario.GetValueOrDefault());

                        if (dataRif.Year != modello.Anno) { throw new InvalidOperationException("Questa scheda carburante deve contenere rifornimenti inclusi nell'anno " + @modello.Anno); }

                        if (dataRif > DateTime.Now) { throw new InvalidOperationException("La data dei rifornimenti non può essere nel futuro"); }

                        if (modello.RifornimentiInseriti.Count(n => n.Data == modello.RifornimentiInseriti[i].Data && n.Orario == modello.RifornimentiInseriti[i].Orario) > 1) { throw new InvalidOperationException("Sono stati inseriti due o più rifornimenti nella stessa data ed ora"); }

                        if (dataRif < dataDecorrenza) { throw new InvalidOperationException("Per l'anno " + modello.Anno + " la data dei rifornimenti dev'essere successiva al " + dataDecorrenza.ToShortDateString()); }

                        if (dataRif > dataTermine) { throw new InvalidOperationException("Per l'anno " + modello.Anno + " la data dei rifornimenti dev'essere precedente al " + dataTermine.ToShortDateString()); }

                        if (checkAuth_FuelCard(matricola, dataRif) == false) { throw new InvalidOperationException("L'utente non è abilitato ad inserire rifornimenti in questa data"); }

                        //verifico che non ci siano conflitti di date con rifornimenti già salvati nel db
                        string idDoc = modello.IdSkCarb ?? "";

                        var RifGiaEsistente = db.T_SkCarburantiDati.Any(n =>
                            n.Anno_SkCarburantiDati == modello.Anno &&
                            n.Matricola_SkCarburantiDati == matricola &&
                            n.DataTransazione_SkCarburantiDati == dataRif &&
                            n.Attivo_SkCarburantiDati == 0 &&
                            n.Documento_SkCarburantiDati != idDoc);

                        if (RifGiaEsistente)
                        { throw new InvalidOperationException("Errore: in data " + dataRif.ToString("dd/MM/yyyy hh:mm") + " esiste già un rifornimento registrato"); }
                    }

                    var oldRifs = new List<T_SkCarburantiDati>();
                    //Se idSkCarb è popolato significa che è una modifica della skcarb preesistente, quindi prendo tutti i rif salvati nel db
                    if (modello.IdSkCarb != null)
                    {
                        oldRifs = db.T_SkCarburantiDati.Where(n =>
                            n.Anno_SkCarburantiDati == modello.Anno &&
                            n.Matricola_SkCarburantiDati == matricola &&
                            n.Documento_SkCarburantiDati == modello.IdSkCarb &&
                            n.Attivo_SkCarburantiDati == 0).ToList();

                        if (oldRifs == null || oldRifs.Count(n => n.Stato_SkCarburantiDati == "Inserito") == 0)
                        { throw new Exception("Nessun rifornimento modificabile associato alla scheda carburante"); }
                    }

                    //Se fileByte è popolato significa che è stato selezionato un nuovo allegato, quindi genero una nuova chiave per l'allegato
                    if (fileExtension != null && fileBytes != null)
                    {
                        if (oldRifs.Any(n => n.Stato_SkCarburantiDati != "Inserito"))
                        { throw new InvalidOperationException("Impossibile cambiare l'allegato di una scheda carburante contenente rifornimenti non modificabili"); }

                        var newIdSkCarb = modello.Anno
                                         + CommonHelper.GetCurrentUserMatricola()
                                         + DateTime.Now.ToString("yyyyMMddHHmmss");

                        db.T_SkCarburantiDoc.Add(new T_SkCarburantiDoc
                        {
                            Chiave_SkCarburantiDoc = newIdSkCarb,
                            TipoDoc_SkCarburantiDoc = fileExtension,
                            Documento_SkCarburantiDoc = fileBytes,
                        });

                        var oldAllegato = db.T_SkCarburantiDoc.FirstOrDefault(n => n.Chiave_SkCarburantiDoc == modello.IdSkCarb);
                        if (oldAllegato != null) { db.T_SkCarburantiDoc.Remove(oldAllegato); }
                        modello.IdSkCarb = newIdSkCarb;
                        db.SaveChanges();
                    }

                    //Prima di confrontare gli aggiornamenti filtro solo i rifornimenti che sono in uno stato che permette la modifica
                    oldRifs = oldRifs.Where(n => n.Stato_SkCarburantiDati == "Inserito").ToList();
                    modello.RifornimentiInseriti = modello.RifornimentiInseriti.Where(n => n.StatoLavorazione == "Inserito").ToList();

                    //Per ciascuno dei vecchi record verifico se qualche dato è cambiato rispetto ai nuovi record
                    foreach (var oldrif in oldRifs)
                    {
                        string importo = oldrif.ImpoSiIvaSiScoMag_SkCarburantiDati.HasValue ? oldrif.ImpoSiIvaSiScoMag_SkCarburantiDati.Value.ToString("F", CultureInfo.InvariantCulture) : "";
                        string quantita = oldrif.QtaCarb_SkCarburantiDati.HasValue ? oldrif.QtaCarb_SkCarburantiDati.Value.ToString("F", CultureInfo.InvariantCulture) : "";
                        string contakm = oldrif.Chilometri_SkCarburantiDati.ToString();

                        var rifInvariato = modello.RifornimentiInseriti.FirstOrDefault(n =>
                            modello.Anno == oldrif.Anno_SkCarburantiDati &&
                            modello.IdSkCarb == oldrif.Documento_SkCarburantiDati &&
                            modello.TipoCarb == oldrif.TipoCarb_SkCarburantiDati &&
                            modello.TipoSkCarb == oldrif.TipoSkCar_SkCarburantiDati &&
                            modello.TargaAssociata == oldrif.Targa_SkCarburantiDati &&
                            n.Data == oldrif.DataTransazione_SkCarburantiDati.Date &&
                            n.Orario == oldrif.DataTransazione_SkCarburantiDati.TimeOfDay &&
                            n.Quantita == quantita &&
                            n.Importo == importo &&
                            n.ContaKm == contakm &&
                            n.Nazione == oldrif.Paese_SkCarburantiDati);

                        if (rifInvariato == null)
                        {   //Cancellazione logica
                            var oldRifStates = db.T_SkCarburantiDati.Where(n =>
                                n.Anno_SkCarburantiDati == modello.Anno &&
                                n.Matricola_SkCarburantiDati == matricola &&
                                n.DataTransazione_SkCarburantiDati == oldrif.DataTransazione_SkCarburantiDati).ToArray();

                            if (oldRifStates != null)
                            {
                                int statoLogicoMax = oldRifStates.Max(n => n.Attivo_SkCarburantiDati) + 1;

                                var rifAttivo = oldRifStates.FirstOrDefault(n => n.Attivo_SkCarburantiDati == 0);
                                if (rifAttivo != null) { rifAttivo.Attivo_SkCarburantiDati = statoLogicoMax; }
                            }
                        }
                        else
                        {
                            modello.RifornimentiInseriti.Remove(rifInvariato);
                        }
                    }
                    db.SaveChanges();

                    //Inserimento rif
                    string pmatricola = CommonHelper.GetCurrentRealUsername();

                    foreach (var rif in modello.RifornimentiInseriti)
                    {
                        DateTime dataRif = rif.Data.GetValueOrDefault().Add(rif.Orario.GetValueOrDefault());

                        var newRif = new T_SkCarburantiDati
                        {
                            Anno_SkCarburantiDati = modello.Anno,
                            Matricola_SkCarburantiDati = matricola,
                            DataTransazione_SkCarburantiDati = dataRif,
                            Attivo_SkCarburantiDati = 0,
                            Utente_SkCarburantiDati = pmatricola,
                            DataIns_SkCarburantiDati = DateTime.Now,
                            QtaCarb_SkCarburantiDati = decimal.Parse(rif.Quantita, CultureInfo.InvariantCulture),
                            ImpoSiIvaSiScoMag_SkCarburantiDati = decimal.Parse(rif.Importo, CultureInfo.InvariantCulture),
                            ControValSiIvaSiScoMag_SkCarburantiDati = decimal.Parse(rif.Importo, CultureInfo.InvariantCulture),
                            Chilometri_SkCarburantiDati = int.Parse(rif.ContaKm, CultureInfo.InvariantCulture),
                            Paese_SkCarburantiDati = rif.Nazione,
                            Targa_SkCarburantiDati = modello.TargaAssociata.ToUpper(),
                            TipoCarb_SkCarburantiDati = modello.TipoCarb,
                            TipoSkCar_SkCarburantiDati = modello.TipoSkCarb,
                            TipoDoc_SkCarburantiDati = fileExtension,
                            Documento_SkCarburantiDati = modello.IdSkCarb,
                            CodValuta_SkCarburantiDati = "EUR",
                            Stato_SkCarburantiDati = "Inserito",
                        };
                        db.T_SkCarburantiDati.Add(newRif);
                    }
                    db.SaveChanges();

                    if ( isNew )
                    {
                        // solo in insert
                        // una volta terminato il salvataggio
                        // Invia una mail di notifica agli indirizzi inseriti in 
                        // parametri di sistema, chiave "NotificaInserimentoSKCarburante"
                        InvioMail( matricola , UtenteHelper.Nominativo( ) );
                    }

                    modello = null;
                    return Content("<script language='javascript' type='text/javascript'>swal('Scheda carburante salvata correttamente'); $('#containerModaleInsertSkCarb').modal('hide'); CambiaAnno(0);</script>");
                }
            }
            catch (InvalidOperationException ex)
            {
                LogError(ex);
                modello.FileContentSkCarb = null;
                modello.ReturnMessage = ex.Message;
                return PartialView("~/Views/Rifornimenti/subpartial/InserisciSchedaCarburante.cshtml", modello);
            }
            catch (Exception ex)
            {
                LogError(ex);
                modello.FileContentSkCarb = null;
                modello.ReturnMessage = "Impossibile attuare la richiesta a causa di un errore interno";
                return PartialView("~/Views/Rifornimenti/subpartial/InserisciSchedaCarburante.cshtml", modello);
            }
        }

        private static void LogError(Exception ex)
        {
            Logger.LogErrori(new MyRai_LogErrori()
            {
                applicativo = "PORTALE",
                data = DateTime.Now,
                error_message = ex.ToString(),
                matricola = CommonHelper.GetCurrentUserMatricola(),
                provenienza = "getView_EditSkCarb_FormSubmitManager"
            });
        }

        private RiepilogoRifornimentiAnno getModel_RiepilogoRifornimenti(int annoRif, string matricola)
        {
            RiepilogoRifornimentiAnno riepilogo = new RiepilogoRifornimentiAnno();
            try
            {
                var rifs = getRifornamenti(annoRif, matricola);
                foreach (var rif in rifs)
                {
                    //se non ha il pdf con gli scontrini è una fuel card
                    if (string.IsNullOrWhiteSpace(rif.Documento_SkCarburantiDati))
                    {
                        riepilogo.TotFuelCard += rif.QtaCarb_SkCarburantiDati.GetValueOrDefault(0);
                    }
                    else
                    {
                        if (rif.Paese_SkCarburantiDati == RifornimentiConfig.TipiNazione[0])
                        {
                            riepilogo.TotSkCarbItalia += rif.QtaCarb_SkCarburantiDati.GetValueOrDefault(0);
                            //se il campo contiene la data è stato contabilizzato
                            if (!string.IsNullOrWhiteSpace(rif.AnnoMeseContab_SkCarburantiDati))
                            {
                                riepilogo.TotContabilizSkCarbItalia += rif.QtaCarb_SkCarburantiDati.GetValueOrDefault(0);
                                riepilogo.TotImportoContabilizSkCarbItalia += rif.ImpoSiIvaSiScoMag_SkCarburantiDati.GetValueOrDefault(0);
                            }
                        }
                        else
                        {
                            riepilogo.TotSkCarbEstero += rif.QtaCarb_SkCarburantiDati.GetValueOrDefault(0);
                            if (!string.IsNullOrWhiteSpace(rif.AnnoMeseContab_SkCarburantiDati))
                            {
                                riepilogo.TotContabilizSkCarbEstero += rif.QtaCarb_SkCarburantiDati.GetValueOrDefault(0);
                                riepilogo.TotImportoContabilizSkCarbEstero += rif.ImpoSiIvaSiScoMag_SkCarburantiDati.GetValueOrDefault(0);
                            }
                        }
                    }
                }

                if (annoRif == 2018)
                {
                    riepilogo.TotContabiliz = rifs
                        .Where(n => ((n.AnnoMeseContab_SkCarburantiDati == "201805" ||
                            n.AnnoMeseContab_SkCarburantiDati == "201808" ||
                            n.AnnoMeseContab_SkCarburantiDati == "201810" ||
                            n.AnnoMeseContab_SkCarburantiDati == "201902") &&
                            string.IsNullOrWhiteSpace(n.Documento_SkCarburantiDati)))
                            .Sum(n => n.QtaCarb_SkCarburantiDati).GetValueOrDefault(0);
                    riepilogo.TotFuelCard -= riepilogo.TotContabiliz;
                }
                DateTime DecorrenzaInsSkCarb = new DateTime();
                DateTime TermineInsSkCarb = new DateTime();
                checkAuth_InsertEditSkCarb(annoRif, out DecorrenzaInsSkCarb, out TermineInsSkCarb);
                riepilogo.InizioPeriodoInsertSkCarb = DecorrenzaInsSkCarb;
                riepilogo.FinePeriodoInsertSkCarb = TermineInsSkCarb;
                riepilogo.AuthInsertSkCarb = checkAuth_SkCarb(matricola, annoRif);
                riepilogo.Anno = annoRif;
                return riepilogo;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private List<Rifornimento> getModel_StoricoRifornimenti(int annoRif, string matricola, int statoLogico = 0)
        {
            List<Rifornimento> RifornimentiAnno = new List<Rifornimento>();
            try
            {
                DateTime DecorrenzaInsSkCarb = new DateTime();
                DateTime TermineInsSkCarb = new DateTime();
                bool AuthToEditSkCarb = checkAuth_InsertEditSkCarb(annoRif, out DecorrenzaInsSkCarb, out TermineInsSkCarb);

                foreach (var rif in getRifornamenti(annoRif, matricola, statoLogico))
                {
                    Rifornimento dettagliRif = convertSkCarbDati_ToRif(rif, AuthToEditSkCarb);
                    RifornimentiAnno.Add(dettagliRif);
                }
                return RifornimentiAnno;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private SchedaCarburante getModel_ModificaSchedaCarburante(int annoRif, string matricola, DateTime dataRifToEdit)
        {
            SchedaCarburante model = new SchedaCarburante() { Anno = annoRif };
            try
            {
                using (HRPADBEntities db = new HRPADBEntities())
                {
                    var rifSelez = db.T_SkCarburantiDati
                        .FirstOrDefault(n => n.Anno_SkCarburantiDati == annoRif &&
                            n.Matricola_SkCarburantiDati == matricola &&
                            n.DataTransazione_SkCarburantiDati == dataRifToEdit &&
                            n.Attivo_SkCarburantiDati == 0);

                    if (rifSelez == null)
                    { throw new InvalidOperationException("Scheda carburante non trovata"); }

                    var rifSkCarb = db.T_SkCarburantiDati
                        .Where(n => n.Anno_SkCarburantiDati == annoRif &&
                            n.Matricola_SkCarburantiDati == matricola &&
                            n.Attivo_SkCarburantiDati == 0 &&
                            n.Documento_SkCarburantiDati == rifSelez.Documento_SkCarburantiDati)
                        .OrderBy(n => n.DataTransazione_SkCarburantiDati).ToList();

                    if (rifSkCarb.All(n => n.Stato_SkCarburantiDati != "Inserito"))
                    { throw new InvalidOperationException("Scheda carburante non modificabile"); }

                    if (rifSkCarb.Any(n => n.Documento_SkCarburantiDati == null || n.TipoDoc_SkCarburantiDati == null))
                    { throw new InvalidOperationException("Scheda carburante non modificabile, nessun allegato associato"); }

                    var allegato = db.T_SkCarburantiDoc.FirstOrDefault(n => n.Chiave_SkCarburantiDoc == rifSelez.Documento_SkCarburantiDati);

                    if (allegato == null || allegato.Documento_SkCarburantiDoc == null || RifornimentiConfig.EstensioniFilePermesse.Contains(allegato.TipoDoc_SkCarburantiDoc))
                    { throw new InvalidOperationException("Scheda carburante non modificabile, allegato non valido"); }

                    model.idxEvidenziaRif = rifSkCarb.IndexOf(rifSelez);
                    model.IdSkCarb = rifSelez.Documento_SkCarburantiDati;
                    model.FileNameSkCarb = rifSelez.TipoDoc_SkCarburantiDati;
                    model.TipoCarb = rifSelez.TipoCarb_SkCarburantiDati;
                    model.TipoSkCarb = rifSelez.TipoSkCar_SkCarburantiDati;
                    model.TargaAssociata = rifSelez.Targa_SkCarburantiDati;

                    foreach (var rif in rifSkCarb)
                    {
                        Rifornimento dettagliRif = convertSkCarbDati_ToRif(rif, true);
                        model.RifornimentiInseriti.Add(dettagliRif);
                    }
                }
                return model;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
        }

        /// <summary>
        /// Ottiene i rifornimenti carburante fatti da una matricola Rai nell'anno contabile specificato.
        /// </summary>
        /// <param name="targetYear">Anno contabile di riferimento.</param>
        /// <param name="targetMatricola">Matricola Rai.</param>
        /// <param name="targetStato">Stato logico del dato, stato >0 significa rifornimenti cancellati logiamente.</param>
        /// <returns></returns>
        private static IEnumerable<T_SkCarburantiDati> getRifornamenti(int targetYear, string targetMatricola, int targetStato = 0)
        {
            try
            {
                using (HRPADBEntities db = new HRPADBEntities())
                {
                    return db.T_SkCarburantiDati
                        .Where(n =>
                            n.Anno_SkCarburantiDati == targetYear &&
                            n.Matricola_SkCarburantiDati == targetMatricola &&
                            n.Attivo_SkCarburantiDati == targetStato)
                        .OrderByDescending(n => n.DataTransazione_SkCarburantiDati)
                        .ToArray();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Permette di convertire l'entita rifornimento salvata nel db in un oggetto con i dati utili alla sezione Rifornimenti di MyRai.
        /// </summary>
        /// <param name="rifdb">Tupla della Tabella T_SkCarburantiDati del db HRPA.</param>
        /// <param name="authToEditSkCarb">Se true i dati appartengono a una scheda carburante modificabile.</param>
        /// <returns>Oggetto Rifornimento con i dati utili alla sezione Rifornimenti di MyRai.</returns>
        private Rifornimento convertSkCarbDati_ToRif(T_SkCarburantiDati rifdb, bool authToEditSkCarb)
        {
            try
            {
                Rifornimento rif = new Rifornimento();
                rif.Id_Anno = rifdb.Anno_SkCarburantiDati;
                rif.Id_DataTransaz = rifdb.DataTransazione_SkCarburantiDati;
                rif.Id_StatoLogico = rifdb.Attivo_SkCarburantiDati;
                rif.Data = rifdb.DataTransazione_SkCarburantiDati.Date;
                rif.Orario = rifdb.DataTransazione_SkCarburantiDati.TimeOfDay;
                rif.Nazione = rifdb.Paese_SkCarburantiDati;
                rif.ContaKm = rifdb.Chilometri_SkCarburantiDati.ToString();
                rif.Importo = rifdb.ImpoSiIvaSiScoMag_SkCarburantiDati.HasValue ? rifdb.ImpoSiIvaSiScoMag_SkCarburantiDati.Value.ToString(CultureInfo.InvariantCulture) : "";
                rif.Quantita = rifdb.QtaCarb_SkCarburantiDati.HasValue ? rifdb.QtaCarb_SkCarburantiDati.Value.ToString(CultureInfo.InvariantCulture) : "";
                rif.Carburante = rifdb.TipoCarb_SkCarburantiDati;
                rif.Dove = rifdb.ImpiantoServ_SkCarburantiDati;
                rif.Targa = rifdb.Targa_SkCarburantiDati;
                rif.NumScheda = rifdb.NumCarta_SkCarburantiDati;
                rif.NumRicevuta = rifdb.Documento_SkCarburantiDati;
                rif.StatoLavorazione = rifdb.Stato_SkCarburantiDati;
                rif.DataContabilizzazione = rifdb.AnnoMeseContab_SkCarburantiDati;
                rif.AuthInsEditSkCarb = authToEditSkCarb;
                return rif;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Utile a nascondere la sezione rifornimenti dalla sidebar di MyRai agli utenti non dirigenti o che non hanno rifornimenti recenti.
        /// </summary>
        /// <param name="MatricolaToCheck">Matricola dell'utente RAI su cui fare le verifiche.</param>
        /// <returns>Ritorna true se l'utente è autorizzato a vedere la sezione "Rifornimenti".</returns>
        public static bool checkAuth_VedereRifornimenti(string MatricolaToCheck)
        {
            try
            {
                bool? auth = SessionHelper.Get<bool?>("RifornimentiAuth",  ()=>{ return null; });
                if (auth == null)
                {
                    auth = checkAuth_FuelCard(MatricolaToCheck) || checkAuth_HaRifUltimi3Anni(MatricolaToCheck);
                    SessionHelper.Set("RifornimentiAuth",auth);
                }
                if (auth == true) { return true; } else { return false; }
            }
            catch (Exception)
            {
                SessionHelper.Set("RifornimentiAuth", false);
                return false;
                throw new InvalidOperationException("Impossibile stabilire se l'utente è autorizzato al servizio");
            }
        }

        /// <summary>Controlla se è possibile effettuare operazioni CRUD sui dati inerenti alle schede carburante.</summary>
        /// <param name="MatricolaToCheck">Matricola dell'operatore.</param>
        /// <param name="annoRif">Anno contabile in cui si vorrebbe operare.</param>
        /// <returns>Ritorna True se la matricola è autorizzata a modificare i dati dell'anno contabile richiesto.</returns>
        public static bool checkAuth_SkCarb(string MatricolaToCheck, int annoRif)
        {
            DateTime DecorrenzaInsSkCarb = new DateTime();
            DateTime TermineInsSkCarb = new DateTime();
            return checkAuth_FuelCard(MatricolaToCheck) && checkAuth_InsertEditSkCarb(annoRif, out DecorrenzaInsSkCarb, out TermineInsSkCarb);
        }

        /// <summary>
        /// Verifica se la matricola è assegnataria del servizio FuelCard attivo nella dta odierna o in una data specificata.
        /// </summary>
        /// <param name="MatricolaToCheck">Matricola su cui fare le verifiche.</param>
        /// <param name="dataToCheck">Data in cui si vuole conoscere se il servizio Fuel Card è attivo.</param>
        /// <returns>Ritorna true se l'utente è assegnatario di una Fuel Card attiva nella data indicata.</returns>
        public static bool checkAuth_FuelCard(string MatricolaToCheck, DateTime? dataToCheck = null)
        {
            try
            {
                if (dataToCheck == null) { dataToCheck = DateTime.Now; }
                using (HRPADBEntities db = new HRPADBEntities())
                {
                    return db.T_SkCarburantiAssegnatari.Any(n =>
                                n.matricola_SkCarburantiAssegnatari == MatricolaToCheck &&
                                n.decorrenza_SkCarburantiAssegnatari <= dataToCheck &&
                                n.scadenza_SkCarburantiAssegnatari >= dataToCheck);
                }
            }
            catch (Exception)
            {
                return false;
                //throw new InvalidOperationException("Impossibile stabilire se l'utente è autorizzato al servizio");
            }
        }

        /// <summary>
        /// Controlla se la matricola ha eseguito almeno un rifornimento di carburante negli ultimi 3 Anni (se oggi è <Giugno) o 2 anni (se oggi >Giugno).
        /// </summary>
        /// <param name="matricolaToCheck">Matricola su cui fare le verifiche.</param>
        /// <returns>True se c'è almeno un rifornimento negli ultimi 2 o 3 anni.</returns>
        private static bool checkAuth_HaRifUltimi3Anni(string matricolaToCheck)
        {
            try
            {
                DateTime inizioRangeRiF = new DateTime(DateTime.Now.Year - 2, 1, 1);
                if (DateTime.Now.Month < 6) { inizioRangeRiF.AddYears(-1); }

                using (HRPADBEntities db = new HRPADBEntities())
                {
                    return db.T_SkCarburantiDati.Any(n =>
                                n.Matricola_SkCarburantiDati == matricolaToCheck &&
                                n.DataTransazione_SkCarburantiDati >= inizioRangeRiF &&
                                n.Attivo_SkCarburantiDati == 0);
                }
            }
            catch (Exception)
            {
                return false;
                //throw new InvalidOperationException("Impossibile stabilire se l'utente è autorizzato al servizio");
            }
        }

        /// <summary>
        /// Ottiene le date di decorrenza e termine del serzivio di editing delle schede carburante.
        /// </summary>
        /// <param name="annoRif">Anno contabile di riferimento</param>
        /// <param name="decorrenza">Data a partire dal quale è possibile inserire, modificare o cancellare le schede carburante.</param>
        /// <param name="scadenza">Data oltre il quale non è più possibile inserire, modificare o cancellare le schede carburante.</param>
        /// <returns>Ritorna True se la data odierna e nel range fra la date di Decorrenza e la data di Scadenza del servizio.</returns>
        private static bool checkAuth_InsertEditSkCarb(int annoRif, out DateTime decorrenza, out DateTime scadenza)
        {
            try
            {
                using (HRPADBEntities db = new HRPADBEntities())
                {
                    string dataStart = db.T_SkCarburantiParam.FirstOrDefault(n =>
                        n.Anno_SkCarburantiParam == annoRif &&
                        n.Chiave_SkCarburantiParam == "Inizio Inserimento").Valore_SkCarburantiParam;

                    string dataEnd = db.T_SkCarburantiParam.FirstOrDefault(n =>
                        n.Anno_SkCarburantiParam == annoRif &&
                        n.Chiave_SkCarburantiParam == "Termine Inserimento").Valore_SkCarburantiParam;

                    decorrenza = DateTime.ParseExact(dataStart, "yyyyMMdd", CultureInfo.InvariantCulture);
                    scadenza = DateTime.ParseExact(dataEnd, "yyyyMMdd", CultureInfo.InvariantCulture);

                    return (decorrenza < DateTime.Now && scadenza > DateTime.Now);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Impossibile determinare le date di decorrenza/scadenza del servizio Rifornimenti");
            }
        }

        /// <summary>
        /// Valida l'anno contabile richiesto e lo converte in un numero intero.
        /// </summary>
        /// <param name="targetYear">Anno contabile di riferimento.</param>
        /// <returns>Anno contabile come Integer.</returns>
        private int checkParse_AnnoRif(string targetYear)
        {
            try
            {
                int year = int.Parse(targetYear);
                if (year >= 2018 && year <= DateTime.Now.Year)
                {
                    return year;
                }
                throw new InvalidOperationException("periodo richiesto fuori dai limiti");
            }
            catch (Exception)
            {
                throw new InvalidOperationException("periodo richiesto non valido");
            }
        }

        public ActionResult getSchedaCarburante(string TipoSkCarb, string TipoCarb, string Targa, string AnnoRif)
        {
            byte[] tipoPdf;
            var matricola = CommonHelper.GetCurrentUserMatricola();
            string user = CommonHelper.GetNominativoPerMatricola(matricola);

            using (var db = new HRPADBEntities())
            {
                var entity = db.T_SkCarburantiAssegnatari.FirstOrDefault(x => x.matricola_SkCarburantiAssegnatari == matricola);

                if (!string.IsNullOrWhiteSpace(TipoSkCarb) ||
                    !string.IsNullOrWhiteSpace(TipoCarb) ||
                    !string.IsNullOrWhiteSpace(Targa)
                    )
                {
                    using (var db2 = new digiGappEntities())
                    {
                        tipoPdf = db2.MyRai_Moduli.FirstOrDefault(x => x.destinatario == entity.societa_SkCarburantiAssegnatari + TipoSkCarb).bytes_content;
                    }
                }
                else
                {
                    return new HttpStatusCodeResult(400, "Campi obbligatori non presenti");
                }
            }

            using (MemoryStream ms = new MemoryStream())
            {

                PdfReader pdfReader = new PdfReader(tipoPdf);
                PdfStamper pdfStamper = new PdfStamper(pdfReader, ms);
                AcroFields pdfFormFields = pdfStamper.AcroFields;

                foreach (var de in pdfFormFields.Fields)
                {
                    Console.WriteLine(de.Key.ToString());
                }

                pdfFormFields.SetField("pdfANNO", AnnoRif);
                pdfFormFields.SetField("pdfMATRICOLA", matricola);
                pdfFormFields.SetField("pdfNOMINATIVO", user);
                RifornimentiConfig.TipiSkCarburanteEnum tipoCarbEnum = (RifornimentiConfig.TipiSkCarburanteEnum)Enum.Parse(typeof(RifornimentiConfig.TipiSkCarburanteEnum), TipoSkCarb, true);
                pdfFormFields.SetField("pdfTIPOSCHEDA", tipoCarbEnum.GetDisplayAttributeFrom(typeof(RifornimentiConfig.TipiSkCarburanteEnum)));
                pdfFormFields.SetField("pdfTARGA", Targa.ToUpperInvariant());
                pdfFormFields.SetField("pdfTIPOCARBURANTE", TipoCarb);

                // Questa istruzione consente di rimuovere le potenzialità di editing del form, risultando
                // in un PDF non più modificabile dall'utente, se si imposta a false, il PDF risultante sarà 
                // compilato ma comunque editabile dall'utente
                pdfStamper.FormFlattening = true;

                pdfStamper.Close();
                Response.AddHeader("Content-Disposition", "inline; filename=" + "SchedaRendicontazione_" + AnnoRif + "_" + matricola + ".pdf");

                return File(ms.ToArray(), "application/pdf");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="destinatario"></param>
        /// <param name="sedeGAPP"></param>
        private static void InvioMail (string matricola = "", string nomeUtente = "" )
        {
            try
            {
                string[] MailParams = CommonHelper.GetParametri<string>( EnumParametriSistema.MailTemplateNotificaRifornimento );

                string body = MailParams[0];
                string MailSubject = MailParams[1];

                string[] mailParams2 = CommonHelper.GetParametri<string>( EnumParametriSistema.MailTemplateNotificaRifornimentoFromTo );
                string from = mailParams2[0];
                string to = mailParams2[1];

                GestoreMail mail = new GestoreMail( );

                //to = "RUO.SIP.PRESIDIOOPEN@rai.it";
                body = body.Replace( "#NOMEUTENTE#", nomeUtente );
                body = body.Replace( "#MATRICOLA#" , matricola );
                body = body.Replace( "#DATA#" , DateTime.Now.ToString("dd/MM/yyyy") );
                var response = mail.InvioMail( body, MailSubject , to , "raiplace.selfservice@rai.it" , from, null, null );
                //var response = mail.InvioMail( body , MailSubject , to , "francesco.buonavita80@gmail.com" , from );

                if ( response != null && !String.IsNullOrEmpty( response.Errore ) )
                {
                    myRaiCommonDatacontrollers.Logger.LogErrori( new MyRai_LogErrori( )
                    {
                        applicativo = "Portale" ,
                        data = DateTime.Now ,
                        provenienza = "Rifornimenti - InvioMail" ,
                        error_message = response.Errore + " per " + to ,
                        feedback = null ,
                        matricola = matricola
                    } );
                }
            }
            catch ( Exception ex )
            {
                myRaiCommonDatacontrollers.Logger.LogErrori( new MyRai_LogErrori( )
                {
                    applicativo = "Portale" ,
                    data = DateTime.Now ,
                    provenienza = "Rifornimenti - InvioMail" ,
                    error_message = ex.ToString( ) ,
                    feedback = null ,
                    matricola = matricola
                } );
            }
        }
    }
}