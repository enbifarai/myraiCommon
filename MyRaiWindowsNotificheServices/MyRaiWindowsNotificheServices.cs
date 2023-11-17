using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using myRaiCommonTasks;
using myRaiData;

namespace MyRaiWindowsNotificheServices
{
	public partial class MyRaiWindowsNotificheServices : ServiceBase
	{

		public enum EnumParametriSistema
		{
			MaxRowsVisualizzabiliDaApprovare,
			AccountUtenteServizio,
			MatricolaSimulata,
			MailApprovazioneSubject,
			MailApprovazioneFrom,
			MailApprovazioneTemplate,
			MailRifiutaTemplate,
			UrlImmagineDipendente,
			OrariGapp,
			ValidazioneGenericaEccezioni,
			RowsPerScroll,
			LimiteMesiBackPerEvidenze,
			MessaggioAssenteIngiustificato,
			PosizioneNumDoc,
			TipiDipQuadraturaSettimanale,
			TipiDipQuadraturaGiornaliera,
			GestisciDateSuDocumenti,
			OreRichiesteUrgenti,
			SovrascritturaTipoDipendente,
			CodiceCSharp,
			IgnoraAssenzeIngiustificatePerMatricole,
			GiornoEsecuzioneBatchPDF,
			IntervalloBatchSecondi,
			LivelloUtenteListaEccezioni,
			AutorizzaMinimale,
			SoppressePOH,
			ApprovaPresenzeDopoGG,
			POHperMese,
			MailTemplateOmaggi,
			AppKeyhrce,
			NotificheRangeOre,
			MailTemplateAvvisoDelega,
			MailTemplateRevocaDelega,
			MailTemplateOmaggiUte,
			MinutiPrenotazione,
			MatricoleAdmin,
			ColoriCharts,
			MailNotificaQuestionario,
			ServizioTema,
			ServizioTemaMapping,
			MailAbbonamentiFrom,
			MailTemplateAbbonamentiRoma,
			MailTemplateAbbonamentiTorino,
			BrowserAmmessi,
			MessaggioBadBrowser,
			ShareAllegati,
			TestoPrivacy,
			ForzaFiguraProfessionale,
			RedirectEmailSuSviluppo,
			RangeMensa,
			MatricoleTester,
			EtichettaFirmaSuPDF,
			TestoPolicyAbbonamenti,
			TestoPolicyAbbonamentiRoma,
			TestoPolicyAbbonamentiTorino,
			ToolbarSummernote,
			NumeriMassimoAllegati,
			DimensioneMassimaAllegati,
			AccettaSoloDurata,
			IndirizzoComnitNotFound,
			IndirizzoComnit3,
			IndirizzoComnit4,
			IndirizzoComnit5,
			IndirizzoComnit6,
			IndirizzoComnit7,
			CorsoOnline,
			TipiContrattoCorsiOlnlie,
			SocietaCorsiOnline,
			UsaServizioPerProfiloPersonale,
			MailEventiFrom,
			MailEventiBody,
			MailEventiBodyDel,
			MailCCRoma,
			MailCCTorino,
			MatricoleAutorizzatePDF,
			MessaggioChiusura,
			EccezioniNoQuantita,
			ForzaAbilitazioneGapp,
			EccezioniIniettate,
			EccezioniOneClick,
			MailDebug,
			ServiziChiSono,
			EccezioniDigitaSoloQuantita,
			UrlNotificheBustaPagaDocumenti,
			EccezioneFittiziaSpostamento,
			EccezioneFittiziaIgnora,
			MailIscrizioneCorsoFrom,
			MailIscrizioneCorsoBody,
			CVEditorialiSezContabiliAbilitate,
			AbilitatiSimulazione,
			StopMail,
			TestoPrivacyGenerale,
			MaxDatePolicy,
			NotificaCV100,
			NotificaCVLess100,
			NotificaCVZero,
			GetCategoriaDatoNetCached,
			GetCategoriaDatoNetCachedL2,
			GetCategoriaDatoNetCachedNolevel,
			ServizioCeitonGetAttivita,
			AcademyTematicheAggiuntive,
			AcademyUrlIlias,
			MailMappaturaPrenotazione,
			MailMappaturaCancellazione,
			AbbonamentiMatricoleCancellazione,
			PathImmaginiFittizie,
			EccezioniMotivoObbligatorioSeServizioEsterno,
			EccezioniMotivoObbligatorioDescrizione,
			UtentePerConvalida,
			CeitonPosizionamentoEccezioni
		}

