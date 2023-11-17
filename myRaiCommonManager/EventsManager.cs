using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using myRai.DataAccess;
using myRaiCommonModel;
using myRaiHelper;

namespace myRaiCommonManager
{
    public class EventsManager
    {
        public static EventsClientModel InitModel(string backurl,string id)
        {
            EventsClientModel model = getModel(false);

            if (!string.IsNullOrWhiteSpace(backurl) && !String.IsNullOrWhiteSpace(id))
            {
                model.ideventoAperturaRemota = id;
                model.ReturnURL = backurl;
                model.PopupAutoOpen = true;
            }
            model.timeoutMinuti = CommonHelper.GetParametro<int>(EnumParametriSistema.MinutiPrenotazione);
            return model;
        }
        public Nullable<System.TimeSpan> Published { get; set; }

        public static EventsClientModel getModel(bool loadDisponibili=true)
        {
            EliminaPrenotazioniAppese();
            var db = new myRaiData.digiGappEntities();
            string matr = CommonHelper.GetCurrentUserMatricola();
            string sede = UtenteHelper.EsponiAnagrafica()._sedeApppartenenza;

            EventsClientModel model = new EventsClientModel(sede);
            model.EventiPrenotati = db.B2RaiPlace_Eventi_Anagrafica
                .Where(x => x.matricola == matr && x.confermata == true)
                .Select(x => x.B2RaiPlace_Eventi_Evento).Distinct().ToList();

            string sedecont = UtenteHelper.EsponiAnagrafica().SedeContabile;

            var prof = db.MyRai_Profili.Where(x => x.nome_profilo == "Rossi Comint").FirstOrDefault();
            bool VediTuttiProfili = (prof != null && prof.matricola != null && prof.matricola.Contains(CommonHelper.GetCurrentUserMatricola()));

            if (loadDisponibili)
            {
                model.EventiDisponibili = db.B2RaiPlace_Eventi_Evento.Include("B2RaiPlace_Eventi_Anagrafica")
                    .Where(a => VediTuttiProfili || a.sede_contabile == null || a.sede_contabile.Trim() == "" || a.sede_contabile == sedecont)
                    .Where(x => x.data_fine_prenotazione > DateTime.Now &&
                        x.data_inizio_prenotazione < DateTime.Now &&
                        x.B2RaiPlace_Eventi_Anagrafica.Where(z =>
                            z.confermata == true && z.matricola == matr).Count() < x.numero_massimo
                            //&& (
                                //x.B2RaiPlace_Eventi_Utenti.Count == 0 && x.B2RaiPlace_Eventi_Sede.Count == 0 ||
                                //(x.B2RaiPlace_Eventi_Utenti_Abilitati.Where(w => w.Matricola == matr).Count() > 0) ||
                                //(x.B2RaiPlace_Eventi_Sede.Where(t => t.sede_gapp == sede).Count() > 0))
                            && (x.B2RaiPlace_Eventi_Utenti_Abilitati.Count==0 || x.B2RaiPlace_Eventi_Utenti_Abilitati.Any(z=>z.Matricola==matr))
                            && (x.B2RaiPlace_Eventi_Sede.Count==0 || x.B2RaiPlace_Eventi_Sede.Any(s=>s.sede_gapp==sede))

                        )
                    .Select(x => new EventoDisponibile
                    {
                        EventoDisp = x,
                        PostiRimasti = (int)x.numero_totale - x.B2RaiPlace_Eventi_Anagrafica.Count(),
                        PostiDisponibili = (int)x.numero_massimo - x.B2RaiPlace_Eventi_Anagrafica.Where(a => a.matricola == matr && a.confermata == true).Count()

                    }).OrderByDescending(a => a.EventoDisp.data_inizio)
                    .ToList();

                //model.EventiDisponibili = model.EventiDisponibili
                //    .Where(b => b.EventoDisp.B2RaiPlace_Eventi_Utenti_Abilitati == null
                //    || b.EventoDisp.B2RaiPlace_Eventi_Utenti_Abilitati.Count() == 0
                //    || b.EventoDisp.B2RaiPlace_Eventi_Utenti_Abilitati.Any(x => x.Matricola == matr)).ToList();


                ControllaPrenotazioniStessoProgr(matr, model);
            }

            model.matricola = matr;

            return model;
        }

