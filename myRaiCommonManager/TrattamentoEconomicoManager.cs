using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using myRaiCommonModel;
using myRaiData.Incentivi;
using myRaiHelper;
using Newtonsoft.Json;

namespace myRaiCommonManager
{
    public class TrattamentoEconomicoManager
    {
        public static List<AttributiAggiuntivi> GetDatiJson(int IDRichiesta)
        {
            var db = new IncentiviEntities();
            var R = db.XR_MAT_RICHIESTE.Where(x => x.ID == IDRichiesta).FirstOrDefault();
            return GetDatiJson(R);
        }
        public static List<AttributiAggiuntivi> GetDatiJson(XR_MAT_RICHIESTE Rich)
        {
            if (Rich == null)
                return null;
            else
                return GetDatiJson(Rich.CUSTOM_JSON);
        }
        public static List<AttributiAggiuntivi> GetDatiJson(string JsonText)
        {
            if (String.IsNullOrWhiteSpace(JsonText)) return null;
            else return JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(JsonText);
        }
        public static TrattamentoEconomicoModel GetTrattamentoEconomicoModel(SearchTeModel search)
        {
            var db = new IncentiviEntities();
            var query = db.XR_MAT_RICHIESTE.Where(x => x.CUSTOM_JSON != null && x.CUSTOM_JSON.Trim() != "");
            TrattamentoEconomicoModel model = new TrattamentoEconomicoModel();

            if (!String.IsNullOrWhiteSpace(search.matr))
            {
                query = query.Where(x => x.MATRICOLA == search.matr);
            }
            if (!String.IsNullOrWhiteSpace(search.mese))
            {
                DateTime D1 = new DateTime(Convert.ToInt32(search.mese.Split('/')[1]), Convert.ToInt32(search.mese.Split('/')[0]), 1);
                DateTime D2 = D1.AddMonths(1);
                query = query.Where(x => x.DATA_INVIO_RICHIESTA > D1 && x.DATA_INVIO_RICHIESTA < D2);
            }
            if (!String.IsNullOrWhiteSpace(search.tipo))
            {
                int idCategoria = Convert.ToInt32(search.tipo);
                query = query.Where(x => x.CATEGORIA == idCategoria);
            }
            var Richieste =
                (from ric in query

                 join st in db.XR_WKF_OPERSTATI on ric.ID equals st.ID_GESTIONE
                 into stati

                 orderby ric.NOMINATIVO, ric.DATA_INVIO_RICHIESTA
                 select new TrattamentoEconomicoItem()
                 {
                     Richiesta = ric,
                     InCarico = stati.Where(x => x.ID_STATO == (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin)
                                      .Select(x => x.COD_USER).FirstOrDefault(),
                     TaskAvviati = ric.XR_MAT_TASK_IN_CORSO.Any()
                 }).ToList();

            model.RichiesteInCaricoAme = new TEtabModel()
            {
                ListaRichieste = Richieste.Where(x => x.InCarico == CommonHelper.GetCurrentUserMatricola()).ToList(),
                index = "1"
            };

            model.RichiesteInCaricoANessuno = new TEtabModel()
            {
                ListaRichieste = Richieste.Where(x => x.InCarico == null).ToList(),
                index = "2"
            };

            model.RichiesteInCaricoAdAltri = new TEtabModel()
            {
                ListaRichieste = Richieste.Where(x => x.InCarico != CommonHelper.GetCurrentUserMatricola()
                                && x.InCarico != null).ToList(),
                index = "3"
            };


            return model;
        }

        public static PopupTeModel GetPopupTEModel(int id)
        {
            PopupTeModel model = new PopupTeModel();
            var ric = new IncentiviEntities().XR_MAT_RICHIESTE.Where(x => x.ID == id).FirstOrDefault();
            model.Richiesta = ric;
            model.ListaAttributiJson = GetDatiJson(ric);
            model.InCaricoAMe = ChiInCarico(ric) == CommonHelper.GetCurrentUserMatricola();
            return model;
        }
        public static PopupTeAssegnaModel GetPopupTEassegnaModel(int id)
        {
            var ric = new IncentiviEntities().XR_MAT_RICHIESTE.Where(x => x.ID == id).FirstOrDefault();
            PopupTeAssegnaModel model = new PopupTeAssegnaModel()
            {
                Richiesta = ric,
                PuoiRilasciare = IsInCaricoAMe(ric),
                PuoiPrendereIncarico = InCaricoANessuno(ric) || IsInCaricoACollegaAssente(ric),
                PossibileAssegnare = true//da vedere abilitazioni
            };
            model.AssegnatariPossibili = myRaiHelper.AuthHelper.GetAllEnabledAs("HRIS", "01GEST")
               .Where(x => x.Matricola != CommonHelper.GetCurrentUserMatricola()).ToList();
            model.InCaricoAMe = ChiInCarico(ric) == CommonHelper.GetCurrentUserMatricola();
            return model;
        }
        public static TrattamentoEconomicoIndexModel GetTrattamentoEconomicoIndexModel()
        {
            var db = new IncentiviEntities();
            TrattamentoEconomicoIndexModel model = new TrattamentoEconomicoIndexModel();
            model.ListaCategorie = db.XR_MAT_CATEGORIE.Where(x => x.ECCEZIONE == null).OrderBy(x => x.TITOLO).ToList();
            return model;
        }
        public static string ChiInCarico(XR_MAT_RICHIESTE Richiesta)
        {
            var stato = Richiesta.XR_WKF_OPERSTATI.OrderByDescending(x => x.ID_STATO).FirstOrDefault();
            if (stato == null || stato.ID_STATO != (int)MaternitaCongediManager.EnumStatiRichiesta.InCaricoAmmin)
                return null;
            else return stato.COD_USER;
        }
        public static bool InCaricoANessuno(XR_MAT_RICHIESTE Richiesta)
        {
            return ChiInCarico(Richiesta) == null;
        }
        public static bool IsInCaricoAMe(XR_MAT_RICHIESTE Richiesta)
        {
            return ChiInCarico(Richiesta) == CommonHelper.GetCurrentUserMatricola();
        }
        public static bool IsInCaricoACollegaAssente(XR_MAT_RICHIESTE Richiesta)
        {
            string matr = ChiInCarico(Richiesta);
            if (String.IsNullOrWhiteSpace(matr) || matr == CommonHelper.GetCurrentUserMatricola())
                return false;

            var resp = HomeManager.GetEccezioni(DateTime.Now.ToString("ddMMyyyy"), matr);

            if (!resp.eccezioni.Any(x => x.cod.Trim() == "SW") && !resp.timbrature.Any())
            {
                return true;
            }

            return false;
        }
    }
    public class DatiTE {
        
        public string name { get; set; }
        public string value { get; set; }
    }
    public class DatiTracciatoTE
    {
        public Dictionary<string, string> DatiUtente { get; set; }
        public List<ItemTracciatoTE> Items { get; set; }

        public string GetValoreCampo(string Campo, int IDtracc)
        {
            var item = Items.Where(x => x.NomeCampoTracciato.Trim().ToUpper() == Campo.Trim().ToUpper()
                        && x.IDtracciato == IDtracc).FirstOrDefault();
            if (item == null) throw new Exception("Definizioni campo " + Campo + " non trovate");

            return DatiUtente.Where(x => x.Key == item.NomeCampoDatiUtente).Select(x => x.Value).FirstOrDefault();
        }
    }
    public class ItemTracciatoTE
    {
        public string NomeCampoTracciato { get; set; }
        public string NomeCampoDatiUtente { get; set; }
        public int IDtracciato { get; set; }
        public string MetodoConversione { get; set; }
    }
}
