using myRaiData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.Net;
using ClosedXML.Excel;
using System.IO;
using myRaiCommonModel.raiplace;
using myRaiHelper;

namespace myRai.Controllers
{
    public class CampagnaAbbonamentiController : BaseCommonController
    {
        public ActionResult Index()
        {
            digiGappEntities db = new digiGappEntities();
            try
            {
                var dbabbonamenti = db.B2RaiPlace_Abbonamento_Campagna.AsQueryable();
                List<CampagnaAbbonamentiModel> listaCampagna = CreaListaCampagne(dbabbonamenti.ToList());
                var dbabbonamenti2 = db.B2RaiPlace_Abbonamento_Richieste.Where(a=> a.DataGiornoInizio >= DateTime.Now).AsQueryable();
                MyAbbonamenti listaabbonamenti = CreaListaAbbonamenti(dbabbonamenti2.OrderByDescending(x => x.DataGiornoInizio).ToList());
                TotalAbbonamnetiModel modello = new TotalAbbonamnetiModel();
                modello.Abbonamenti = listaabbonamenti;
                modello.ListCampagnaAbbonamenti = listaCampagna;
                modello.EnabledDelete = CommonHelper.GetParametro<string>( EnumParametriSistema.AbbonamentiMatricoleCancellazione ).Contains( CommonHelper.GetCurrentUserMatricola( ) );
                string[] SoppressePOH = CommonHelper.GetParametro<String>( EnumParametriSistema.SoppressePOH ).Trim( ).ToUpper( ).Split( ',' );

                return View("~/Views/RaiPlace/CampagnaAbbonamenti/Index.cshtml", modello);
            }
            catch (Exception ex)
            {

                return Content(ex.Message);
            }
        }

        public IEnumerable<AbbonamentiCvsRomaModel> GetAbbonamentiRoma(MyAbbonamenti abbonamento)
        {
            digiGappEntities db = new digiGappEntities();
            List<AbbonamentiCvsRomaModel> listarichieste = new List<AbbonamentiCvsRomaModel>();
            foreach (AbbonamentiModel abb in abbonamento.Abbonamenti)
            {
                    int idRichiesta = Convert.ToInt32(abb.idAbbonamento);
                    B2RaiPlace_Abbonamento_Richieste  richiesta= db.B2RaiPlace_Abbonamento_Richieste.Where(a => a.Id_Richiesta == idRichiesta).FirstOrDefault();
                    AbbonamentiCvsRomaModel model = new AbbonamentiCvsRomaModel();
                    model.ATAC_PATRON_ID = richiesta.Matrciola;
                    model.SALUTATION="";
                    model.FIRST_NAME=richiesta.Nome;
                    model.LAST_NAME=richiesta.Cognome;
                    model.CARE_OF_ADDRESSEE="";
                    model.FLAT_LOCATION="";
                    model.STREET_NAME = richiesta.Indirizzo;
                    model.CITY =richiesta.Comune;
                    model.PROVINCE=richiesta.Provincia;
                    model.POST_CODE= richiesta.Cap;
                    model.NATIONALITY= richiesta.Nazione;
                    model.GENDER= richiesta.Genere;
                    model.DATE_OF_BIRTH = richiesta.DataNascita!= null ? Convert.ToString(richiesta.DataNascita) : "";
                    model.PLACE_OF_BIRTH= richiesta.ComuneNascita;
                    model.FISCAL_NUMBER= richiesta.CF;
                    model.HOME_PHONE_NUMBER= richiesta.Telefono;
                    model.OFFICE_PHONE_NUMBER="";
                    model.MOBILE_PHONE_NUMBER= richiesta.Cellulare;
                    model.FAX_NUMBER = "";
                    model.E_MAIL=richiesta.Email;
                    model.NO_MAILING="";
                    model.REGISTRATION_DATE=richiesta.DataRichiesta.ToString();
                    model.PASS_TYPE="19";
                    model.PATRON_CLASS = "8";
                    model.ID = richiesta.Id_Richiesta.ToString();
                    model.VALIDITA= richiesta.DataGiornoInizio.ToString();
                    model.RATE= richiesta.NumeroRate.ToString(); 
                    model.RINNOVO= richiesta.Flag_Rinnovo?"si":"no";
                    model.tipologia= richiesta.B2RaiPlace_Abbonamento_ZoneAbbonamento.ZoneAbbonamento; 
                    model.metrebus= richiesta.NumeroAbbonamento;
                          //PASS_TYPE, PATRON_CLASS, 
                    listarichieste.Add(model);
            }
            return listarichieste;
        }

