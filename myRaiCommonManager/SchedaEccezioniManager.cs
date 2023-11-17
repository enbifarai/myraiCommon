using myRaiCommonModel;
using myRaiData;
using System;
using System.Collections.Generic;
using System.Linq;
using myRaiHelper;

namespace myRaiCommonManager
{
    public class SchedaEccezioniManager
    {
        public enum EnumPosizioniCampo
        {
            Prima_del_campo_DEFINIZIONE,
            Prima_del_campo_CRITERI_DI_INSERIMENTO,
            Prima_del_campo_TRATTAMENTO_ECONOMICO,
            Prima_del_campo_DOCUMENTAZIONE,
            Prima_del_campo_PRESUPPOSTI_E_PROCEDURE,
            Prima_del_campo_NOTE,
            Prima_del_campo_ALLEGATI,
            Prima_del_campo_FONTI_DELLA_DISCIPLINA,
            Prima_del_campo_ULTERIORI_INFORMAZIONI,
        }

        public static PopupEccezioneModel getEccezioneForUpdate(int id)
        {
            var db = new digiGappEntities();
            var scheda = db.MyRai_Regole_SchedeEccezioni.ValidToday().Where(x => x.id == id).FirstOrDefault();

            PopupEccezioneModel model = new PopupEccezioneModel() { IsNew = false, IdEccezione = id };
            model.CodiceEccezione = scheda.codice;
            model.CriteriInserimento = scheda.criteri_inserimento;
            model.Definizione = scheda.definizione;
            model.DescrittivaEccezione = scheda.descrittiva;
            model.Note = scheda.note;
            model.Presupposti = scheda.presupposti_documentazione;
            model.TipoAssenza = scheda.id_tipo_assenza;
            model.TipoDocumentazione = scheda.id_tipo_documentazione;
            model.TrattamentoEconomico = scheda.trattamento_economico;
            model.PresuppostiProcedure = scheda.presupposti_procedure;
            model.DescrittivaLibera = scheda.descrittiva_libera;

            var sc = scheda.MyRai_Regole_CampiDinamici.ValidToday();
            model.campodinamico = sc .Select(x => x.chiave).ToArray();
            model.valoredinamico = sc.Select(x => x.valore).ToArray();
            model.posizionedinamico = sc.Select(x => x.Posizione!=null?x.Posizione.ToString():null).ToArray();

            model.utenti = scheda.MyRai_Regole_SchedeEccezioni_Utenti.ValidToday().Select(x => x.id_utente).ToArray();
            model.destinatari = scheda.MyRai_Regole_SchedeEccezioni_Destinatari.ValidToday().Select(x => x.id_destinatario).ToArray();
            model.tematiche = scheda.MyRai_Regole_SchedeEccezioni_Tematiche
                .ValidToday()
                .Select(x => x.id_tematica).ToArray();


            model.fonti = scheda.MyRai_Regole_SchedeEccezioni_Fonti.ValidToday().Select(x => x.descrizione).ToArray();
            model.urlfonti = scheda.MyRai_Regole_SchedeEccezioni_Fonti.ValidToday().Select(x => x.url).ToArray();
            model.allegati = scheda.MyRai_Regole_Allegati.ValidToday().ToList();
            model.EccezioniCollegate = scheda.EccezioniCollegate;
            model.Pubblicata = scheda.Pubblicato==true ? true : false;

            return model;
        }

        public static SchedeEccezioniModel getIndexModel()
        {
         var db = new digiGappEntities();
            List<MyRai_Regole_SchedeEccezioni> list = db.MyRai_Regole_SchedeEccezioni
                                                        .ValidToday()
                                                        .OrderBy(x => x.codice).ToList();
            var listAll = db.L2D_ECCEZIONE.Select(z => new { cod = z.cod_eccezione, desc = z.desc_eccezione });

            foreach (var e in listAll)
            {
                if (list.Select(x => x.codice.Trim()).Contains(e.cod)) continue;
                MyRai_Regole_SchedeEccezioni s = new MyRai_Regole_SchedeEccezioni();
                s.codice = e.cod;
                s.descrittiva = e.desc;
                list.Add(s);
            }

            SchedeEccezioniModel model = new SchedeEccezioniModel() { Schede = list.OrderBy(x => x.codice).ToList() };
            return model;
        }