		public class Abilitazioni
		{
			public Abilitazioni ()
			{
				ListaAbilitazioni = new List<AbilitazioneSede>();
			}
			public List<AbilitazioneSede> ListaAbilitazioni { get; set; }
		}
		public class AbilitazioneSede
		{
			public AbilitazioneSede ()
			{
				MatrLivello1 = new List<MatrSedeAppartenenza>();
				MatrLivello2 = new List<MatrSedeAppartenenza>();
			}
			public string Sede { get; set; }
			public string DescrSede { get; set; }
			public List<MatrSedeAppartenenza> MatrLivello1 { get; set; }
			public List<MatrSedeAppartenenza> MatrLivello2 { get; set; }
		}
		public class MatrSedeAppartenenza
		{
			public string Matricola { get; set; }
			public string SedeAppartenenza { get; set; }
			public string Delegante { get; set; }
			public string Delegato { get; set; }
		}

		System.Timers.Timer TimerNotificheInserimentiVersoL1;
		System.Timers.Timer TimerNotificheStorniVersoL1;
		System.Timers.Timer TimerNotificheScaduteVersoL1;
		System.Timers.Timer TimerNotificheUrgentiVersoL1;
		System.Timers.Timer TimerNotificheDipendente;

		public MyRaiWindowsNotificheServices ()
		{
			InitializeComponent();
		}

		public void MyStart ()
		{
			OnStart( null );
		}

		protected override void OnStart ( string[] args )
		{
			Logger.Log( "---------------------------------------------------------------------------------" );
			Logger.Log( "Service started" );
			Logger.Log( "---------------------------------------------------------------------------------" );

			// Invio notifiche riepilogo richieste da approvare (scadute, urgenti, normali)
			//TimerNotificheInserimentiVersoL1 = new System.Timers.Timer();
			//TimerNotificheInserimentiVersoL1.Interval = 5 * 60 * 1000;
			//TimerNotificheInserimentiVersoL1.Elapsed += TimerNotificheInserimentiVersoL1_Elapsed;
			//TimerNotificheInserimentiVersoL1.Enabled = true;
			//TimerNotificheInserimentiVersoL1.Start();
			//TimerNotificheInserimentiVersoL1_Elapsed( null, null );

			////Invio notifiche di storni verso  L1
			//TimerNotificheStorniVersoL1 = new System.Timers.Timer();
			//TimerNotificheStorniVersoL1.Interval = 5 * 60 * 1000;
			//TimerNotificheStorniVersoL1.Elapsed += TimerNotificheStorniVersoL1_Elapsed;
			//TimerNotificheStorniVersoL1.Enabled = true;
			//TimerNotificheStorniVersoL1.Start();

			////Invio notifiche scadute verso L1
			//TimerNotificheScaduteVersoL1 = new System.Timers.Timer();
			//TimerNotificheScaduteVersoL1.Interval = 5 * 60 * 1000;
			//TimerNotificheScaduteVersoL1.Elapsed += TimerNotificheScaduteVersoL1_Elapsed;
			//TimerNotificheScaduteVersoL1.Enabled = true;
			//TimerNotificheScaduteVersoL1.Start();

			////Invio notifiche urgenti verso L1
			//TimerNotificheUrgentiVersoL1 = new System.Timers.Timer();
			//TimerNotificheUrgentiVersoL1.Interval = 5 * 60 * 1000;
			//TimerNotificheUrgentiVersoL1.Elapsed += TimerNotificheUrgentiVersoL1_Elapsed;
			//TimerNotificheUrgentiVersoL1.Enabled = true;
			//TimerNotificheUrgentiVersoL1.Start();

			// Notifiche ai dipendenti di eccezioni approvate o rifiutate
			//TimerNotificheDipendente = new System.Timers.Timer();
			//TimerNotificheDipendente.Interval = 0.5 * 60 * 1000;
			//TimerNotificheDipendente.Elapsed += TimerNotificheDipendente_Elapsed;
			//TimerNotificheDipendente.Enabled = true;
			//TimerNotificheDipendente.Start();
			//TimerNotificheDipendente_Elapsed( null, null );

			return;
		}

		private List<AbilitazioneSede> GetSediL1 ()
		{
			var ab = getAbilitazioni();
			return ab.ListaAbilitazioni.Where( w => w.MatrLivello1.Any() ).ToList();
		}

		private List<AbilitazioneSede> GetSediL2 ()
		{
			var ab = getAbilitazioni();
			return ab.ListaAbilitazioni.Where( w => w.MatrLivello2.Any() ).ToList();
		}

