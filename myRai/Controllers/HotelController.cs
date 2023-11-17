using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data;
using myRaiData;
using System.Data.Objects.SqlClient;
using System.Globalization;
using myRai.Data.CurriculumVitae;
using myRaiCommonModel;
using myRaiHelper;

namespace myRai.Controllers
{
    public class HotelController : Controller //: BaseCommonController
    {        
        public ActionResult Index()
        {
            HotelModel model = new HotelModel();
            model.regioni_list = HotelManager.getRegioni();
            model.AlberghiVisitati = HotelManager.GetAlberghiVisitati();
            DateTime dnow=DateTime.Now;

            model.listaNews = new alberghiEntities().News.Where(x => x.Flag_Attivo == "1").AsEnumerable()
                .Where(a => DateTime.ParseExact(a.Data_Inizio, "yyyyMMdd", CultureInfo.InvariantCulture) <= dnow)
                .Where(a => DateTime.ParseExact(a.Data_Fine, "yyyyMMdd", CultureInfo.InvariantCulture) >= dnow)
                .ToList();

            return View(model);
        }

        public ActionResult getProvince(int idreg)
        {
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { result = HotelManager.getProvince(idreg) }
            };
        }

        public ActionResult getComuni(int idprov)
        {
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { result = HotelManager.getComuni(idprov) }
            };
        }

        public ActionResult getHotels(int idcomune)
        {
            List<HotelSingoloModel> model = HotelManager.getHotels(idcomune);
            return View(model);
        }

        public ActionResult getHotelsEntro(int idcomune, int km)
        {
            List<HotelSingoloModel> model = HotelManager.getHotelsEntro(idcomune,km);
            return View( "~/Views/Hotel/getHotels.cshtml",model);
           
        }

        //Azione per questionario hotel
        public ActionResult getRadioListHotels(int idDomanda, int idcomune, int km)
        {
            List<HotelSingoloModel> model = null;
            if (km==0)
                model = HotelManager.getHotels(idcomune);
            else
                model = HotelManager.getHotelsEntro(idcomune, km);

            model.Sort(delegate(HotelSingoloModel hotel1, HotelSingoloModel hotel2)
            {
                return hotel1.hotel.Nome.CompareTo(hotel2.hotel.Nome);
            });

            ViewData.Add("IdDomanda",idDomanda);

            return View(model);
        }
    }

    public class HotelManager
    {
        public static alberghiEntities dbh = new alberghiEntities();

        public static SelectList getRegioni()
        {
            var list = dbh.Regioni.OrderBy(x => x.Nome_Regione)
                .Select(a =>
                  new SelectListItem() { Text = a.Nome_Regione, Value = SqlFunctions.StringConvert((decimal)a.Id_Regione) });

            return new SelectList(list, "Value", "Text");
        }

        public static SelectList getProvince(int idreg)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list = dbh.Province
                .Where(x => x.Id_Regione == idreg)
                .Select(a => new SelectListItem()
                {
                    Text = a.Nome_Provincia,
                    Value = SqlFunctions.StringConvert((decimal)a.Id_Provincia)
                }).ToList();

            return new SelectList(list, "Value", "Text");
        }

        public static SelectList getComuni(int idprov)
        {

            List<SelectListItem> list = new List<SelectListItem>();
            list = dbh.Città
                .Where(x => x.Id_Provincia == idprov 
                    //&& x.Hotel.Count > 0
                )
                .Select(a => new SelectListItem()
            {
                Text = a.Nome_Città,
                Value = SqlFunctions.StringConvert((decimal)a.Id_Città)
            }).ToList();

            return new SelectList(list, "Value", "Text");
        }

      
        public static List<HotelSingoloModel> getHotelConvenzioniValide(List<HotelSingoloModel> lista)
        {
            var listaConvenzioni = dbh.Convenzioni.ToList();
            int now = Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd"));
           
            var convValide = listaConvenzioni.Where(x => Convert.ToInt32(x.Data_Inizio) <= now 
                && Convert.ToInt32(x.Data_Fine) >= now).ToList();

            lista.RemoveAll(x => ! convValide.Contains(x.hotel.Convenzioni.FirstOrDefault()));
            return lista;
        }
        public static List<HotelSingoloModel> getHotelsEntro(int idComune,int km)
        {
            List<HotelSingoloModel> ListaHotelComune = getHotels(idComune);

            var ci = dbh.Città.Where(x => x.Id_Città == idComune).FirstOrDefault();
            System.Device.Location.GeoCoordinate gcitta = null;

            try
            {
                double latCitta = Convert.ToDouble(ci.Latitudine, System.Globalization.CultureInfo.InvariantCulture);
                double lonCitta = Convert.ToDouble(ci.Longitudine, System.Globalization.CultureInfo.InvariantCulture);
                gcitta = new System.Device.Location.GeoCoordinate()
                {
                    Latitude = latCitta,
                    Longitude = lonCitta
                };
            }
            catch (Exception ex)
            {
            }

            var model = dbh.Hotel.Where(x => x.Id_Città != idComune)
                                .Select(x => new HotelSingoloModel() { hotel = x })
                                .ToList();
          
            if (gcitta != null)
            {
                foreach (var item in model)
                {
                    try
                    {
                        if (String.IsNullOrWhiteSpace(item.hotel.Latitudine) ||
                                        String.IsNullOrWhiteSpace(item.hotel.Longitudine))
                            item.Distanza = null;
                        else
                            item.Distanza = (int)(gcitta.GetDistanceTo(new System.Device.Location.GeoCoordinate()
                            {
                                Latitude = Convert.ToDouble(item.hotel.Latitudine,
                                                    System.Globalization.CultureInfo.InvariantCulture),
                                Longitude = Convert.ToDouble(item.hotel.Longitudine,
                                                    System.Globalization.CultureInfo.InvariantCulture)
                            }) / (double)1000);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            ListaHotelComune.AddRange(model.Where(x => x.Distanza <= km).ToList());
            ListaHotelComune = getHotelConvenzioniValide(ListaHotelComune);

            return ListaHotelComune.OrderBy(x => x.Distanza).ToList();
        }

        public static List<HotelSingoloModel> getHotels(int idComune)
        {
            var ci = dbh.Città.Where(x => x.Id_Città == idComune).FirstOrDefault();
            System.Device.Location.GeoCoordinate gcitta = null;

            try
            {
                double latCitta = Convert.ToDouble(ci.Latitudine,System.Globalization.CultureInfo.InvariantCulture);
                double lonCitta = Convert.ToDouble(ci.Longitudine, System.Globalization.CultureInfo.InvariantCulture);
                gcitta = new System.Device.Location.GeoCoordinate()
                        {
                            Latitude = latCitta,
                            Longitude = lonCitta
                        };
            }
            catch (Exception ex)
            {
            } 

            var model = dbh.Hotel
                .Where(x => x.Id_Città == idComune)
                                .Select(x => new HotelSingoloModel() {hotel = x})
                                .ToList();
            if (gcitta != null)
            {
                foreach (var item in model)
                {
                    try
                    {
                        if (String.IsNullOrWhiteSpace(item.hotel.Latitudine) ||
                                        String.IsNullOrWhiteSpace(item.hotel.Longitudine))
                            item.Distanza = null;
                        else
                            item.Distanza = (int)(gcitta.GetDistanceTo(new System.Device.Location.GeoCoordinate()
                            {
                                Latitude = Convert.ToDouble(item.hotel.Latitudine, 
                                                    System.Globalization.CultureInfo.InvariantCulture),
                                Longitude = Convert.ToDouble(item.hotel.Longitudine, 
                                                    System.Globalization.CultureInfo.InvariantCulture)
                            }) / (double)1000);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            model = getHotelConvenzioniValide(model);

            return model.OrderBy (x=>x.Distanza).ToList();        
        }

        public static List<HotelVisitato> GetAlberghiVisitati()
        {
            List<HotelVisitato> alberghi = new List<HotelVisitato>();

            string matr = CommonHelper.GetCurrentUserMatricola();
            DateTime dataLimite = DateTime.Now.AddDays(-30);

            using (var db = new cv_ModelEntities())
            {
                string query = "SELECT alberghi.foglio_viaggio as FoglioViaggio, "+
                               "alberghi.COD_ALBERGO as CodAlbergo, "+
                               "MIN(alberghi.DATA) as DataVisita, " +
                               "COUNT(*) as NumNotti "+
                               "FROM [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_TRAS_alberghi]	as alberghi " +
                               "join [LINK_HRIS_HRLIV1].[HR_LIV1].[dbo].[L1C_TRAS_FOGLIO_VIAGGIO] as viaggi on viaggi.num_fog=alberghi.foglio_viaggio " +
                               "WHERE viaggi.matricola_dp = '##MATR##' AND alberghi.COD_ALBERGO<>'' AND alberghi.data> {d '##DATA_LIM##'} " +
                               "GROUP BY alberghi.FOGLIO_VIAGGIO, alberghi.COD_ALBERGO " +
                               "ORDER BY MIN(alberghi.DATA )";

                query = query.Replace("##MATR##", matr).Replace("##DATA_LIM##", dataLimite.ToString("yyyy-MM-dd"));
                alberghi = db.Database.SqlQuery<HotelVisitato>(query).ToList();
            }

            using (var dbd = new digiGappEntities())
            {
                using (var dbh = new alberghiEntities())
                {
                    int idFormHotel = dbd.MyRai_FormPrimario.FirstOrDefault(x => x.MyRai_FormTipologiaForm.tipologia == "Hotel").id;
                    int countAlb = alberghi.Count;
                    for (int i = countAlb-1; i >=0; i--)
                    {
                        var albergo = alberghi[i];

                        if (dbd.MyRai_FormCompletati.Any(x => x.id == idFormHotel && x.matricola == matr && x.valore1 == albergo.CodAlbergo))
                            alberghi.RemoveAt(i);
                        else
                            albergo.Albergo = dbh.Hotel.Include("Città").Include("Categoria").Include("Città.Province").FirstOrDefault(y => y.Id_Hotel == albergo.CodAlbergo);
                    }
                }
            }
            return alberghi;
        }
    }
}