        private static void ControllaPrenotazioniStessoProgr(string matr, EventsClientModel model)
        {
            foreach (var item in model.EventiDisponibili)
            {
                if (item.EventoDisp.id_programma != null)
                {
                    //item.PostiDisponibili = GetPostiDisponibiliEventoConsiderandoMaxProgramma(item.EventoDisp.id_programma, matr, item.PostiDisponibili);

                    if (item.EventoDisp.id_programma == null) continue;
                    int? maxPren = item.EventoDisp.B2RaiPlace_Eventi_Programma.numero_massimo;
                    if (maxPren!=null)
                    {
                        int miePren = MiePrenotazioniPerProgramma(item.EventoDisp.id_programma.Value, matr);
                        if (miePren == 0) continue;
                        int miePrenPossibili = (int)maxPren - miePren;
                        if (miePrenPossibili < 0) miePrenPossibili = 0;
                        if (miePrenPossibili < item.PostiDisponibili) item.PostiDisponibili = miePrenPossibili;
                    }
                }
            }
        }
        //public static int GetPostiDisponibiliEventoConsiderandoMaxProgramma(int? idprogramma,string matr,int postiDisponibiliDaEvento)
        //{
        //    if (idprogramma == null) return postiDisponibiliDaEvento;
        //    int? maxPren = PrenotazioniMaxPerProgramma((int)idprogramma);
        //    if (maxPren != null)
        //    {
        //        int miePren = MiePrenotazioniPerProgramma((int)idprogramma, matr);
        //        if (miePren == 0) return postiDisponibiliDaEvento;
        //        int miePrenPossibili = (int)maxPren - miePren;
        //        if (miePrenPossibili < 0) miePrenPossibili = 0;
        //        if (miePrenPossibili < postiDisponibiliDaEvento) postiDisponibiliDaEvento = miePrenPossibili;

        //        return postiDisponibiliDaEvento;
        //    }
        //    return postiDisponibiliDaEvento;
        //}
        public static int? PrenotazioniMaxPerProgramma(int idprogramma)
        {
            var db = new myRaiData.digiGappEntities();
            return db.B2RaiPlace_Eventi_Programma.Where(x => x.id == idprogramma).Select(x => x.numero_massimo).FirstOrDefault();
        }
        public static int MiePrenotazioniPerProgramma(int idProgramma, string matricola)
        {
            var db = new myRaiData.digiGappEntities();
            return db.B2RaiPlace_Eventi_Anagrafica.Where(x => x.matricola == matricola && x.confermata == true && x.B2RaiPlace_Eventi_Evento.id_programma == idProgramma).Count();
        }

        public static int PrenotazioniConfermate(int idevento, string matricola)
        {
            var db = new myRaiData.digiGappEntities();
            return db.B2RaiPlace_Eventi_Anagrafica.Where(x => x.matricola == matricola && x.id_evento == idevento
                    && x.confermata == true).Count();
        }
        public static bool EliminaPrenotazioniAppese()
        {
            int minutiMax = CommonHelper.GetParametro<int>(EnumParametriSistema.MinutiPrenotazione);
            var db = new myRaiData.digiGappEntities();
            DateTime dataMax = DateTime.Now.AddMinutes(-minutiMax);
            var scadute = db.B2RaiPlace_Eventi_Anagrafica.Where(x => x.confermata == false && x.data_prenotazione < dataMax);
            if (scadute == null || scadute.Count() == 0) return true;

            foreach (var item in scadute)
            {
                db.B2RaiPlace_Eventi_Anagrafica.Remove(item);
            }
            return DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
        }