		public static Abilitazioni getAbilitazioni ()
		{
			Autorizzazioni.Sedi service = new Autorizzazioni.Sedi();
			service.Credentials = new System.Net.NetworkCredential( GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[0], GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[1] );

			Autorizzazioni.CategorieDatoAbilitate response = Get_CategoriaDato_Net_Cached( 0 );

			Abilitazioni AB = new Abilitazioni();

			foreach ( var item in response.CategorieDatoAbilitate_Array )
			{
				AbilitazioneSede absede = new AbilitazioneSede()
				{
					Sede = item.Codice_categoria_dato,
					DescrSede = item.Descrizione_categoria_dato
				};

				foreach ( System.Data.DataRow row in item.DT_Utenti_CategorieDatoAbilitate.Rows )
				{
					if ( row["codice_sottofunzione"].ToString() == "01GEST" )
					{
						MatrSedeAppartenenza ms = new MatrSedeAppartenenza();
						ms.Matricola = row["logon_id"].ToString();
						ms.SedeAppartenenza = row["SedeGappAppartenenza"].ToString();
						ms.Delegante = row["Delegante"].ToString();
						ms.Delegato = row["Delegato"].ToString();
						absede.MatrLivello1.Add( ms );
					}

					if ( row["codice_sottofunzione"].ToString() == "02GEST" )
					{
						MatrSedeAppartenenza ms = new MatrSedeAppartenenza();
						ms.Matricola = row["logon_id"].ToString();
						ms.SedeAppartenenza = row["SedeGappAppartenenza"].ToString();
						ms.Delegante = row["Delegante"].ToString();
						ms.Delegato = row["Delegato"].ToString();
						absede.MatrLivello2.Add( ms );
					}
				}
				AB.ListaAbilitazioni.Add( absede );
			}
			AB.ListaAbilitazioni = AB.ListaAbilitazioni.OrderBy( x => x.Sede ).ToList();

			return AB;
		}

		public static Autorizzazioni.CategorieDatoAbilitate Get_CategoriaDato_Net_Cached ( int Liv )
		{
			string[] responseDB;

			if ( Liv == 1 )
			{
				responseDB = GetParametri<string>( EnumParametriSistema.GetCategoriaDatoNetCached );
				
				if (
					   String.IsNullOrWhiteSpace( responseDB[0] )
					|| responseDB[1] != DateTime.Today.ToString( "dd/MM/yyyy" )
				   )
				{
					Autorizzazioni.Sedi service = new Autorizzazioni.Sedi();
					Autorizzazioni.CategorieDatoAbilitate response = service.Get_CategoriaDato_Net_RaiPerMe( "sedegapp", "", "HRUP", "01GEST" );

					return response;
				}
				else
				{
					Autorizzazioni.CategorieDatoAbilitate response =
						Newtonsoft.Json.JsonConvert.DeserializeObject<Autorizzazioni.CategorieDatoAbilitate>( responseDB[0] );

					return response;
				}
			}
			else if ( Liv == 2 )
			{
				responseDB = GetParametri<string>( EnumParametriSistema.GetCategoriaDatoNetCachedL2 );
				if (
					   String.IsNullOrWhiteSpace( responseDB[0] )
					|| responseDB[1] != DateTime.Today.ToString( "dd/MM/yyyy" )
					)
				{
					Autorizzazioni.Sedi service = new Autorizzazioni.Sedi();
					Autorizzazioni.CategorieDatoAbilitate response =
						service.Get_CategoriaDato_Net_RaiPerMe( "sedegapp", "", "HRUP", "02GEST" );

					return response;
				}
				else
				{
					Autorizzazioni.CategorieDatoAbilitate response =
						Newtonsoft.Json.JsonConvert.DeserializeObject<Autorizzazioni.CategorieDatoAbilitate>( responseDB[0] );

					return response;
				}
			}
			else  //senza livello
			{
				responseDB = GetParametri<string>( EnumParametriSistema.GetCategoriaDatoNetCachedNolevel );
				if (
					   String.IsNullOrWhiteSpace( responseDB[0] )
					|| responseDB[1] != DateTime.Today.ToString( "dd/MM/yyyy" )
					)
				{
					Autorizzazioni.Sedi service = new Autorizzazioni.Sedi();
					Autorizzazioni.CategorieDatoAbilitate response =
						service.Get_CategoriaDato_Net_RaiPerMe( "sedegapp", "", "HRUP", "01GEST|02GEST" );

					return response;
				}
				else
				{
					Autorizzazioni.CategorieDatoAbilitate response =
						Newtonsoft.Json.JsonConvert.DeserializeObject<Autorizzazioni.CategorieDatoAbilitate>( responseDB[0] );

					return response;
				}
			}
		}

		public static T[] GetParametri<T> ( EnumParametriSistema chiave )
		{
			var db = new digiGappEntities();
			String NomeParametro = chiave.ToString();

			MyRai_ParametriSistema p = db.MyRai_ParametriSistema.FirstOrDefault( x => x.Chiave == NomeParametro );
			if ( p == null ) return null;
			else
			{
				T[] parametri = new T[] { ( T )Convert.ChangeType( p.Valore1, typeof( T ) ), ( T )Convert.ChangeType( p.Valore2, typeof( T ) ) };
				return parametri;
			}
		}
		
