using System;
using System.Collections.Generic;
using System.Linq;
using myRaiCommonModel.RaiAcademy;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using myRaiDataTalentia;
using System.Linq.Expressions;
using myRaiHelper;
using myRai.Data.CurriculumVitae;

namespace myRaiCommonModel.DataControllers.RaiAcademy
{
    public class RaiAcademyDataController
    {
        private string GetDurataString(decimal value, string unit)
        {
            string result = "";

            TimeSpan t = new TimeSpan();
            if (unit == "O")
                t = TimeSpan.FromHours(Convert.ToDouble(value));
            else if (unit == "M")
                t = TimeSpan.FromMinutes(Convert.ToDouble(value));

            if (t.TotalMinutes > 0)
            {
                int hours = t.Days * 24 + t.Hours;
                int minutes = t.Minutes;

                if (hours == 1)
                    result += "1 ora";
                else if (hours > 1)
                    result += hours + " ore";

                if (hours > 0 && minutes > 0)
                    result += " e ";

                if (minutes == 1)
                    result += "1 minuto";
                else if (minutes > 1)
                    result += minutes + " minuti";

                //if (t.Hours == 1)
                //    result += "1 ora";
                //else if (t.Hours > 1)
                //    result += t.Hours + " ore";

                //if (t.Hours > 0 && t.Minutes > 0)
                //    result += " e ";

                //if (t.Minutes == 1)
                //    result += "1 minuto";
                //else if (t.Minutes > 1)
                //    result += t.Minutes + " minuti";
            }

            return result;
        }
        private string RimuoviCodice(string codiceCompleto)
        {
            string result = codiceCompleto;

            if (result.Length > 6 && result[5] == '-')
                result = result.Substring(7);

            return result;
        }