        public ActionResult Esporta()
        {
            digiGappEntities db = new digiGappEntities();
            try
            {
                MyAbbonamenti abbonamenti = ((MyAbbonamenti)Session["ListaRicercaAbbonamenti"]);
                if (abbonamenti.CittaAbbonamento.ToUpper() == "ROMA")
                {
                    IEnumerable<AbbonamentiCvsRomaModel> abbo = GetAbbonamentiRoma(abbonamenti);
                    return new CsvFileResult<AbbonamentiCvsRomaModel>(abbo, "tblAbbonamentiRM.CSV"); 
                }
                else
                {
                    XLWorkbook workbook = new XLWorkbook();
                    var worksheet = workbook.Worksheets.Add("RichiesteAbbonamenti CV");

                    int counter = 0;
                    worksheet.Cell(2, 1).Value = "N°";
                    worksheet.Cell(2, 2).Value = "NOME";
                    worksheet.Cell(2, 3).Value = "COGNOME";
                    worksheet.Cell(2, 4).Value = "CODICE FISCALE";
                    worksheet.Cell(2, 5).Value = "COMUNE NASCITA";
                    worksheet.Cell(2, 6).Value = "PROVINCIA NASCITA";
                    worksheet.Cell(2, 7).Value = "DATA NASCITA";
                    worksheet.Cell(2, 8).Value = "COMUNE RESIDENZA";
                    worksheet.Cell(2, 9).Value = "PROV. RES.";
                    worksheet.Cell(2, 10).Value = "INDIRIZZO RESIDENZA";
                    worksheet.Cell(2, 11).Value = "CAP RES.";
                    worksheet.Cell(2, 12).Value = "TELEFONO";
                    worksheet.Cell(2, 13).Value = "INDIRIZZO MAIL";
                    worksheet.Cell(2, 14).Value = "NUMERO BIP CARD";
                    worksheet.Cell(2, 15).Value = "PERCORSO DA";
                    worksheet.Cell(2, 16).Value = "PERCORSO A";
                    worksheet.Cell(2, 17).Value = "N. RATE";
                    worksheet.Cell(2, 18).Value = "MATRICOLA";
                    worksheet.Row(2).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Row(2).Style.Font.Bold = true;
                    worksheet.Row(1).Style.Fill.BackgroundColor = XLColor.BallBlue;
                    worksheet.Row(1).Style.Fill.PatternColor = XLColor.BallBlue;
                    worksheet.Row(1).Style.Font.Bold = true;
                    foreach (AbbonamentiModel abb in abbonamenti.Abbonamenti)
                    {
                        counter++;
                        
                        int idRichiesta = Convert.ToInt32(abb.idAbbonamento);
                        B2RaiPlace_Abbonamento_Richieste richiesta = db.B2RaiPlace_Abbonamento_Richieste.Where(a => a.Id_Richiesta == idRichiesta).FirstOrDefault();
                        worksheet.Cell(2 + counter, 1).SetValue<int>(richiesta.Id_Richiesta);
                        worksheet.Cell(2 + counter, 2).Value = richiesta.Nome.ToString();
                        worksheet.Cell(2 + counter, 3).Value = richiesta.Cognome.ToString();
                        worksheet.Cell(2 + counter, 4).Value = richiesta.CF.ToString();
                        worksheet.Cell(2 + counter, 5).Value = richiesta.ComuneNascita.ToString();
                        worksheet.Cell(2 + counter, 6).Value = richiesta.ProvinciaNascita.ToString();
                        worksheet.Cell(2 + counter, 7).Value = richiesta.DataNascita.ToString();
                        worksheet.Cell(2 + counter, 8).Value = richiesta.Comune.ToString();
                        worksheet.Cell(2 + counter, 9).Value = richiesta.Provincia.ToString();
                        worksheet.Cell(2 + counter, 10).Value = richiesta.Indirizzo.ToString();
                        worksheet.Cell(2 + counter, 11).Value = richiesta.Cap.ToString();
                        worksheet.Cell(2 + counter, 12).Value = richiesta.Telefono.ToString();
                        worksheet.Cell(2 + counter, 13).Value = richiesta.Email?? "@";
                        worksheet.Cell(2 + counter, 14).Value = richiesta.NumeroBipCard?? "";
                        worksheet.Cell(2 + counter, 15).Value = richiesta.PercorsoDa?? "";
                        worksheet.Cell(2 + counter, 16).Value = richiesta.PercorsoA?? "";
                        worksheet.Cell(2 + counter, 17).SetValue<int>(richiesta.NumeroRate);
                        worksheet.Cell(2 + counter, 18).SetValue<string>(richiesta.Matrciola);

                        //worksheet.Cell(1 + counter, 1).SetValue<string>(item.Item1);
                        //worksheet.Cell(1 + counter, 2).Value = item.Item2.ToString() + " %";
                    }
            
                    worksheet.Columns().AdjustToContents();
                    if (String.IsNullOrWhiteSpace(abbonamenti.VettoreAbbonamento))
                    {
                        worksheet.Cell(1, 1).Value = "RICHIESTA ABBONAMENTI";
                    }
                    else
                    {
                        int idVettore = Convert.ToInt32(abbonamenti.VettoreAbbonamento);
                        string desVettore = db.B2RaiPlace_Abbonamento_VettoreAbbonamento.FirstOrDefault(x=>x.Id_Vettore_Abbonamento==idVettore).VettoreAbbonamento;
                        worksheet.Cell(1, 1).Value = "RICHIESTA ABBONAMENTI - SOCIETA' "+desVettore;
                    }
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    worksheet.Range("A1:P1").Row(1).Merge();
            
                    MemoryStream M = new MemoryStream();
                    workbook.SaveAs(M);
                    M.Position = 0;
                    return new FileStreamResult(M, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = "RichiesteAbbonamenti.xlsx" };
                }
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public ActionResult RicercaAbbonamentiAjax(string datada, string dataa,string citta,string nome,string cognome, string vettore)
        {
            digiGappEntities db = new digiGappEntities();
            try
            {
                var dbabbonamenti = db.B2RaiPlace_Abbonamento_Campagna.AsQueryable();
                List<CampagnaAbbonamentiModel> listaCampagna = CreaListaCampagne(dbabbonamenti.ToList());
                var dbabbonamenti2 = db.B2RaiPlace_Abbonamento_Richieste.AsQueryable();
                if (!String.IsNullOrEmpty(datada))
                {
                    DateTime da = new DateTime(Convert.ToInt16(datada.Substring(6, 4)), Convert.ToInt16(datada.Substring(3, 2)), Convert.ToInt16(datada.Substring(0, 2)));
                    dbabbonamenti2 = dbabbonamenti2.Where(x => x.DataGiornoInizio >= da);
                }

                if (!String.IsNullOrEmpty(dataa))
                {
                    DateTime a = new DateTime(Convert.ToInt16(dataa.Substring(6, 4)), Convert.ToInt16(dataa.Substring(3, 2)), Convert.ToInt16(dataa.Substring(0, 2)));
                    dbabbonamenti2 = dbabbonamenti2.Where(x => x.DataGiornoInizio <= a);
                }

                if (!String.IsNullOrEmpty(nome))
                {
                    nome = nome.ToUpper();
                    dbabbonamenti2 = dbabbonamenti2.Where(x => x.Nome.Contains(nome));
                }
                if (!String.IsNullOrEmpty(cognome))
                {
                    cognome = cognome.ToUpper();
                    dbabbonamenti2 = dbabbonamenti2.Where(x => x.Cognome.Contains(cognome));
                }


                string descitta = "ROMA";
                if (!String.IsNullOrEmpty(citta))
                {
                    int idCitta = Convert.ToInt32(citta);
                    dbabbonamenti2 = dbabbonamenti2.Where(x => x.Fk_Id_Citta_Abbonamento == idCitta);
                    descitta = db.B2RaiPlace_Abbonamento_CittaAbbonamento.Where(a => a.Id_Citta_Abbonamento == idCitta).Select(a => a.CittaAbbonamento).FirstOrDefault().ToString();
                }
                else { 
                }

                if (!String.IsNullOrWhiteSpace(vettore))
                {
                    int idVettore = Convert.ToInt32(vettore);
                    dbabbonamenti2 = dbabbonamenti2.Where(x => x.Fk_Id_Vettore_Abbonamento == idVettore);
                }

                MyAbbonamenti listaabbonamenti = CreaListaAbbonamentiRicerca(dbabbonamenti2.OrderBy(x => x.DataGiornoInizio).ToList(), descitta);
                listaabbonamenti.VettoreAbbonamento = vettore;
                TotalAbbonamnetiModel modello = new TotalAbbonamnetiModel();
                modello.Abbonamenti = listaabbonamenti;
                modello.ListCampagnaAbbonamenti = listaCampagna;
                modello.EnabledDelete = CommonHelper.GetParametro<string>(EnumParametriSistema.AbbonamentiMatricoleCancellazione).Contains( CommonHelper.GetCurrentUserMatricola());
                Session["ListaRicercaAbbonamenti"] = modello.Abbonamenti;
                return View("~/Views/RaiPlace/RicercaAbbonamenti/subpartial/_elencoAbbonamenti.cshtml", modello);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        private string VerificaDati(CampagnaAbbonamentiModel campagna, digiGappEntities db)
        {
            string error = null;
            if (campagna.DataInizioCampagna >= campagna.DataFineCampagna)
            {
                    error += "La Data di fine campagna deve essere maggiore della data di inizio";
            }
            if (campagna.DataInizioValidita < campagna.DataFineCampagna)
            {
                error += "La Data di inizio validità deve essere maggiore della data di fine";
            }
            if (campagna.idVettore.Count == 0)
            {
                error += "Selezionare almeno un vettore";
            }
            List<B2RaiPlace_Abbonamento_Campagna> abb= db.B2RaiPlace_Abbonamento_Campagna.Where(a => ((a.DataInizioCampagna <= campagna.DataInizioCampagna) && (a.DataFineCampagna >= campagna.DataInizioCampagna)) || ((a.DataInizioCampagna <= campagna.DataFineCampagna) && (a.DataFineCampagna >= campagna.DataFineCampagna)) ).ToList();
            if (abb.Where(a=> a.Id_Campagna_Abbonamento != campagna.IdCampagna).Count() > 0)
            {
                error += "Esiste una campagna già nello stesso periodo";
            }
            if (!ModelState.IsValid)
            {
                var errori = ModelState.Values.Where(E => E.Errors.Count > 0).SelectMany(E => E.Errors).Select(E => E.ErrorMessage).ToList();

                foreach (var err in errori)
                {
                    error += err + ";";

                }
            }
            return error;
        }

        [HttpPost]
        public ActionResult GestisciCampagnaAbbonamenti(string DataInizio, string DataFine,int IdCampagna, string ArrayIdVettore, string DataInizioValidita)
        {
            CampagnaAbbonamentiModel campagna = new CampagnaAbbonamentiModel();
            
            campagna.DataFineCampagna = Convert.ToDateTime(DataFine);
            campagna.DataInizioCampagna = Convert.ToDateTime(DataInizio);
            campagna.DataInizioValidita = Convert.ToDateTime(DataInizioValidita);
            campagna.idVettore = new List<int>();
            foreach (string stringVettore in ArrayIdVettore.Split(';'))
            {
                if (stringVettore != "")
                {
                    campagna.idVettore.Add(Convert.ToInt32(stringVettore));
                }
            }
            campagna.IdCampagna = IdCampagna;
            
            string result = "";
            digiGappEntities db = new digiGappEntities();
            var ctr = this.VerificaDati(campagna, db);

            if (ctr != null)
            {
                return Content(ctr);
            }
            B2RaiPlace_Abbonamento_Campagna camp;
            B2RaiPlace_Abbonamento_CampagnaVettore campVettore;
            string operazione = "";
            if (campagna.IdCampagna> 0)
            {
                foreach (B2RaiPlace_Abbonamento_CampagnaVettore item in db.B2RaiPlace_Abbonamento_CampagnaVettore.Where(a => a.FK_IdCampagna == campagna.IdCampagna).ToList())
                {
                    db.B2RaiPlace_Abbonamento_CampagnaVettore.Remove(item);
                }
                db.SaveChanges();
                camp = db.B2RaiPlace_Abbonamento_Campagna.Find(campagna.IdCampagna);

                if (camp == null)
                {
                    return HttpNotFound();
                }
                camp.DataInizioCampagna = campagna.DataInizioCampagna;
                camp.DataFineCampagna = campagna.DataFineCampagna;
                camp.DataInizioValidita = campagna.DataInizioValidita;

                B2RaiPlace_Abbonamento_CampagnaVettore vettore = new B2RaiPlace_Abbonamento_CampagnaVettore();
                foreach (int idVettore in campagna.idVettore)
                {
                    vettore = new B2RaiPlace_Abbonamento_CampagnaVettore();
                    vettore.FK_IdVettore = idVettore;
                    camp.B2RaiPlace_Abbonamento_CampagnaVettore.Add(vettore);
                }
                
                operazione = "aggiornato";
            }
            else camp = new B2RaiPlace_Abbonamento_Campagna();
            try
            {
                B2RaiPlace_Abbonamento_CampagnaVettore vettore = new B2RaiPlace_Abbonamento_CampagnaVettore();

                if (camp.Id_Campagna_Abbonamento == 0)
                {
                    camp.DataInizioCampagna = campagna.DataInizioCampagna;
                    camp.DataFineCampagna = campagna.DataFineCampagna;
                    camp.DataInizioValidita = campagna.DataInizioValidita;
                    foreach (int idVettore in campagna.idVettore)
                    {
                        vettore = new B2RaiPlace_Abbonamento_CampagnaVettore();
                        vettore.FK_IdVettore = idVettore;
                        camp.B2RaiPlace_Abbonamento_CampagnaVettore.Add(vettore);
                    }
                    db.B2RaiPlace_Abbonamento_Campagna.Add(camp);
                    operazione = "aggiunto";
                }

                db.SaveChanges();
                
                result = "ok";
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return Content(result);
        }

        public ActionResult ShowGestisciCampagna(int idCampagna)
        {
            digiGappEntities db = new digiGappEntities();
            CampagnaAbbonamentiModel campagna = new CampagnaAbbonamentiModel();
            try
            {
                if (idCampagna == 0)
                {
                    campagna.DataInizioCampagna = DateTime.Now;
                    campagna.DataFineCampagna = DateTime.Now.AddDays(1);
                }
                else {
                    var camp = db.B2RaiPlace_Abbonamento_Campagna.Find(idCampagna);
                    campagna.DataInizioCampagna = camp.DataInizioCampagna;
                    campagna.DataFineCampagna =  new DateTime( camp.DataFineCampagna.Year,camp.DataFineCampagna.Month,camp.DataFineCampagna.Day,0,0,0);
                    //campagna.idVettore = Convert.ToInt32(camp.Fk_Id_Vettore_Abbonamento);
                    campagna.DataInizioValidita = camp.DataInizioValidita;

                    campagna.IdCampagna = idCampagna;
                }
                return PartialView("~/Views/RaiPlace/CampagnaAbbonamenti/subpartial/_gestisciCampagnaAbbonamenti.cshtml", campagna);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public static List<ListItem> GetVettoreAbbonamento(int id = 0)
        {
            digiGappEntities db = new digiGappEntities();
            List<ListItem> lista = new List<ListItem>();
            List<int> idVettori = new List<int>();
            if (id != 0)

            {
                idVettori =db.B2RaiPlace_Abbonamento_CampagnaVettore.Where(a => a.FK_IdCampagna == id).Select(a=> a.FK_IdVettore).ToList();
            }
            var elenco = db.B2RaiPlace_Abbonamento_VettoreAbbonamento.ToList();
            foreach (var documento in elenco)
            {
                {
                    ListItem item = new ListItem()
                    {
                        Value = documento.Id_Vettore_Abbonamento.ToString(),
                        Text = documento.VettoreAbbonamento,
                        Selected = idVettori.Contains(documento.Id_Vettore_Abbonamento)
                    };
                    lista.Add(item);
                }
            }
            return lista;
        }
        
        private static List<CampagnaAbbonamentiModel> CreaListaCampagne(List<B2RaiPlace_Abbonamento_Campagna> richieste)
        {
            List<CampagnaAbbonamentiModel> lista = new List<CampagnaAbbonamentiModel>();
            digiGappEntities db = new digiGappEntities();
            bool ModificabileCampagna= false;
            string vettoreCampagna= String.Empty;
            
            foreach (var item in richieste)
            {
                List<int> ListaVettori= item.B2RaiPlace_Abbonamento_CampagnaVettore.Select(x=> x.FK_IdVettore).ToList();
                ModificabileCampagna = db.B2RaiPlace_Abbonamento_Richieste.Where(a => a.Fk_Id_Citta_Abbonamento == 2 && a.DataRichiesta >= item.DataInizioCampagna && a.DataRichiesta <= item.DataFineCampagna).Count() > 0 ? false : true;
                vettoreCampagna=String.Empty;
                foreach (B2RaiPlace_Abbonamento_CampagnaVettore vettore in item.B2RaiPlace_Abbonamento_CampagnaVettore)
                {
                    vettoreCampagna += vettore.B2RaiPlace_Abbonamento_VettoreAbbonamento.VettoreAbbonamento + ";";
                }
                    
                CampagnaAbbonamentiModel riga = new CampagnaAbbonamentiModel()
                {
                    IdCampagna = item.Id_Campagna_Abbonamento,
                    DataInizioCampagna= item.DataInizioCampagna,
                    DataFineCampagna= item.DataFineCampagna,
                    Modificabile = ModificabileCampagna,
                    Vettore= vettoreCampagna
                };
                lista.Add(riga);
            }
            return lista;
        }

        [HttpGet]
        public ActionResult DeleteCampagna(int prog)
        {
            digiGappEntities db = new digiGappEntities();
            if (prog < 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var campagna = db.B2RaiPlace_Abbonamento_Campagna.Find(prog);
            if (campagna == null)
            {
                return HttpNotFound();
            }
            try
            {
                var ls = db.B2RaiPlace_Abbonamento_CampagnaVettore.Where(x => x.FK_IdCampagna == campagna.Id_Campagna_Abbonamento).ToList();
                for (int i = ls.Count()-1; i >= 0; i--)
                {
                    db.B2RaiPlace_Abbonamento_CampagnaVettore.Remove(ls[i]);
                }

                db.B2RaiPlace_Abbonamento_Campagna.Remove(campagna);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult DeleteAbbonamento(int prog)
        {
            digiGappEntities db = new digiGappEntities();
            if (prog < 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var abbonamento = db.B2RaiPlace_Abbonamento_Richieste.Find(prog);
            if (abbonamento == null)
            {
                return HttpNotFound();
            }
            try
            {
                db.B2RaiPlace_Abbonamento_Richieste.Remove(abbonamento);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return RedirectToAction("Index");
        }

        private static MyAbbonamenti CreaListaAbbonamenti(List<B2RaiPlace_Abbonamento_Richieste> richieste)
        {
            MyAbbonamenti abbonamento = new MyAbbonamenti();
            List<AbbonamentiModel> lista = new List<AbbonamentiModel>();
            digiGappEntities db = new digiGappEntities();
            foreach (var item in richieste)
            {
                AbbonamentiModel riga = new AbbonamentiModel()
                {
                    idAbbonamento = item.Id_Richiesta,
                    TipologiaDocumento = item.B2RaiPlace_Abbonamento_TipoDocumento == null ? "" : item.B2RaiPlace_Abbonamento_TipoDocumento.TipoDocumento,
                    NumeroDocumento = item.NumeroDocumento,
                    EnteRilascioDocumento = item.EnteRilascioDocumento,
                    VettoreDiAbbonamento = item.B2RaiPlace_Abbonamento_VettoreAbbonamento == null ? "" : item.B2RaiPlace_Abbonamento_VettoreAbbonamento.VettoreAbbonamento,
                    PercorsoDa = item.PercorsoDa,
                    PercorsoA = item.PercorsoA,
                    GiornoInizio = item.DataGiornoInizio,
                    GiornoFine = item.DataGiornoFine,
                    NumeroRate = item.NumeroRate,
                    ZonaAbbonamento = item.B2RaiPlace_Abbonamento_ZoneAbbonamento == null ? "" : item.B2RaiPlace_Abbonamento_ZoneAbbonamento.ZoneAbbonamento,
                    DataRilascioDocumento = item.DataRilascioDocumento,
                    Rinnovo = true,
                    Approvata = item.Flag_Approvata,
                    Nome = item.Nome,
                    Cognome = item.Cognome
                };
                lista.Add(riga);
            }
            abbonamento.Abbonamenti = lista;
            abbonamento.CreaAbbonamento = false;
            abbonamento.CittaAbbonamento = "ROMA";
            return abbonamento;
        }

        private static MyAbbonamenti CreaListaAbbonamentiRicerca(List<B2RaiPlace_Abbonamento_Richieste> richieste,string citta)
        {
            MyAbbonamenti abbonamento = new MyAbbonamenti();
            List<AbbonamentiModel> lista = new List<AbbonamentiModel>();
            digiGappEntities db = new digiGappEntities();
            foreach (var item in richieste)
            {
                AbbonamentiModel riga = new AbbonamentiModel()
                {
                    idAbbonamento = item.Id_Richiesta,
                    TipologiaDocumento = item.B2RaiPlace_Abbonamento_TipoDocumento == null ? "" : item.B2RaiPlace_Abbonamento_TipoDocumento.TipoDocumento,
                    NumeroDocumento = item.NumeroDocumento,
                    EnteRilascioDocumento = item.EnteRilascioDocumento,
                    VettoreDiAbbonamento = item.B2RaiPlace_Abbonamento_VettoreAbbonamento == null ? "" : item.B2RaiPlace_Abbonamento_VettoreAbbonamento.VettoreAbbonamento,
                    PercorsoDa = item.PercorsoDa,
                    PercorsoA = item.PercorsoA,
                    GiornoInizio = item.DataGiornoInizio,
                    GiornoFine = item.DataGiornoFine,
                    NumeroRate = item.NumeroRate,
                    ZonaAbbonamento = item.B2RaiPlace_Abbonamento_ZoneAbbonamento == null ? "" : item.B2RaiPlace_Abbonamento_ZoneAbbonamento.ZoneAbbonamento,
                    DataRilascioDocumento = item.DataRilascioDocumento,
                    Rinnovo = true,
                    Approvata = item.Flag_Approvata,
                    Nome = item.Nome,
                    Cognome = item.Cognome
                };
                lista.Add(riga);
            }
            abbonamento.Abbonamenti = lista;
            abbonamento.CreaAbbonamento = false;
            abbonamento.CittaAbbonamento = citta;
            
            return abbonamento;
        }
    }
}