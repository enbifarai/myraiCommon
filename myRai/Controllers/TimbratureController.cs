using myRaiCommonManager;
using myRaiCommonModel;
using myRaiHelper;
using MyRaiServiceInterface.MyRaiServiceReference1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace myRai.Controllers
{
    public class TimbratureController : BaseCommonController
    {
        public CartellinoTimbratureModel getCartellinoTimbratureModel(int?mese, int? anno, bool? next)
        {
            CartellinoTimbratureModel model;
            if (mese == null)
                model = new CartellinoTimbratureModel()
                {
                    AnnoCorrente = DateTime.Now.Year,
                    MeseCorrenteString = DateTime.Now.ToString("MMMM").ToUpper(),
                    MeseCorrente = DateTime.Now.Month,
                    FrecciaAvanti = true,
                    FrecciaIndietro = true,
                    giorni = getGiorni(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1))
                };
            else
            {
                DateTime D = new DateTime((int)anno, (int)mese, 1, 0, 0, 0);
                if (next==true)
                    D = D.AddMonths(1);
                else if (next==false)
                    D = D.AddMonths(-1);

                model = new CartellinoTimbratureModel()
                {
                    AnnoCorrente = D.Year,
                    MeseCorrenteString = D.ToString("MMMM").ToUpper(),
                    MeseCorrente = D.Month,
                    FrecciaAvanti = true,
                    FrecciaIndietro = true,
                    giorni = getGiorni(D)
                };
            }
            DateTime d1 = new DateTime(model.AnnoCorrente, model.MeseCorrente, 1);
            model.DettaglioTimbrature = getTimbrature(CommonHelper.GetCurrentUserMatricola(),
                d1,
                d1.AddMonths(1).AddDays(-1));

            model.DettaglioPresenze = getPresenze( CommonHelper.GetCurrentUserMatricola(),
                d1,
                d1.AddMonths(1).AddDays(-1));
            
            var db = new myRaiData.digiGappEntities();
            DateTime dnow=DateTime.Now;
            List<string> codiciOrari= model.DettaglioPresenze.Giorni.Select(x => x.CodiceOrario).Distinct().ToList();
            Dictionary<string, int> minutiCodice = new Dictionary<string, int>();
            foreach (string codice in codiciOrari)
            {
                var orario = db.L2D_ORARIO.Where(x => x.cod_orario == codice && x.data_inizio_validita <= dnow && x.data_fine_validita >= dnow)
                    .FirstOrDefault();
                if (orario != null)
                {
                    minutiCodice.Add(codice, (int)orario.prevista_presenza);
                }
            }

            if ( model.DettaglioPresenze != null &&
                model.DettaglioPresenze.Giorni != null &&
                model.DettaglioPresenze.Giorni.Any( ) )
            {
                model.ListaGiorniEvidenza = new CalendarioFerie( );
                model.ListaGiorniEvidenza = FeriePermessiManager.GetCalendarioSituazioneEccezioni( d1.Year , d1.Month );

                model.ListaEccezioniMese = new List<DaEvidenziare>( );
                DateTime start;
                DateTime end;
                if ( mese == null )
                {
                    start = new DateTime( DateTime.Now.Year , DateTime.Now.Month , 1 , 0 , 0 , 0 );
                }
                else
                {
                    start = new DateTime( anno.GetValueOrDefault( ) , mese.GetValueOrDefault( ) , 1 , 0 , 0 , 0 );
                }

                end = start.AddMonths( 1 );
                end = end.AddDays( -1 );

                try
                {
                    string matricola = CommonHelper.GetCurrentUserMatricola( );

                    model.ListaEccezioniMese = ( from e in db.MyRai_Eccezioni_Richieste
                                                 join r in db.MyRai_Richieste
                                                 on e.id_richiesta equals r.id_richiesta
                                                 where r.matricola_richiesta == matricola &&
                                                 ( e.data_eccezione >= start && e.data_eccezione <= end )
                                                 select new DaEvidenziare( )
                                                 {
                                                     Cod_Eccezione = e.cod_eccezione ,
                                                     Data = e.data_eccezione ,
                                                     StatoRichiesta = (myRaiHelper.EnumStatiRichiesta) e.id_stato
                                                 } ).ToList( );
                }
                catch (Exception ex)
                {
                    model.ListaEccezioniMese = new List<DaEvidenziare>( );
                }

            foreach (var item in model.DettaglioPresenze.Giorni)
            {
                if (item.OreServizio != null && item.OreServizio.Contains("."))
                {
                    int minutiLavorati = (Convert.ToInt32(item.OreServizio.Split('.')[0]) * 60) +
                        Convert.ToInt32(item.OreServizio.Split('.')[1]);

                    int minutiDaLavorare = minutiCodice.Where(x => x.Key == item.CodiceOrario).Select(x => x.Value).FirstOrDefault();
                    item.CopreOrario = (minutiLavorati >= minutiDaLavorare);
                }
            }
            }

            return model;
        }

        public ActionResult Index()
        {
            SessionHelper.Set(SessionVariables.AnnoFeriePermessi, null);
            CartellinoTimbratureModel model = getCartellinoTimbratureModel(null, null, true);
            return View(model);
        }

        public ActionResult getCalendario(int? mese , int? anno, bool? next)
        {
            CartellinoTimbratureModel model = this.getCartellinoTimbratureModel(mese, anno, next);
            return View(model);
        }

        public List<giorno> getGiorni(DateTime D)
        {
            List<giorno> lg = new List<giorno>();
            int month = D.Month;
            while (month == D.Month)
            {
                lg.Add(new giorno() { dayNumber=D.Day, day=D.ToString("ddd").ToUpper()  });
                D=D.AddDays(1);
            }
            return lg;
        }

        private GetTimbratureMeseResponse getTimbrature(string matricola, DateTime d1, DateTime d2)
        {
            MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client cl = new MyRaiService1Client();
            try
            {
                cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential( CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
                return cl.GetTimbratureMese(matricola, d1, d2);
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new myRaiData.MyRai_LogErrori() {
                 applicativo="portale",
                  data=DateTime.Now,
                   matricola=matricola,
                   provenienza = "getTimbrature",
                    error_message=ex.ToString()
                });
                return new GetTimbratureMeseResponse();
            }
        }

        private GetSchedaPresenzeMeseResponse getPresenze(string matricola, DateTime d1, DateTime d2)
        {
            MyRaiService1Client cl = new MyRaiService1Client();
            cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential( CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
            return cl.GetSchedaPresenzeMese(matricola, d1, d2);
        }
    }
}