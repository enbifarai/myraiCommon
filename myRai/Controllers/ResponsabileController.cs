using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using myRaiCommonManager;
using myRaiCommonModel;
using myRaiData;
using myRaiHelper;

namespace myRai.Controllers
{
    public class ResponsabileController : BaseCommonController
    {
        ModelDash pr = new ModelDash();

        public ActionResult totaledaapprovare()
        {
            string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            Autorizzazioni.Sedi SEDI = new Autorizzazioni.Sedi();

            //giornata di oggi
            int i; string matricola, pmatricola;
            if (int.TryParse(userName.Substring(5), out i))
            {
                matricola = userName.Substring(5);
                pmatricola = userName.Substring(4);
            }
            else
            {
                matricola = "103650";
                pmatricola = "P103650";
            }

            var db = new digiGappEntities();
            pr.menuSidebar = UtenteHelper.getSidebarModel();
            pr.elencoProfilieSedi = new daApprovareModel( CommonHelper.GetCurrentUserPMatricola( ) , CommonHelper.GetCurrentUserMatricola( ) , true , "01" , true );
            pr.SediGappAbilitateConEccezioni = new Dictionary<string, string[]>();

            List<string> listaSedi = CommonHelper.GetSediL1( CommonHelper.GetCurrentUserPMatricola( ) ).Select( x => ( x.Length > 5 ? x.Substring( 0 , 5 ) : x ) ).ToList( );
            foreach (string sede in listaSedi)
            {
              
                int NumeroEcc = db.MyRai_Richieste.Where(x => x.codice_sede_gapp == sede && (
                    x.id_stato.Equals((int)EnumStatiRichiesta.InApprovazione) || x.id_stato.Equals((int)EnumStatiRichiesta.Approvata) || x.id_stato.Equals((int)EnumStatiRichiesta.Rifiutata))).Count();
                if (NumeroEcc > 0 &&
                    !pr.SediGappAbilitateConEccezioni.ContainsKey(sede))
                {
                    string[] st = new string[2];
                    st[0] = db.L2D_SEDE_GAPP.Where(x => x.cod_sede_gapp == sede).Select(x => x.desc_sede_gapp).FirstOrDefault();
                    st[1] = NumeroEcc.ToString();
                    pr.SediGappAbilitateConEccezioni.Add(sede, st);
                }
            }

            return View(pr);
        }        

        public ActionResult GetEccezioniHTML(string sede, int da, string ActiveList = "")
        {
            int stato = 0;
            if (ActiveList == "btabdapprovare")
                stato = (int)EnumStatiRichiesta.InApprovazione;
            if (ActiveList == "btabdapprovati")
                stato = (int)EnumStatiRichiesta.Approvata;
            if (ActiveList == "btabrifiutati")
                stato = (int)EnumStatiRichiesta.Rifiutata;

            ViewBag.listaAttiva = ActiveList;

            ModelDash model = HomeManager.GetDaApprovareModel(pr, true, da, sede, stato);

            ViewBag.TotEccezioniDaApprovare = model.elencoProfilieSedi.TotEccezioniDaApprovare;
            ViewBag.TotEccezioniApprovate = model.elencoProfilieSedi.TotEccezioniApprovate;
            ViewBag.TotEccezioniRifiutate = model.elencoProfilieSedi.TotEccezioniRifiutate;

            if (ActiveList == "")
                return View("TotaleListaEccezioni", model);
            else
            {
                return View("EccezioniScrolling", model);
            }
        }
    }
}