		/// <summary>
		/// Reperimento degli eventi prenotati dall'utente
		/// </summary>
		/// <param name="matricola"></param>
		/// <returns></returns>
		public static List<EventoPrenotato> GetEventiPrenotati ( )
		{
			List<EventoPrenotato> result = new List<EventoPrenotato>();

			try
			{
				using ( var db = new myRaiData.digiGappEntities() )
				{
					string matricola = CommonHelper.GetCurrentUserMatricola();

					//var query = from a in db.B2RaiPlace_Eventi_Anagrafica
					//			join evento in db.B2RaiPlace_Eventi_Evento
					//				on a.id_evento equals evento.id
					//			join programma in db.B2RaiPlace_Eventi_Programma
					//				on evento.id_programma equals programma.id
					//			where programma.amministrazione.Contains( matricola ) &&
					//					a.confermata == true
					//			select new { a, evento, programma };

					var query = from evento in db.B2RaiPlace_Eventi_Evento
								join programma in db.B2RaiPlace_Eventi_Programma
									on evento.id_programma equals programma.id
								where programma.amministrazione.Contains( matricola )
								select new { evento, programma };

					if ( query == null || !query.Any() )
					{
						throw new Exception( "Nessun evento trovato per l'utente corrente" );
					}

					var eventi = ( from t in query
							  select t.evento ).Distinct();

					if ( eventi == null || !eventi.Any() )
					{
						throw new Exception( "Nessun evento trovato per l'utente corrente" );
					}

					eventi.ToList().ForEach( itm =>
					{
						int count = db.B2RaiPlace_Eventi_Anagrafica.Where( e => e.id_evento == itm.id && e.confermata == true ).Count();

						result.Add( new EventoPrenotato()
						{
							DataEvento = itm.data_inizio.GetValueOrDefault(),
							Id = itm.id,
							IdProgramma = itm.id_programma,
							Luogo = itm.luogo,
							Titolo = itm.titolo,
							TotalePostiPrenotati = count
						} );
					} );
				}
			}
			catch ( Exception ex )
			{
				result = new List<EventoPrenotato>();
			}

			return result.OrderByDescending(a=> a.DataEvento).ToList();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="idEvento"></param>
		/// <returns></returns>
		public static List<Prenotati> GetPrenotati ( int idEvento )
		{
			List<Prenotati> result = new List<Prenotati>();
			int modifiche = 0;
			try
			{
				using ( var db = new myRaiData.digiGappEntities() )
				{
					string matricola = CommonHelper.GetCurrentUserMatricola();

					var evento = db.B2RaiPlace_Eventi_Evento.Where( e => e.id == idEvento ).FirstOrDefault();

					if ( evento != null )
					{
						var anag = db.B2RaiPlace_Eventi_Anagrafica.Where( e => e.id_evento == idEvento &&
							e.confermata == true ).ToList();

						if ( anag != null && anag.Any() )
						{
							anag.ForEach( e =>
							{
								Prenotati p = new Prenotati()
								{
									DataEvento = evento.data_inizio.GetValueOrDefault(),
									Id = e.id,
									IdProgramma = evento.id_programma,
									Luogo = evento.luogo,
									Titolo = evento.titolo,
									Matricola = e.matricola,
									Nome = e.nome,
									Cognome = e.cognome,
									DataNascita = e.data_nascita.GetValueOrDefault(),
									Telefono = e.telefono,
									Mail = e.email,
									TotalePostiPrenotati = anag.Count,
									SedeInsediamento = e.sede_insediamento,
									Citta = e.citta_nascita,
									Genere = e.genere,
									Grado = e.grado_parentela,
                                    Nota = e.note
								};

								if ( String.IsNullOrEmpty( e.Dipendente ) )
								{
									p.Dipendente = GetNominativoPerMatricola( e.matricola );
									e.Dipendente = p.Dipendente; // aggiorno il campo dipendente
									modifiche++;
								}
								else
								{
									p.Dipendente = e.Dipendente;
								}

								result.Add( p );
							} );
						}
					}

					// Se è stato aggiornato qualche campo allora salva le modifiche
					if (modifiche > 0)
						db.SaveChanges();
				}
			}
			catch ( Exception ex )
			{
				result = new List<Prenotati>();
			}

			return result;
		}

        private static string GetNominativoPerMatricola(string matricola)
        {
            try
            {
                string r = myRaiHelper.ServiceWrapper.EsponiAnagraficaRaicvWrapped(matricola);
                return r.Split(';')[1] + " " + r.Split(';')[2];
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}