using System;
using System.Collections.Generic;
using myRaiCommonTasks;

namespace myRaiCommonModel
{
    public class SediGappModel
    {
        public SediGappModel()
        {
            SediGappList = new List<SedeGappItem>();
        }

        public List<SedeGappItem> SediGappList { get; set; }
        public Abilitazioni Abilitazioni { get; set; }


        public System.Web.Mvc.SelectList listaCalendari { get; set; }
        public string searchCal { get; set; }
        public string searchDesc { get; set; }
        public string searchName { get; set; }

    }

    public class SedeGappItem
    {
        public myRaiData.L2D_SEDE_GAPP sedeGappDB { get; set; }
        public string NomeSede { get; set; }
        public string DescSede { get; set; }

    }

    public partial class L2D_SEDE_GAPP_HRDW
    {
        public int sky_sede_gapp { get; set; }
        public string cod_sede_gapp { get; set; }
        public string desc_sede_gapp { get; set; }
        public string cod_rsu { get; set; }
        public string desc_rsu { get; set; }
        public Nullable<System.DateTime> data_inizio_validita { get; set; }
        public Nullable<System.DateTime> data_fine_validita { get; set; }
        public string flag_ivt { get; set; }
        public string flag_presenza_sirio { get; set; }
        public string minimo_car { get; set; }
        public string cod_serv_cont { get; set; }
        public string cod_sede { get; set; }
        public Nullable<System.DateTime> partenza_fase_2 { get; set; }
        public Nullable<System.DateTime> partenza_fase_3 { get; set; }
        public Nullable<System.DateTime> scadenza { get; set; }
        public string flg_ultimo { get; set; }
        public Nullable<System.DateTime> data_inizio_val { get; set; }
        public System.DateTime Data_Ins { get; set; }
        public Nullable<System.DateTime> Data_Fine_Val { get; set; }
        public Nullable<System.DateTime> Data_Agg { get; set; }
        public Nullable<System.DateTime> Data_Elim { get; set; }

    }
}