        /// <summary>
        /// Reperimento dei corsi in base ai parametri passati
        /// </summary>
        /// <param name="tipoCorso">Filtra in base ai possibili valori: All, Iniziati, Obbligatori,  MiIncuriscono, Consigliati, Attinenti</param>
        /// <param name="pageSize">Numero massimo di elementi restituiti per pagina</param>
        /// <param name="requestedPage">Pagina richiesta</param>
        /// <param name="attivi">Null, reperisce tutti i corsi. True, reperisce soltanto i corsi attivi. False, reperisce soltato i corsi chiusi</param>
        /// <returns></returns>
        public GetCorsiResult GetCorsi(string matricola, bool loadAllInfo)
        {
            //TalentiaEntities db = new TalentiaEntities();
            GetCorsiResult result = new GetCorsiResult();
            try
            {
                result.List = EstraiCorsi(matricola, false);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return result;
        }


        /// <summary>
        /// Reperimento dei corsi Consigliati in base al percorso formativo del corso selezionato
        /// </summary>
        /// <param name="idCorso">Identificativo univoco del corso per il quale reperire la lista dei corsi attinenti</param>
        /// <param name="pageSize">Numero massimo di record restituiti</param>
        /// <param name="requestedPage">Pagina richiesta</param>
        /// <returns></returns>
        public List<Corso> GetCorsiConsigliati(string matricola, int idCorso, int pageSize = 3, int requestedPage = 1, bool previewCorso = false)
        {
            TalentiaEntities db = new TalentiaEntities();
            List<Corso> corsi = new List<Corso>();

            try
            {
                int toSkip = 0;
                int toTake = pageSize;
                if (requestedPage > 1)
                {
                    toSkip = (requestedPage - 1) * pageSize;
                }

                Corso corso = EstraiCorsi(matricola, false, idCorso, true, !previewCorso, StatoPubblicazione.NonDefinito).First();

				var percorsiFormativi = corso.AltriDettagli.PercorsiFormativi.Select(x => x.Id);
                var tmp = EstraiCorsi(matricola, false, idCorso, false).Where(x => x.AltriDettagli.PercorsiFormativi.Any(y => percorsiFormativi.Contains(y.Id)));

                Random rnd = null;
                List<int> usedI = new List<int>();
                int itemCount = tmp.Count();
                int maxI = toTake > 0 ? toTake : itemCount;
                if (toTake > 0)
                    rnd = new Random();

                for (int i = 0; i < toTake; i++)
                {
                    int index = 0;
                    if (toTake > 0)
                    {
                        do index = rnd.Next(0, itemCount);
                        while (usedI.Contains(index));
                        usedI.Add(index);
                    }
                    else
                    {
                        index = i;
                    }
                    corsi.Add(tmp.ElementAt(index));
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return corsi;
        }

        /// <summary>
        /// Reperimento dei corsi attinenti ad un determinato corso
        /// </summary>
        /// <param name="idCorso">Identificativo univoco del corso per il quale reperire la lista dei corsi attinenti</param>
        /// <param name="pageSize">Numero massimo di record restituiti</param>
        /// <param name="requestedPage">Pagina richiesta</param>
        /// <returns></returns>
        public List<Corso> GetCorsiAttinenti(string matricola, int idCorso, int area, int pageSize = 3, int requestedPage = 1, bool isPreview = false)
        {
            TalentiaEntities db = new TalentiaEntities();
            List<Corso> corsi = new List<Corso>();

            try
            {
                int toSkip = 0;
                int toTake = pageSize;
                if (requestedPage > 1)
                {
                    toSkip = (requestedPage - 1) * pageSize;
                }

                int tipologia = -1;

                var tmp = EstraiCorsi(matricola, false, idCorso, false, filter: x => x.ID_TEMA == area);

                Random rnd = null;
                List<int> usedI = new List<int>();
                int itemCount = tmp.Count();
                int maxI = toTake > 0 ? toTake : itemCount;
                if (toTake > 0)
                    rnd = new Random();

                for (int i = 0; i < toTake; i++)
                {
                    int index = 0;
                    if (toTake > 0)
                    {
                        do index = rnd.Next(0, itemCount);
                        while (usedI.Contains(index));
                        usedI.Add(index);
                    }
                    else
                    {
                        index = i;
                    }
                    corsi.Add(tmp.ElementAt(index));
                }

                //corsi = EstraiCorsi(false, idCorso, false).Where(x=>x.AreaFormativa==corso.AreaFormativa).Skip(toSkip).Take(toTake).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return corsi;
        }
        
        public List<Corso> EstraiCorsi(string matricola, bool loadAllInfo, int idCorso = 0, bool criterioUguale = true, bool addAccessoCorso = false, StatoPubblicazione statoPubblicazione = StatoPubblicazione.Pubblicato, List<int> listCorsi = null, Expression<Func<CORSO, bool>> filter = null)
        {
            TalentiaEntities db = new TalentiaEntities();

            if (addAccessoCorso)
            {
                var rec = db.XR_ACA_PARAM.FirstOrDefault(x => x.COD_PARAM == "LogAccessiCorso");
                if (rec == null || rec.COD_VALUE1 != "TRUE")
                    addAccessoCorso = false;
            }

            int idPersona = 0;
            var tmpPersona = db.V_XR_XANAGRA.FirstOrDefault(x => x.MATRICOLA == matricola);
            if (tmpPersona != null)
                idPersona = tmpPersona.ID_PERSONA;

            var tmpInt = db.XR_CORSOINTPERSONA.Where(x => x.MATRICOLA == matricola);
            var tmpCurrForm = db.CURRFORM.Where(x => x.ID_PERSONA == idPersona);


            List<Corso> corsiList = new List<Corso>();
            
            string baseUrl = System.Web.HttpContext.Current.Request.Url.Authority;

            IQueryable<CORSO> query = null;

            //Il primo caso è relativo alla query per il dettaglio del corso
            if (idCorso>0 && criterioUguale)
            {
                query = db.CORSO.Include("TB_TEMA").Include("TB_TPCORSO").Include("TB_MTDDID").Include("EDIZIONE").Include("GENNOTES").Include("CCATALOGUEITEM").Include("CZNDOCS")
                    .Where(x => true);
            }
            else if (loadAllInfo)
            {
                query = db.CORSO.Include("TB_TEMA").Include("TB_TPCORSO").Include("TB_MTDDID").Include("EDIZIONE").Include("GENNOTES").Include("CCATALOGUEITEM")
                    .Where(x => true);
            }
            else
            {
                query = db.CORSO.Include("TB_TEMA").Include("TB_TPCORSO").Include("TB_MTDDID").Include("EDIZIONE").Include("CCATALOGUEITEM")//Include("CZNDOCS").
                    .Where(x => true);
            }

            query = query.Where(x => x.IND_ATTIVO == "Y" && (x.IND_OBBLIGATORIO == "1" || x.IND_OBBLIGATORIO == "2" || x.IND_OBBLIGATORIO == "3"));

            if (statoPubblicazione == StatoPubblicazione.Pubblicato)
                query = query.Where(x => x.IND_TUTORSHIP == "Y");

            if (idCorso>0)
            {
                if (criterioUguale)
                {
                    query = query.Where(x => x.ID_CORSO == idCorso);
                    tmpInt = tmpInt.Where(x => x.ID_CORSO == idCorso);
                    tmpCurrForm = tmpCurrForm.Where(x => x.EDIZIONE.ID_CORSO == idCorso);
                }
                else
                {
                    query = query.Where(x => x.ID_CORSO != idCorso);
                    tmpInt = tmpInt.Where(x => x.ID_CORSO != idCorso);
                    tmpCurrForm = tmpCurrForm.Where(x => x.EDIZIONE.ID_CORSO != idCorso);
                }

            }

            if (listCorsi!=null && listCorsi.Count()>0)
            {
                query = query.Where(x => listCorsi.Contains(x.ID_CORSO));
                tmpInt = tmpInt.Where(x => listCorsi.Contains(x.ID_CORSO));
                tmpCurrForm = tmpCurrForm.Where(x => listCorsi.Contains(x.EDIZIONE.ID_CORSO));
            }

            if (filter != null)
                query = query.Where(filter);

            query = query.OrderBy(x => x.COD_CORSO);

            if (query.Count() > 0)
            {

                var listaInteressi = tmpInt.ToList();
                var listaCurrForm = tmpCurrForm.ToList();

                foreach (var item in query)
                {
                    CZNDOCS immagine = null;

                    if (loadAllInfo && idCorso > 0 && criterioUguale)
                        immagine = item.CZNDOCS.FirstOrDefault(x => x.COD_EXTENSION == ".jpg");

                    Corso corso = new Corso();
                    corso.Id = item.ID_CORSO;
                    corso.IdAreaFormativa = item.ID_TEMA;
                    corso.AreaFormativa = item.TB_TEMA.COD_TEMA;
                    corso.MetodoFormativo = item.TB_MTDDID.COD_METODODID;
                    corso.DesMetodoFormativoStr = item.TB_MTDDID.DES_METODODID;
                    corso.IdTematica = item.ID_TPCORSO;
                    corso.Tematica = item.TB_TPCORSO.COD_TIPOCORSO;
                    corso.Titolo = item.COD_CORSO;
                    corso.Immagine = immagine != null ? immagine.OBJ_OBJECT : null;
                    corso.ImmagineDidascalia = immagine != null ? immagine.DES_DESCRIPTION : null;
                    corso.StatoStr = item.IND_OBBLIGATORIO;
                    corso.MiIncuriosisce = listaInteressi.Any(x => x.ID_CORSO == item.ID_CORSO);
                    corso.Sede = item.DES_LOCATION ?? "";
                    corso.Descrizione = item.DES_CORSO ?? "";
                    corso.Target = item.NOT_CONSIGLIATO ?? "";
                    corso.NumeroPartecipanti = item.NMB_MAXATTENDANCES;
                    corso.NomePortale = item.DES_LOCATION ?? "";
                    corso.QtaDurata = item.QTA_DURATA;
                    corso.UdmDurata = item.COD_UNITATEMPO;
                    corso.CertificazioneOttenuta = item.NOT_CERTIF ?? "";
                    corso.DisponibileDal = item.NOT_DISPONIBILE ?? "";
                    corso.Gruppo = item.ID_DOMITEMGRUPPO.HasValue && item.ID_DOMITEMGRUPPO.Value > 0 ? item.DES_DOMGRUPPO : "";
                    corso.ExternalCourse = item.COD_CORSOLMS;
                    corso.ObiettiviEContenuti = new ObiettiviEContenuti()
                    {
                        Serve = item.DES_PREREQ2 ?? "",
                        Imparerai = item.DES_CONTENT ?? ""
                    };
                    corso.AltriDettagli = new AltriDettagli()
                    {
                        ArticolazioneCorso = item.DES_PREREQ3 ?? "",
                        Docenti = item.NOT_DOCENTE ?? "",
                        Note = item.NOT_NOTA ?? "",
                        Requisiti = item.DES_PREREQ1 ?? ""
                    };
                    
                    if (item.CCATALOGUEITEM.Count() > 0)
                    {
                        var tmpCCat = item.CCATALOGUEITEM.Where(x =>x.CCATALOGUE.COD_STATUS == "1");
                        foreach (var itemCCat in tmpCCat)
                        {
                            corso.AltriDettagli.PercorsiFormativi.Add(new PercorsoFormativo()
                            {
                                Id = itemCCat.ID_CCATALOGUE,
                                Nome = itemCCat.CCATALOGUE.NME_CCATALOGUE
                            });
                        }
                    }


                    corso.TipoMetodoFormativo = GetTipoMetodoDidattico(corso.DesMetodoFormativoStr);
                    corso.Durata = GetDurataString(corso.QtaDurata, corso.UdmDurata);
                    corso.Stato = GetEnumStatoCorso(corso.StatoStr);
                    corso.AltriDettagli.Edizioni = new List<Edizione>();

                    //verifico la presenza di pillole
                    if (loadAllInfo && corso.TipoMetodoFormativo == MetodoEnum.FAD)
                    {
                        if (item.GENNOTES.Any(x => x.COD_GRPNOTE == "L001" && x.COD_TPNOTE == "SCORM"))
                        {
                            corso.Pacchetto = new Pacchetto();
                            corso.Pacchetto.IdCorso = corso.Id;
                            foreach (var scorm in item.GENNOTES.Where(x => x.COD_GRPNOTE == "L001" && x.COD_TPNOTE == "SCORM"))
                            {
                                Risorsa ris = new Risorsa();
                                string[] elems = scorm.NOT_NOTE.Split('|');
                                if (elems.Length >= 3)
                                {
                                    ris.Prog = !String.IsNullOrWhiteSpace(elems[0]) ? Convert.ToInt32(elems[0]) : 0;
                                    ris.Nome = elems[1];
                                    ris.Url = elems[2];
                                    ris.Abilitato = true;
                                    ris.VincoloEdizione = elems.Length > 3 ? elems[3] : null;
                                    corso.Pacchetto.Pillole.Add(ris);
                                }
                            }
                        }
                    }

                    //Risorse
                    if (loadAllInfo && item.GENNOTES.Any(x => x.COD_GRPNOTE == "L001" && x.COD_TPNOTE == "MATCOR"))
                    {
                        corso.Risorse = new Materiali();
                        corso.Risorse.IdCorso = corso.Id;

                        foreach (var matcor in item.GENNOTES.Where(x => x.COD_GRPNOTE == "L001" && x.COD_TPNOTE == "MATCOR"))
                        {
                            Risorsa ris = new Risorsa();
                            string[] elems = matcor.NOT_NOTE.Split('|');
                            if (elems.Length >= 3)
                            {
                                ris.Prog = Convert.ToInt32(elems[0]);
                                ris.Nome = elems[1];
                                ris.Url = elems[2];
                                ris.Abilitato = true;
                                ris.VincoloEdizione = elems.Length > 3 ? elems[3] : null;
                                corso.Risorse.Risorse.Add(ris);
                            }

                            //ris.Prog = ++prog;
                            //ris.Nome = czndocs.DES_DESCRIPTION ?? "";
                            //if (String.IsNullOrWhiteSpace(ris.Nome))
                            //    ris.Nome = "Risorsa " + prog.ToString();
                            //ris.Tipo = "doc";
                            //ris.Url = "/RaiAcademy/GetRisorsa?id=" + czndocs.ID_DOC.ToString();
                        }
                    }

                    var tmp = item.EDIZIONE.Where(x => x.DTA_INIZIO >= DateTime.Now || x.DTA_FINE >= DateTime.Now).OrderBy(y => y.DTA_INIZIO).Distinct();
                    if (tmp.Count() > 0)
                    {

                        var firstAvailable = tmp.FirstOrDefault(x => x.DTA_INIZIO >= DateTime.Now);

                        if (firstAvailable != null)
                        {
                            corso.DataInizioDisponibilita = firstAvailable.DTA_INIZIO;
                            corso.DisponibileDal = corso.DataInizioDisponibilita.Value.ToString("d MMMM yyyy");
                        }

                        if (loadAllInfo)
                        {
                            int progrEdiz = 0;
                            foreach (var dbEdiz in tmp)
                            {
                                Edizione ediz = new Edizione();
                                ediz.IdEdizione = dbEdiz.ID_EDIZIONE;
                                ediz.Nome = dbEdiz.COD_EDIZIONE;
                                ediz.DataInizio = dbEdiz.DTA_INIZIO;
                                ediz.DataFine = dbEdiz.DTA_FINE;
                                if (dbEdiz.EDITIONCALENDAR.Count > 0)
                                {
                                    ediz.FromHour = dbEdiz.EDITIONCALENDAR.First().NMB_FROMHOUR;
                                    ediz.FromMinute = dbEdiz.EDITIONCALENDAR.First().NMB_FROMMINUTE;
                                    ediz.ToHour = dbEdiz.EDITIONCALENDAR.First().NMB_TOHOUR;
                                    ediz.ToMinute = dbEdiz.EDITIONCALENDAR.First().NMB_TOMINUTE;
                                    ediz.Orario = String.Format("dalle {0}:{1:00} alle {2}:{3:00}", ediz.FromHour, ediz.FromMinute, ediz.ToHour, ediz.ToMinute);

                                    ediz.Giornate = dbEdiz.EDITIONCALENDAR.Select(x => new EdizioneGiornata() { Data = x.DTA_DATE, DaOra = x.NMB_FROMHOUR, DaMinuti = x.NMB_FROMMINUTE, AOra = x.NMB_TOHOUR, AMinuti = x.NMB_TOMINUTE }).ToList();

                                }
                                ediz.Luogo = dbEdiz.TRACENTER != null ? dbEdiz.TRACENTER.TB_COMUNE.DES_CITTA + " - " + dbEdiz.TRACENTER.DES_TRACENTER
                                                : dbEdiz.ENTE != null ? dbEdiz.ENTE.DES_ENTE : "";
                                ediz.DesLuogo = dbEdiz.TRACENTER != null ? dbEdiz.TRACENTER.DES_ADDRESS + (dbEdiz.TRACLASSROOM != null ? " - " + dbEdiz.TRACLASSROOM.DES_TRACLASSROOM : "") : "";

                                ediz.Note = dbEdiz.NOT_EDIZIONE;

                                ediz.Stato = dbEdiz.IND_STATOEDIZ;

                                //ediz.Nome = "Edizione " + ediz.Nome.Substring(0, 4) + " - " + ediz.Nome.Substring(4);
                                ediz.Nome = "Edizione " + (++progrEdiz).ToString();

                                var currform = listaCurrForm.FirstOrDefault(x => x.ID_EDIZIONE == dbEdiz.ID_EDIZIONE);

                                ediz.Iscritto = IscrittoEnum.NonIscritto;
                                //if (dbEdiz.CURRFORM.Any(x => x.ID_PERSONA == idPersona))
                                if (currform != null)
                                    ediz.Iscritto = IscrittoEnum.Iscritto;
                                else if (db.TREQUESTS.Any(x => x.ID_PERSONA == idPersona && x.ID_CORSO == corso.Id))
                                    ediz.Iscritto = IscrittoEnum.RichiestaInAttesa;

                                if (ediz.Iscritto == IscrittoEnum.Iscritto)
                                    corso.Completamento = Convert.ToInt32(currform.QTA_COMPLETION);

                                /*
                                 * Gli scorm possono essere associati ad una determinata edizione
                                 * in quanto potrebbero essere diversi i contenuti o le piattaforme
                                 */
                                if (corso.Pacchetto.Pillole.Any() && ediz.Iscritto!=IscrittoEnum.Iscritto)
                                    corso.Pacchetto.Pillole.RemoveWhere(x => x.VincoloEdizione != null && x.VincoloEdizione.Contains(dbEdiz.COD_EDIZIONE));

                                if (corso.Risorse.Risorse.Any() && ediz.Iscritto != IscrittoEnum.Iscritto)
                                    corso.Risorse.Risorse.RemoveWhere(x => x.VincoloEdizione != null && x.VincoloEdizione.Contains(dbEdiz.COD_EDIZIONE));

                                if (corso.TipoMetodoFormativo==MetodoEnum.FAD && corso.Stato==StatoCorsoTipoOffertaEnum.Obbligatoria && dbEdiz.COD_AUTHORIZATION=="M" && ediz.Iscritto!=IscrittoEnum.Iscritto)
                                {
                                    corso.Pacchetto.Pillole.RemoveWhere(x => true);
                                    corso.Risorse.Risorse.RemoveWhere(x => true);
                                }

                                corso.AltriDettagli.Edizioni.Add(ediz);
                            }
                        }
                        else
                        {
                            //var ediz = tmp.FirstOrDefault(x => x.CURRFORM.Any(y => y.ID_PERSONA == idPersona));
                            var ediz = tmp.FirstOrDefault(x => listaCurrForm.Any(y => y.ID_EDIZIONE == x.ID_EDIZIONE));
                            if (ediz != null)
                            {
                                //var currform = ediz.CURRFORM.FirstOrDefault(x => x.ID_PERSONA == idPersona);
                                var currform = listaCurrForm.FirstOrDefault(x => x.ID_EDIZIONE == ediz.ID_EDIZIONE);
                                if (currform != null)
                                {
                                    corso.Completamento = Convert.ToInt32(currform.QTA_COMPLETION);
                                    corso.DataInizio = currform.DTA_STARTEDON.GetValueOrDefault();
                                    if (currform.DTA_COMPLETEDON.HasValue)
                                        corso.DataFine = currform.DTA_COMPLETEDON.Value;
                                }
                            }
                        }
                    }
                    else
                    {
                        corso.DataInizioDisponibilita = new DateTime();
                    }

                    if (corso.AltriDettagli.Edizioni != null && corso.AltriDettagli.Edizioni.Count() > 0)
                        corso.AltriDettagli.NoteEdizioni = corso.AltriDettagli.Edizioni.Count() + " edizioni";
                    else
                    {
                        corso.AltriDettagli.NoteEdizioni = "Informazione non disponibile";
                        if (corso.TipoMetodoFormativo == MetodoEnum.FPRES)
                        {

                            switch (corso.Stato)
                            {
                                case StatoCorsoTipoOffertaEnum.SuRichiesta:
                                    corso.AltriDettagli.NoteEdizioni = "Calendario delle sessioni da definire in base alle esigenze rilevate";
                                    break;
                                case StatoCorsoTipoOffertaEnum.Obbligatoria:
                                    corso.AltriDettagli.NoteEdizioni = " Le date delle edizioni saranno comunicate ai diretti interessati in base al calendario definito";
                                    break;
                            }
                        }
                        else if (corso.TipoMetodoFormativo == MetodoEnum.FAD)
                        {
                            switch (corso.Stato)
                            {
                                case StatoCorsoTipoOffertaEnum.SuRichiesta:
                                    corso.AltriDettagli.NoteEdizioni = "Sempre disponibile";
                                    break;
                                case StatoCorsoTipoOffertaEnum.Aperta:
                                    corso.AltriDettagli.NoteEdizioni = "Sempre disponibile";
                                    break;
                                case StatoCorsoTipoOffertaEnum.Obbligatoria:
                                    corso.AltriDettagli.NoteEdizioni = "Sempre disponibile";
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                    if (loadAllInfo && corso.Stato == StatoCorsoTipoOffertaEnum.SuRichiesta)
                        corso.Iscritto = corso.Pacchetto.Iscritto = corso.Risorse.Iscritto =
                            corso.AltriDettagli.Edizioni.Any(x => x.Iscritto == IscrittoEnum.Iscritto) ? IscrittoEnum.Iscritto : corso.AltriDettagli.Edizioni.Any(x => x.Iscritto == IscrittoEnum.RichiestaInAttesa) ? IscrittoEnum.RichiestaInAttesa : IscrittoEnum.NonIscritto;
                    else
                        corso.Iscritto = corso.Pacchetto.Iscritto = corso.Risorse.Iscritto = corso.Completamento > 0 || (corso.DataInizio>DateTime.MinValue && corso.DataFine==DateTime.MinValue) ? IscrittoEnum.Iscritto : IscrittoEnum.NonIscritto;


                    //solo se sono entrato nel dettaglio
                    if (addAccessoCorso && idCorso > 0)
                    {
                        int idAccesso = db.XR_CORSOACCESSO.Count() == 0 ? 1 : db.XR_CORSOACCESSO.Max(x => x.ID_ACCESSO) + 1;
                        XR_CORSOACCESSO access = new XR_CORSOACCESSO()
                        {
                            ID_ACCESSO = idAccesso,
                            ID_CORSO = idCorso,
                            ID_TPCORSO = item.ID_TPCORSO,
                            ID_TEMA = item.ID_TEMA,
                            ID_PERSONA = idPersona,
                            MATRICOLA = matricola,
                            TMS_ULTIMOACCESSO = DateTime.Now
                        };
                        db.XR_CORSOACCESSO.Add(access);

                    }

                    corsiList.Add(corso);
                }

                if (addAccessoCorso && idCorso > 0)
                {
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }

            return corsiList;
        }

        public Corso GetDettaglioCorso(string matricola, int id, bool isPreview=false)
        {
            Corso corso = new Corso();

            try
            {
                corso = EstraiCorsi(matricola, true, id, true, !isPreview, StatoPubblicazione.NonDefinito).First();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return corso;
        }

        private StatoCorsoTipoOffertaEnum GetEnumStatoCorso(string value)
        {
            StatoCorsoTipoOffertaEnum result = StatoCorsoTipoOffertaEnum.NonDefinito;
            switch (value)
            {
                case "1":
                    result = StatoCorsoTipoOffertaEnum.SuRichiesta;
                    break;
                case "2":
                    result = StatoCorsoTipoOffertaEnum.Aperta;
                    break;
                case "3":
                    result = StatoCorsoTipoOffertaEnum.Obbligatoria;
                    break;
                default:
                    break;
            }

            return result;
        }

        public static MetodoEnum GetTipoMetodoDidattico(string desMetodo)
        {
            if (desMetodo != null)
            {
                if (desMetodo == "FPRES")
                    return MetodoEnum.FPRES;
                else if (desMetodo == "FAD")
                    return MetodoEnum.FAD;
                else
                {
                    return MetodoEnum.Altro;
                }
            }
            return MetodoEnum.NonDefinito;
        }

        public static MemoryStream GetCourseImage(int? idCorso)
        {
            if (idCorso.HasValue)
            {
                TalentiaEntities db = new TalentiaEntities();

                CZNDOCS docs = db.CZNDOCS.FirstOrDefault(x => x.ID_ENTITY == idCorso && x.COD_EXTENSION == ".jpg");

                if (docs != null)
                    return new MemoryStream(docs.OBJ_OBJECT);
                else
                    return GetDefaultCourseImage();
            }
            else
            {
                return GetDefaultCourseImage();
            }
        }

        private static Image Resize(Image image, int width, int height)
        {
            float scale;
            float scaleWidth = ((float)width / (float)image.Width);
            float scaleHeight = ((float)height / (float)image.Height);
            if (scaleHeight < scaleWidth)
            {
                scale = scaleHeight;
            }
            else
            {
                scale = scaleWidth;
            }

            int destWidth = (int)((image.Width * scale) + 0.5);
            int destHeight = (int)((image.Height * scale) + 0.5);

            Bitmap bitmap = new Bitmap(destWidth, destHeight, PixelFormat.Format24bppRgb);
            bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                graphics.DrawImage(image,
                    new Rectangle(0, 0, destWidth, destHeight),
                    new Rectangle(0, 0, image.Width, image.Height),
                    GraphicsUnit.Pixel);
            }
            return bitmap;
        }

        public static byte[] GetCourseImageResized(int? idCorso, bool resized=true)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            if (resized)
            {
                using (Image img = Image.FromStream(RaiAcademyDataController.GetCourseImage(idCorso)))
                {
                    Resize(img, 480, 210).Save(ms, ImageFormat.Jpeg);

                }
            }
            else
                ms = RaiAcademyDataController.GetCourseImage(idCorso);
            return ms.ToArray();
        }

        public static MemoryStream GetDefaultCourseImage()
        {
            return new MemoryStream(System.IO.File.ReadAllBytes(System.Web.HttpContext.Current.Server.MapPath("~/assets/img/logo_academy_home.png")));
        }

        public static List<V_CVCorsiRai> GetVCorsiFatti(string matricola)
        {
            List<V_CVCorsiRai> result = new List<V_CVCorsiRai>();

            if (CommonHelper.IsProduzione())
            {
                string statement = "exec [dbo].[Proc_CorsiDip] @pMatricola='" + matricola + "'";
                cv_ModelEntities db = new cv_ModelEntities();
                var tmp = db.Database.SqlQuery<tmpV_CVCorsiRai>(statement);
                if (tmp != null)
                    result.AddRange(tmp.Select(x => new V_CVCorsiRai()
                    {
                        matricola = x.matricola,
                        codice = x.codice,
                        DataInizioDate = x.DataInizioDate,
                        DataInizio = x.DataInizio,
                        DataFine = x.DataFine,
                        TitoloCorso = x.TitoloCorso,
                        Durata = Convert.ToInt32(x.Durata),
                        Societa = x.Societa,
                        flagImage = x.flagImage
                    }));
            }

            return result;
        }

        public List<Corso> GetCorsiFatti(string matricola)
        {
            //myRai.Data.CurriculumVitae.cv_ModelEntities cvEnt = new Data.CurriculumVitae.cv_ModelEntities();

            List<Corso> corsi = new List<Corso>();
            //foreach (var item in cvEnt.V_CVCorsiRai.Where(m => m.matricola == matricola))
            foreach (var item in GetVCorsiFatti(matricola))
            {
                Corso corso = new Corso();
                corso.Id = item.flagImage == 0 ? -1 : Convert.ToInt32(item.flagImage);
                corso.Titolo = item.TitoloCorso;
                corso.DataInizio = item.DataInizio != null ? DateTime.Parse(item.DataInizio) : DateTime.MinValue;
                corso.DataFine = item.DataFine != null ? DateTime.Parse(item.DataFine) : DateTime.MaxValue;
                corso.Societa = String.IsNullOrEmpty(item.Societa) ? "" : item.Societa.Trim();
                corsi.Add(corso);
            }
            return corsi;
        }
    }
}
