using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace myRaiCommonModel
{
    public class HotelModel
    {
        public string Alert { get; set; }

        public int idRegione { get; set; }
        public SelectList regioni_list { get; set; }
        public List<myRaiData.News> listaNews { get;set;}

        public List<HotelVisitato> AlberghiVisitati { get; set; }
    }
    public class HotelsModel
    {
        public List<HotelSingoloModel> hotels { get; set; }
    }

    public class HotelSingoloModel
    {
        public myRaiData.Hotel hotel { get; set; }
        public int? Distanza { get; set; }
    }

    public class HotelVisitato
    {
        public string FoglioViaggio { get; set; }
        public string CodAlbergo { get; set; }
        public DateTime DataVisita { get; set; }
        public int NumNotti { get; set; }
        public myRaiData.Hotel Albergo { get; set; }
    }

   
    public class Citta
    { 
        public int Id_Città {get;set;}
        public int Id_Provincia {get;set;}
        public int Codice_Istat {get;set;}
        public int  Altitudine{get;set;}
        public long Popolazione {get;set;}
        public int  Id_Provincia1 {get;set;}
        public int  Id_Regione {get;set;}
        public int  Id_Regione1 {get;set;}
        public int  Flag_Capoluogo {get;set;}
        public string Nome_Città {get;set;}
        public string Latitudine {get;set;}
        public string Longitudine {get;set;}
        public string Nome_Provincia {get;set;}
        public string Sigla_Provincia {get;set;}
        public string Nome_Regione {get;set;}
        public Boolean Capoluogo{get;set;}
        public Double Superficie {get;set;}
   }
  
}