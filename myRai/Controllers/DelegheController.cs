using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using myRaiData;
using System.Data;
using myRaiCommonModel;
using myRaiCommonManager;
using myRaiHelper;
using myRaiServiceHub.Autorizzazioni;

namespace myRai.Controllers
{
    public class DelegheController : BaseCommonController
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Deleghe()
        {
            DelegheModel model = DelegheManager.GetDelegheModel();
            return View(model);
        }

        public ActionResult Elimina(string da, string a, string matr)
        {
            string esito = DelegheManager.EliminaDelega(da,a,matr);//matr delegato!
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { result =esito }
            };
        }

        public ActionResult Dipendenti()
        {
            PresenzaDipendenti model = HomeManager.GetPresenzaDipendenti();
            if ( UtenteHelper.IsBossLiv2( CommonHelper.GetCurrentUserMatricola( ) ) )
                SecLiv( ref model );
            return View("_dipendentiAjax",model);
        }

        [HttpPost]
        public ActionResult save(PresenzaDipendenti model)
        {
            DateTime da;
            DateTime.TryParseExact(model.dataInizioDelega, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out da);
            DateTime a;
            DateTime.TryParseExact(model.dataFineDelega, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out a);
            if (a < da)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "Impostazione date errata" }
                };  
            }

            string esito = DelegheManager.SetDelega(model);

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { result = esito }
            };
        }

        private string GetDesSedeGapp(string codSedeGapp)
        {
            string desSede = "";
            if (!String.IsNullOrWhiteSpace(codSedeGapp))
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    L2D_SEDE_GAPP sede = db.L2D_SEDE_GAPP.FirstOrDefault(x => x.cod_sede_gapp == codSedeGapp);
                    if (sede != null)
                        desSede = sede.desc_sede_gapp;
                }
            }
            return desSede;
        }

        private void SecLiv(ref PresenzaDipendenti model)
        {
            string matricola = CommonHelper.GetCurrentUserPMatricola();
            string mySedeGapp = UtenteHelper.SedeGapp(DateTime.Now);
            string mySedeGappDes = GetDesSedeGapp(mySedeGapp);
            Sedi service = new Sedi();
            service.Credentials = new System.Net.NetworkCredential( CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            CategorieDatoAbilitate response = CommonHelper.Get_CategoriaDato_Net_Cached( 2 );

            if (model == null)
            {
                model = new PresenzaDipendenti();
                model.ListaDipendenti = new List<PresenzaDipendentiPerSede>();
            }
            PresenzaDipendentiPerSede presMySede = new PresenzaDipendentiPerSede();
            presMySede.ListaDipendentiPerSede = new List<DipendentePresenzaAssenza>();
            PresenzaDipendentiPerSede presMy2Liv = new PresenzaDipendentiPerSede();
            presMy2Liv.ListaDipendentiPerSede = new List<DipendentePresenzaAssenza>();

            presMy2Liv.SedeGapp = "2LIV";
            presMy2Liv.DescrizioneSedeGap = "Responsabile 2° livello";

            var mySede = response.CategorieDatoAbilitate_Array.FirstOrDefault(x => x.Codice_categoria_dato == mySedeGapp);
            if (mySede != null)
            {
                var secLivResp = mySede.DT_Utenti_CategorieDatoAbilitate.AsEnumerable().Where(y => y.Field<string>("logon_id") != matricola);
                if (secLivResp.Count() > 0)
                {
                    string sedeGappApp = secLivResp.First().Field<string>("SedeGappAppartenenza");
                    presMy2Liv.SedeGapp = sedeGappApp + "_2";
                    presMy2Liv.DescrizioneSedeGap = GetDesSedeGapp(sedeGappApp)+" - (2° livello)";

                    presMy2Liv.ListaDipendentiPerSede.AddRange(secLivResp
                        .Select(row => new DipendentePresenzaAssenza()
                        {
                            matricola = row.Field<string>("logon_id"),
                            Nominativo = CommonHelper.GetNominativoPerMatricola(row.Field<string>("logon_id").Substring(1))
                        }));
                }
            }

            presMySede.SedeGapp = mySedeGapp+"_2";
            presMySede.DescrizioneSedeGap = mySedeGappDes+" - (2° livello)";
            foreach (var item in response.CategorieDatoAbilitate_Array.Where(x=>x.Codice_categoria_dato!=mySedeGapp))
            {
                presMySede.ListaDipendentiPerSede.AddRange(item.DT_Utenti_CategorieDatoAbilitate
                    .AsEnumerable()
                    .Where(y => y.Field<string>("logon_id") != matricola 
                        && y.Field<string>("sedeGappAppartenenza")==mySedeGapp
                        && !presMySede.ListaDipendentiPerSede.Any(a=>a.matricola==y.Field<string>("logon_id")))
                    .Select(row => new DipendentePresenzaAssenza()
                    {
                        matricola = row.Field<string>("logon_id"),
                        Nominativo = CommonHelper.GetNominativoPerMatricola(row.Field<string>("logon_id").Substring(1))
                    }));
            }

            if (presMySede.ListaDipendentiPerSede.Count() > 0) model.ListaDipendenti.Add(presMySede);
            if (presMy2Liv.ListaDipendentiPerSede.Count() > 0) model.ListaDipendenti.Add(presMy2Liv);
        }
    }
}