        public static Boolean SchedaEccezioneChanged(PopupEccezioneModel model, myRaiData.MyRai_Regole_SchedeEccezioni scheda)
        {
            bool changed = false;
            changed =
                scheda.note != model.Note
                ||
                scheda.id_tipo_documentazione != model.TipoDocumentazione
                ||
                scheda.criteri_inserimento != model.CriteriInserimento
                ||
                scheda.definizione != model.Definizione
                ||
                scheda.descrittiva != model.DescrittivaEccezione
                ||
                scheda.descrittiva_libera!= model.DescrittivaLibera
                ||
                scheda.id_tipo_assenza != model.TipoAssenza
                ||
                scheda.presupposti_documentazione != model.Presupposti
                ||
                scheda.presupposti_procedure != model.PresuppostiProcedure
                ||
                scheda.EccezioniCollegate != model.EccezioniCollegate
                ||
                (model.Pubblicata ==true && (scheda.Pubblicato == false ||scheda.Pubblicato ==null))
                ||
                (model.Pubblicata ==false && scheda.Pubblicato==true)
                ||
                scheda.trattamento_economico != model.TrattamentoEconomico;

            return changed;
        }

        public static MyRai_Regole_SchedeEccezioni SalvaSchedaAggiornata(PopupEccezioneModel model, 
            myRaiData.MyRai_Regole_SchedeEccezioni oldScheda, digiGappEntities db)
        {
            int idold = oldScheda.id;
            var schedaNew = new MyRai_Regole_SchedeEccezioni();
            schedaNew.codice = model.CodiceEccezione;
            schedaNew.criteri_inserimento = model.CriteriInserimento;
            schedaNew.definizione = model.Definizione;
            schedaNew.descrittiva = model.DescrittivaEccezione;
            schedaNew.descrittiva_libera = model.DescrittivaLibera;
            schedaNew.id_tipo_assenza = model.TipoAssenza;
            schedaNew.presupposti_documentazione = model.Presupposti;
            schedaNew.presupposti_procedure = model.PresuppostiProcedure;

            schedaNew.trattamento_economico = model.TrattamentoEconomico;
            schedaNew.data_inizio_validita = DateTime.Now;
            schedaNew.data_modifica = DateTime.Now;
            schedaNew.id_tipo_documentazione = model.TipoDocumentazione;
            schedaNew.note = model.Note;
            schedaNew.EccezioniCollegate = model.EccezioniCollegate;
            schedaNew.Pubblicato = model.Pubblicata;

            db.MyRai_Regole_SchedeEccezioni.Add(schedaNew);

            oldScheda.data_fine_validita = DateTime.Now;

           // db.SaveChanges();

           
           

            //gira tutte le dipendenze sulla scheda nuova:
            //foreach (MyRai_Regole_SchedeEccezioni_Destinatari d in db.MyRai_Regole_SchedeEccezioni_Destinatari
            //    .Where (x=>x.id_scheda_eccezione==idold))
            //{
            //    d.id_scheda_eccezione = schedaNew.id;
            //}
            foreach (MyRai_Regole_SchedeEccezioni_Destinatari d  in oldScheda.MyRai_Regole_SchedeEccezioni_Destinatari)
            {
               // d.MyRai_Regole_SchedeEccezioni = schedaNew;
                schedaNew.MyRai_Regole_SchedeEccezioni_Destinatari.Add(d);
            }
            foreach (MyRai_Regole_SchedeEccezioni_Utenti d in oldScheda .MyRai_Regole_SchedeEccezioni_Utenti)
            {
               // d.id_scheda_eccezione = schedaNew.id;
                schedaNew.MyRai_Regole_SchedeEccezioni_Utenti.Add(d);
            }
            foreach (MyRai_Regole_SchedeEccezioni_Tematiche d in oldScheda .MyRai_Regole_SchedeEccezioni_Tematiche )
            {
               // d.id_scheda_eccezione = schedaNew.id;
                schedaNew.MyRai_Regole_SchedeEccezioni_Tematiche.Add(d);
            }
            foreach (MyRai_Regole_SchedeEccezioni_Fonti d in oldScheda.MyRai_Regole_SchedeEccezioni_Fonti)
            {
               // d.id_scheda_eccezione = schedaNew.id;
                schedaNew.MyRai_Regole_SchedeEccezioni_Fonti.Add(d);
            }
            foreach (MyRai_Regole_CampiDinamici d in oldScheda.MyRai_Regole_CampiDinamici )
            {
               // d.id_scheda_eccezione = schedaNew.id;
                schedaNew.MyRai_Regole_CampiDinamici.Add(d);
            }
            return schedaNew;

            //db.SaveChanges();
            //var schedaSalvata = db.MyRai_Regole_SchedeEccezioni.Where(x => x.id == schedaNew.id).FirstOrDefault();
            //return schedaSalvata;
        }

