using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using myRaiCommonManager;
using myRaiCommonModel.SituazioneDebitoria.ESS;
using myRaiHelper;
using MyRaiServiceInterface.MyRaiServiceReference1;

namespace myRai.Controllers.ESS
{
	public class SituazioneDebitoriaController : BaseCommonController
    {
		/// <summary>
		/// Render della pagina Home dell'interfaccia di Situazione Debitoria
		/// </summary>
		/// <returns></returns>
		public ActionResult Index ()
		{
			SituazioneDebitoriaVM model = new SituazioneDebitoriaVM();
			DateTime dt = DateTime.Now;
			model.CurrentDate = new DateTime( dt.Year, dt.Month, 1 );

			SituazioneDebitoriaControllerScope.Instance.ListaDebiti = new List<SituazioneDebitoria>();
			return View( model );
		}

		/// <summary>
		/// Restituzione della vista con la tabella riferita ai debiti stipulati
		/// dall'utente corrente
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public PartialViewResult LoadTableDebiti (string date = null)
		{
			try
			{
				DateTime? data = null;
				if ( date != null )
					data = DateTime.Parse( date );
				else
				{
					DateTime dt = DateTime.Now;
					data = new DateTime( dt.Year, dt.Month, 1 );

				}

				SituazioneDebitoriaVM model = new SituazioneDebitoriaVM();
				model.Data = new List<SituazioneDebitoriaModel>();

				if ( !data.HasValue )
				{
					data = DateTime.Now;
				}

				model.CurrentDate = data.Value;
				
				if ( SituazioneDebitoriaControllerScope.Instance.ListaDebiti == null ||
					!SituazioneDebitoriaControllerScope.Instance.ListaDebiti.Any() )
				{
					SituazioneDebitoriaControllerScope.Instance.ListaDebiti = GetDataInternal();
				}

				var resultData = SituazioneDebitoriaControllerScope.Instance.ListaDebiti;

				if ( resultData != null && resultData.Any() )
				{
					foreach ( var itm in resultData )
					{
						if ( data.HasValue )
						{

                            if ((itm.AnnoA == 0) || (itm.AnnoDa == 0) )
                            {
							   model.Data.Add( new SituazioneDebitoriaModel()
								    {
									    Addebito = itm.Addebito,
									    Descrizione = itm.Descrizione,
									    ImportoRata = itm.ImportoRata,
									    ImportoRateResidue = itm.ImportoRateResidue,
									    MeseA = "",
									    MeseDa = "",
									    NumeroRate = itm.NumeroRate,
									    NumeroRateResidue = itm.NumeroRateResidue
								    } );
                            }
                            else
                            {

								DateTime? t1 = null;
								DateTime? t2 = null;

								if ( itm.AnnoDa > 0 && itm.IntMeseDa > 0 )
								{
									t1 = new DateTime( itm.AnnoDa, itm.IntMeseDa, 1 );
								}
								else if ( itm.AnnoDa > 0 && itm.IntMeseDa == 0 )
								{
									t1 = DateTime.MinValue;
									itm.MeseDa = itm.AnnoDa.ToString();
								}

								if ( itm.AnnoA > 0 && itm.IntMeseA > 0 )
								{
									t2 = new DateTime( itm.AnnoA, itm.IntMeseA, 1 );
								}
								else if ( itm.AnnoA > 0 && itm.IntMeseA == 0 )
								{
									t2 = DateTime.MaxValue;
								}  

								if ( t1.HasValue &&
									t2.HasValue &&
									t1 <= data &&
									data <= t2 )
								{
									model.Data.Add( new SituazioneDebitoriaModel()
									{
										Addebito = itm.Addebito,
										Descrizione = itm.Descrizione,
										ImportoRata = itm.ImportoRata,
										ImportoRateResidue = itm.ImportoRateResidue,
										MeseA = String.IsNullOrEmpty(itm.MeseA) ? String.Empty: itm.MeseA,
										MeseDa = String.IsNullOrEmpty( itm.MeseDa ) ? String.Empty : itm.MeseDa,
										NumeroRate = itm.NumeroRate,
										NumeroRateResidue = itm.NumeroRateResidue
									} );
								}
                            }
						}
					}
				}

				return PartialView( "~/Views/SituazioneDebitoria/subpartial/TblSituazioneDebitoria.cshtml", model );
			}
			catch ( Exception ex )
			{
				return PartialView( "~/Views/Shared/TblError.cshtml", new HandleErrorInfo( ex, "SituazioneDebitoriaController", "LoadTableDebiti" ) );
			}
		}

		[HttpGet]
		public PartialViewResult RiepilogoSpese ()
		{
			try
			{
				double attuale = 0.0;
				double residuoTot = 0.0;
				double totaleMese = 0.0;
				double totale = 0.0;
				int rate = 0;
				SpesaMensileModel model = new SpesaMensileModel();
				DateTime current = DateTime.Now;

				model.MeseCorrente = ( MesiEnum )current.Month;
				model.Year = current.Year;

				if ( SituazioneDebitoriaControllerScope.Instance.ListaDebiti == null ||
					!SituazioneDebitoriaControllerScope.Instance.ListaDebiti.Any() )
				{
					SituazioneDebitoriaControllerScope.Instance.ListaDebiti = GetDataInternal();
				}

				var resultData = SituazioneDebitoriaControllerScope.Instance.ListaDebiti;


				if ( resultData != null && resultData.Any() )
				{
					foreach ( var itm in resultData )
					{
						rate++;
						totaleMese += itm.ImportoRata;
						totale += itm.Addebito;
						residuoTot += itm.ImportoRateResidue;
					}

					attuale = totale - residuoTot;

					model.TotaleRimborsato = attuale;
					model.NumeroRateNelMese = rate;
					model.TotaleSpesaMese = totaleMese;
					model.TotaleSpesaPrevista = totale;
					model.PercentualeCompletamento = ( int )( ( 100 * attuale ) / totale );
				}

				return PartialView( "~/Views/SituazioneDebitoria/subpartial/SpesaMensile.cshtml", model );
			}
			catch ( Exception ex )
			{
				return PartialView( "~/Views/Shared/TblError.cshtml", new HandleErrorInfo( ex, "SituazioneDebitoriaController", "RiepilogoSpese" ) );
			}
		}

		private List<SituazioneDebitoria> GetDataInternal ()
		{
            return SituazioneDebitoriaManager.GetSituazioneDebitoria();
		}
    }

	public class SituazioneDebitoriaControllerScope : SessionScope<SituazioneDebitoriaControllerScope>
	{
		public SituazioneDebitoriaControllerScope ()
		{
		}

		public List<SituazioneDebitoria> ListaDebiti
		{
			get
			{
				return this._listaDebiti;
			}
			set
			{
				this._listaDebiti = value;
			}
		}

		private List<SituazioneDebitoria> _listaDebiti = null;
	}
}