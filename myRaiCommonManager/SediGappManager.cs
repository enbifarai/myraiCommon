using myRai.Data.CurriculumVitae;
using myRaiCommonModel;
using myRaiData;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace myRaiCommonManager
{
    public class SediGappManager
    {
        public static SediGappModel GetSediGappModel(string searchName, string searchDesc, string searchCal)
        {
            SediGappModel model = new SediGappModel();
            myRaiCommonTasks.Abilitazioni AB = myRaiCommonTasks.CommonTasks.getAbilitazioni();
            model.Abilitazioni = AB;

            var db = new digiGappEntities();

            var sedigappDB = db.L2D_SEDE_GAPP.ToList();

            foreach (var sede in AB.ListaAbilitazioni)
            {
                SedeGappItem item = new SedeGappItem();
                item.NomeSede = sede.Sede;
                item.DescSede = sede.DescrSede;
                DateTime D = DateTime.Now;

                item.sedeGappDB = sedigappDB.Where(x => x.cod_sede_gapp == sede.Sede
                                  && x.data_inizio_validita <= D && x.data_fine_validita >= D
                                  ).FirstOrDefault();
                model.SediGappList.Add(item);
            }
            model.SediGappList = model.SediGappList.OrderBy(x => x.NomeSede).ToList();
            

            var listCalendario= model.SediGappList.Where (x=>x.sedeGappDB !=null && x.sedeGappDB.CalendarioDiSede!=null)
                .Select(a => a.sedeGappDB.CalendarioDiSede).Distinct().OrderBy(z=>z).ToList();


            List<SelectListItem> listCal = new List<SelectListItem>();
            foreach (string cal in listCalendario)
            {
                listCal.Add(new SelectListItem()
                {
                    Value = cal,
                    Text = cal,
                    Selected = false
                });
            }
            model.listaCalendari = new SelectList(listCal, "Value", "Text");


            if (!string.IsNullOrWhiteSpace(searchName))
            {
                model.SediGappList = model.SediGappList.Where(x => x.NomeSede.ToLower().Contains(searchName.ToLower())).ToList();
            }
            if (!string.IsNullOrWhiteSpace(searchDesc))
            {
                model.SediGappList = model.SediGappList.Where(x => x.DescSede.ToLower().Contains(searchDesc.ToLower())).ToList();
            }
            if (!string.IsNullOrWhiteSpace(searchCal))
            {
                model.SediGappList = model.SediGappList.Where(x => x.sedeGappDB != null && x.sedeGappDB.CalendarioDiSede!=null && x.sedeGappDB.CalendarioDiSede.ToLower() == (searchCal.ToLower())).ToList();
            }

            return model;
        }

        public static void GetDifferenzeOrario()
        {
            //var db = new cv_ModelEntities();
            //string d = DateTime.Now.ToString("yyyy-MM-dd");
            //string query = "SELECT * FROM [LINK_HRIS_HRLIV2].[HR_LIV2].[dbo].[L2D_orario] where data_inizio_validita<='" + 
            //    d + "' and data_fine_validita>='" + d + "'";
            //var orariHRDW= db.Database.SqlQuery<L2D_ORARIO>(query).OrderBy(x=>x.cod_orario).ToList();

            //var dbDigigapp = new myRaiData.digiGappEntities();
            //var data = DateTime.Today;
            //var orariDigigapp= dbDigigapp.L2D_ORARIO.Where(x => x.data_inizio_validita <= data && x.data_fine_validita >= data).ToList();

            //string result = "";

            //foreach (var orarioHrdw in orariHRDW)
            //{
            //    if (! orariDigigapp.Any(x => x.cod_orario == orarioHrdw.cod_orario))
            //    {
            //        result += orarioHrdw.cod_orario + "\r\n";
            //    }
            //}

        }
        public static L2D_SEDE_GAPP_HRDW GetSedeHrdw(string sede)
        {
            var db = new cv_ModelEntities();
            string d = DateTime.Now.ToString("yyyy-MM-dd");
            string query = "SELECT * FROM [LINK_HRIS_HRLIV2].[HR_LIV2].[dbo].[L2D_SEDE_GAPP] where cod_sede_gapp='" + sede + "' and " +
                "data_inizio_validita<='" + d + "' and data_fine_validita>='" + d + "'";

            return db.Database.SqlQuery<L2D_SEDE_GAPP_HRDW>(query).FirstOrDefault();
        }

        public static void AddSede(L2D_SEDE_GAPP_HRDW sedeHrdw)
        {
            myRaiData.L2D_SEDE_GAPP NewSede = new myRaiData.L2D_SEDE_GAPP() { mensa_disponibile = "PC" };

            CommonHelper.Copy(NewSede, sedeHrdw);

            var db_digigapp = new myRaiData.digiGappEntities();

            db_digigapp.L2D_SEDE_GAPP.Add(NewSede);
            db_digigapp.SaveChanges();
        }
    }
}