        public static void UpdateDipendenzaUtenti(PopupEccezioneModel model, MyRai_Regole_SchedeEccezioni scheda, 
            digiGappEntities db)
        {
            
            //cerca eliminati
            foreach (var u in scheda.MyRai_Regole_SchedeEccezioni_Utenti.ValidToday())
            {
                if (! model.utenti .Contains (u.id_utente))
                {
                    u.data_fine_validita=DateTime.Now;
                }
            }
            //cerca aggiunti
            foreach (var idu in model.utenti)
            {
                if (! scheda.MyRai_Regole_SchedeEccezioni_Utenti.ValidToday().Any(x => x.id_utente == idu))
                {
                    MyRai_Regole_SchedeEccezioni_Utenti newu = new MyRai_Regole_SchedeEccezioni_Utenti();
                    newu.data_inizio_validita = DateTime.Now;
                    newu.id_utente = idu;
                    db.MyRai_Regole_SchedeEccezioni_Utenti.Add(newu);
                    scheda.MyRai_Regole_SchedeEccezioni_Utenti.Add(newu);
                }
            }
            
        }

        public static void UpdateDipendenzaTematica(PopupEccezioneModel model, myRaiData.MyRai_Regole_SchedeEccezioni scheda,
           digiGappEntities db)
        {

            //cerca eliminati
            foreach (var te in scheda.MyRai_Regole_SchedeEccezioni_Tematiche.ValidToday())
            {
                if (!model.tematiche.Contains(te.id_tematica))
                {
                    te.data_fine_validita = DateTime.Now;
                }
            }
            //cerca aggiunti
            foreach (var tem in model.tematiche)
            {
                if (!scheda.MyRai_Regole_SchedeEccezioni_Tematiche.ValidToday().Any(x => x.id_tematica == tem))
                {
                    MyRai_Regole_SchedeEccezioni_Tematiche newt = new MyRai_Regole_SchedeEccezioni_Tematiche();
                    newt.data_inizio_validita = DateTime.Now;
                    newt.id_tematica = tem;
                    db.MyRai_Regole_SchedeEccezioni_Tematiche.Add (newt);
                    scheda.MyRai_Regole_SchedeEccezioni_Tematiche.Add(newt);
                }
            }
           
        }

        public static void UpdateDipendenzaDestinatari(PopupEccezioneModel model, myRaiData.MyRai_Regole_SchedeEccezioni scheda,
          digiGappEntities db)
        {

            //cerca eliminati
            foreach (var de in scheda.MyRai_Regole_SchedeEccezioni_Destinatari.ValidToday())
            {
                if (!model.destinatari.Contains(de.id_destinatario))
                {
                    de.data_fine_validita = DateTime.Now;
                }
            }
            //cerca aggiunti
            foreach (var des in model.destinatari)
            {
                if (!scheda.MyRai_Regole_SchedeEccezioni_Destinatari.ValidToday().Any(x => x.id_destinatario== des))
                {
                    MyRai_Regole_SchedeEccezioni_Destinatari newd = new MyRai_Regole_SchedeEccezioni_Destinatari();
                    newd.data_inizio_validita = DateTime.Now;
                    newd.id_destinatario = des;
                    db.MyRai_Regole_SchedeEccezioni_Destinatari.Add(newd);
                    scheda.MyRai_Regole_SchedeEccezioni_Destinatari.Add(newd);
                }
            }
        }