		#region Notifiche verso utente livello 1
		void TimerNotificheUrgentiVersoL1_Elapsed ( object sender, System.Timers.ElapsedEventArgs e )
		{
			TimerNotificheUrgentiVersoL1.Stop();
			TimerNotificheUrgentiVersoL1.Enabled = false;

			try
			{
				Logger.Log( "Inizio invio notifiche urgenti --> L1" );
				NotificheDestinateL1.InviaUrgentiL1();
				Logger.Log( "Fine invio notifiche urgenti" );
			}
			catch ( Exception ex )
			{
				Logger.Log( ex.ToString() );
				LogErrore( ex.ToString(), "TimerNotificheUrgentiVersoL1" );
			}

			TimerNotificheUrgentiVersoL1.Start();
			TimerNotificheUrgentiVersoL1.Enabled = true;
		}

		void TimerNotificheScaduteVersoL1_Elapsed ( object sender, System.Timers.ElapsedEventArgs e )
		{
			TimerNotificheScaduteVersoL1.Stop();
			TimerNotificheScaduteVersoL1.Enabled = false;

			try
			{
				Logger.Log( "Inizio invio notifiche scadute --> L1" );
				NotificheDestinateL1.InviaScaduteL1();
				Logger.Log( "Fine invio notifiche scadute" );
			}
			catch ( Exception ex )
			{
				Logger.Log( ex.ToString() );
				LogErrore( ex.ToString(), "TimerNotificheScaduteVersoL1_Elapsed" );
			}

			TimerNotificheScaduteVersoL1.Start();
			TimerNotificheScaduteVersoL1.Enabled = true;
		}

		void TimerNotificheStorniVersoL1_Elapsed ( object sender, System.Timers.ElapsedEventArgs e )
		{
			TimerNotificheStorniVersoL1.Stop();
			TimerNotificheStorniVersoL1.Enabled = false;

			try
			{
				Logger.Log( "Inizio invio notifiche storni --> L1" );
				NotificheDestinateL1.InviaRichiesteInserite( true );
				Logger.Log( "Fine invio notifiche storni" );
			}
			catch ( Exception ex )
			{
				Logger.Log( ex.ToString() );
				LogErrore( ex.ToString(), "TimerNotificheStorniVersoL1" );
			}

			TimerNotificheStorniVersoL1.Start();
			TimerNotificheStorniVersoL1.Enabled = true;
		}

		void TimerNotificheInserimentiVersoL1_Elapsed ( object sender, System.Timers.ElapsedEventArgs e )
		{
			TimerNotificheInserimentiVersoL1.Stop();
			TimerNotificheInserimentiVersoL1.Enabled = false;

			try
			{
				Logger.Log( "Inizio invio notifiche inserimento --> L1" );
				NotificheDestinateL1.InviaRichiesteInserite();
				Logger.Log( "Fine invio notifiche inserimentoe" );
			}
			catch ( Exception ex )
			{
				Logger.Log( ex.ToString() );
				LogErrore( ex.ToString(), "TimerNotificheInserimentiVersoL1" );
			}

			TimerNotificheInserimentiVersoL1.Start();
			TimerNotificheInserimentiVersoL1.Enabled = true;
		}

		#endregion

		#region Notifiche utente (approvazione/rifiuto)

		public void TimerNotificheDipendente_Elapsed ( object sender, System.Timers.ElapsedEventArgs e )
		{
			TimerNotificheDipendente.Stop();
			TimerNotificheDipendente.Enabled = false;

			try
			{
				NotificheDestinateDipendenti.SendNotificheESS_DIP();
			}
			catch ( Exception ex )
			{
				Logger.Log( ex.ToString() );
				LogErrore( ex.ToString(), "TimerNotificheDipendente" );
			}

			TimerNotificheDipendente.Start();
			TimerNotificheDipendente.Enabled = true;
		}

		#endregion

		protected override void OnStop ()
		{
			Logger.Log( "---------------------------------------------------------------------------------" );
			Logger.Log( "Service stopped" );
			Logger.Log( "---------------------------------------------------------------------------------" );
		}

		public static void LogErrore ( string errore, string provenienza )
		{
			try
			{
				if ( Environment.UserInteractive ) Console.WriteLine( errore );
				MyRai_LogErrori err = new MyRai_LogErrori()
				{
					applicativo = "BATCH",
					data = DateTime.Now,
					error_message = errore,
					matricola = "000000",
					provenienza = provenienza
				};
				using ( var db = new digiGappEntities() )
				{
					db.MyRai_LogErrori.Add( err );
					db.SaveChanges();
				}
				Logger.Log( "Errore da " + provenienza + ": " + errore );
			}
			catch ( Exception ex )
			{
				Logger.Log( ex.ToString() );
			}
		}
	}
}