        public static void UpdateDipendenzaCampiDinamici(PopupEccezioneModel model, myRaiData.MyRai_Regole_SchedeEccezioni scheda,
         digiGappEntities db)
        {
            for (int i=0;i<scheda.MyRai_Regole_CampiDinamici.ValidToday().Count() ;i++)
            {
                var camp =scheda.MyRai_Regole_CampiDinamici.ValidToday().ToList()[i];
                bool trovato = false;
                for (int k = 0; k < model.campodinamico.Length; k++)
                {
                    if (model.campodinamico[k] == camp.chiave && model.valoredinamico[k] == camp.valore)
                    {
                        string pos = camp.Posizione == null ? null : camp.Posizione.ToString();
                        if ( (String.IsNullOrWhiteSpace(model.posizionedinamico[k]) && String.IsNullOrWhiteSpace(pos)) || (model.posizionedinamico[k]==pos ))
                        {
                            trovato = true;
                            break;
                        }
                    }
                }
                if (!trovato)
                {
                    camp.data_fine_validita = DateTime.Now;
                }
            }
            for (int k = 0; k < model.campodinamico.Length; k++)
            {
                if (String.IsNullOrWhiteSpace(model.campodinamico[k]) ) continue;
                
                if (! scheda.MyRai_Regole_CampiDinamici.ValidToday().Any(x => x.chiave == model.campodinamico[k] 
                      && x.valore == model.valoredinamico[k] && (x.Posizione==null?"":x.Posizione.ToString())==model.posizionedinamico[k]   ))
                {
                    MyRai_Regole_CampiDinamici cd = new MyRai_Regole_CampiDinamici();
                    cd.data_inizio_validita = DateTime.Now;
                    cd.chiave = model.campodinamico[k];
                    cd.valore = model.valoredinamico[k];
                    if (String.IsNullOrWhiteSpace( model.posizionedinamico[k]))
                        cd.Posizione = null;
                    else
                        cd.Posizione = Convert.ToInt32(model.posizionedinamico[k]);
                    db.MyRai_Regole_CampiDinamici.Add(cd);
                    scheda.MyRai_Regole_CampiDinamici.Add(cd);
                }
            }
        }

        public static void UpdateDipendenzaFonti(PopupEccezioneModel model, myRaiData.MyRai_Regole_SchedeEccezioni scheda,
        digiGappEntities db)
        {
            for (int i = 0; i < scheda.MyRai_Regole_SchedeEccezioni_Fonti.ValidToday().Count(); i++)
            {
                var fon = scheda.MyRai_Regole_SchedeEccezioni_Fonti.ValidToday().ToList()[i];
                bool trovato = false;
                for (int k = 0; k < model.fonti.Length; k++)
                {
                    if (model.fonti[k] == fon.descrizione && model.urlfonti[k] == fon.url)
                    {
                        trovato = true;
                        break;
                    }
                }
                if (!trovato)
                {
                    fon.data_fine_validita = DateTime.Now;
                }
            }
            for (int k = 0; k < model.fonti.Length; k++)
            {
                if (String.IsNullOrWhiteSpace(model.fonti[k]) && String.IsNullOrWhiteSpace(model.urlfonti[k])) continue;

                if (!scheda.MyRai_Regole_SchedeEccezioni_Fonti.ValidToday().Any(x => x.descrizione == model.fonti[k]
                      && x.url == model.urlfonti[k]))
                {
                    MyRai_Regole_SchedeEccezioni_Fonti f = new MyRai_Regole_SchedeEccezioni_Fonti();
                    f.data_inizio_validita = DateTime.Now;
                    f.descrizione = model.fonti[k];
                    f.url = model.urlfonti[k];
                    db.MyRai_Regole_SchedeEccezioni_Fonti.Add(f);
                    scheda.MyRai_Regole_SchedeEccezioni_Fonti.Add(f);
                }
            }
        }
    }
}