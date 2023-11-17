using myRaiData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.Net.Mail;
using System.Net;
using myRaiHelper;
using myRaiCommonModel.raiplace;
using myRaiServiceHub.it.rai.servizi.sendmail;

namespace myRai.Controllers
{
    public class OmaggiController : BaseCommonController
    {
        public ActionResult Index(string quantita = "10")
        {
            digiGappEntities db = new digiGappEntities();
            try
            {
                string matricola = UtenteHelper.EsponiAnagrafica()._matricola;
                var omaggi = db.B2RaiPlace_RegistroOmaggi_Omaggio.Where(x => x.B2RaiPlace_RegistroOmaggi_Anagrafica.Matricola == matricola && x.Flag_validita == true).AsQueryable();
                List<Omaggio> listaomaggi;
                if (quantita != "tutti")
                {
                    listaomaggi = CreaListaOmaggi(omaggi.OrderByDescending(x => x.Data_Inserimento).Take(Convert.ToInt16(quantita)).ToList());
                    if (listaomaggi.Count > 0)
                    {
                        if (listaomaggi.Count.ToString() == quantita)
                        {
                            listaomaggi[0].VediTutti = true;
                        }
                    }
                }
                else
                {
                    listaomaggi = CreaListaOmaggi(omaggi.OrderByDescending(x => x.Data_Inserimento).ToList());
                }
                return View("~/Views/RaiPlace/Omaggi/Index.cshtml", listaomaggi);
            }
            catch (Exception ex)
            {

                return Content(ex.Message);
            }
        }

        public ActionResult IndexAdmin(string quantita = "10")
        {
            digiGappEntities db = new digiGappEntities();
            try
            {
                var omaggi = db.B2RaiPlace_RegistroOmaggi_Omaggio.Where(x => x.Flag_validita == true).AsQueryable();
                List<Omaggio> listaomaggi;
                if (quantita != "tutti")
                {
                    listaomaggi = CreaListaOmaggi(omaggi.OrderByDescending(x => x.Data_Inserimento).Take(Convert.ToInt16(quantita)).ToList());
                    if (listaomaggi.Count > 0)
                    {
                        if (listaomaggi.Count.ToString() == quantita)
                        {
                            listaomaggi[0].VediTutti = true;
                        }
                    }
                }
                else
                {
                    listaomaggi = CreaListaOmaggi(omaggi.OrderByDescending(x => x.Data_Inserimento).ToList());
                }
                return View("~/Views/RaiPlace/Omaggi/IndexAdmin.cshtml", listaomaggi);
            }
            catch (Exception ex)
            {

                return Content(ex.Message);
            }
        }

		public ActionResult ShowGestisciOmaggio(int idOmaggio)
        {
            digiGappEntities db = new digiGappEntities();
            Omaggio omaggio = new Omaggio();
            string matricola = UtenteHelper.EsponiAnagrafica()._matricola;

            string[] matesterne = new string[] { "RM", "TO", "NA", "00" };
            omaggio.UtenteEsterno = matesterne.Any(s => matricola.ToUpper().Substring(0, 2).Contains(s)); ;
            try
            {
                if (idOmaggio == 0)
                {

                    B2RaiPlace_RegistroOmaggi_Anagrafica utente = new B2RaiPlace_RegistroOmaggi_Anagrafica();
                    if (db.B2RaiPlace_RegistroOmaggi_Anagrafica.Where(x => x.Matricola == matricola).Count() > 0)
                    {
                        var ute = db.B2RaiPlace_RegistroOmaggi_Anagrafica.Where(x => x.Matricola == matricola).FirstOrDefault();
                        omaggio.Utente = ute;
                    }
                    else
                    {
                        utente.Nome = UtenteHelper.EsponiAnagrafica()._nome;
                        utente.Cognome = UtenteHelper.EsponiAnagrafica()._cognome;
                        utente.Indirizzo_Mail = UtenteHelper.EsponiAnagrafica()._email;
                        utente.Matricola = matricola;
                        omaggio.Utente = utente;
                    }

                    omaggio.Data_Ricezione = DateTime.Today.ToString("dd/MM/yyyy");
                }
                else
                {
                    var omag = db.B2RaiPlace_RegistroOmaggi_Omaggio.Find(idOmaggio);
                    omaggio.Data_Inserimento = omag.Data_Inserimento;
                    omaggio.Data_Ricezione = omag.Data_Ricezione.ToString("dd/MM/yyyy");
                    omaggio.Descrizione = omag.Descrizione;
                    omaggio.Ente_Beneficiario = omag.Ente_Beneficiario;
                    omaggio.Flag_UfficioSpedizioni = omag.Flag_UfficioSpedizioni;
                    omaggio.Flag_Accetto = Convert.ToBoolean(omag.Flag_accetto);
                    omaggio.Mittente = omag.Mittente;
                    omaggio.Motivo_id = omag.Id_Motivo_Omaggio;
                    omaggio.Motivo = new SelectListItem()
                    {
                        Value = omag.B2RaiPlace_RegistroOmaggi_Motivo.Id_Motivo_Omaggio.ToString(),
                        Text = omag.B2RaiPlace_RegistroOmaggi_Motivo.Descrizione,
                        Selected = true
                    };
                    omaggio.Note = omag.Note;
                    omaggio.Note_Accetto = omag.Note_accetto;
                    omaggio.Tipo_id = omag.Id_Tipo_Omaggio;
                    omaggio.Tipo = new SelectListItem()
                    {
                        Value = omag.B2RaiPlace_RegistroOmaggi_Tipo.Id_Tipo_Omaggio.ToString(),
                        Text = omag.B2RaiPlace_RegistroOmaggi_Tipo.Descrizione,
                        Selected = true
                    };
                    omaggio.UfficioSpedizioni = omag.Id_UfficioSpedizioni;
                    omaggio.Utente = omag.B2RaiPlace_RegistroOmaggi_Anagrafica;
                    omaggio.Valore = omag.Valore.ToString();
                    omaggio.Id = omag.Id;
                    omaggio.Mail_Responsabile = omag.Mail_Responsabile;
                    omaggio.Note_Altro = omag.Note_Motivo;
                }
                return PartialView("~/Views/RaiPlace/Omaggi/subpartial/_gestisciOmaggio.cshtml", omaggio);


            }
            catch (Exception ex)
            {

                return Content(ex.Message);

            }
        }

        public ActionResult RicercaOmaggiAjax(string tipo, string datada, string dataa, string matricola = "", string valda = "", string vala = "", string chiamante="")
        {
            digiGappEntities db = new digiGappEntities();
            // string matricola = myRai.Models.Utente.EsponiAnagrafica()._matricola;
            try
            {

                var omaggi = db.B2RaiPlace_RegistroOmaggi_Omaggio.Where(x => x.Flag_validita == true).AsQueryable();
                if (!string.IsNullOrEmpty(matricola))
                {
                    omaggi = omaggi.Where(x => x.B2RaiPlace_RegistroOmaggi_Anagrafica.Matricola == matricola);
                }

                if (tipo != "0" && !string.IsNullOrEmpty(tipo.Trim()))
                {
                    int tip = Convert.ToInt16(tipo);
                    omaggi = omaggi.Where(x => x.Id_Tipo_Omaggio == tip);
                }
                if (!String.IsNullOrEmpty(datada))
                {
                    DateTime da = new DateTime(Convert.ToInt16(datada.Substring(6, 4)), Convert.ToInt16(datada.Substring(3, 2)), Convert.ToInt16(datada.Substring(0, 2)));
                    omaggi = omaggi.Where(x => x.Data_Ricezione >= da);
                }

                if (!String.IsNullOrEmpty(dataa))
                {
                    DateTime a = new DateTime(Convert.ToInt16(dataa.Substring(6, 4)), Convert.ToInt16(dataa.Substring(3, 2)), Convert.ToInt16(dataa.Substring(0, 2)));
                    omaggi = omaggi.Where(x => x.Data_Ricezione <= a);
                }

                if (!String.IsNullOrEmpty(valda))
                {
                    var valoreda = Convert.ToDecimal(valda);
                    omaggi = omaggi.Where(x => x.Valore >= valoreda);
                }

                if (!String.IsNullOrEmpty(vala))
                {
                    var valorea = Convert.ToDecimal(vala);
                    omaggi = omaggi.Where(x => x.Valore <= valorea);
                }

                List<Omaggio> listaomaggi = new List<Omaggio>();
                if (omaggi.Count() > 0)
                {
                    listaomaggi = CreaListaOmaggi(omaggi.OrderByDescending(x => x.Data_Inserimento).ToList());
                }
                return PartialView("~/Views/RaiPlace/Omaggi/subpartial/_elencoOmaggi" + chiamante + ".cshtml", listaomaggi);
            }
            catch (Exception ex)
            {

                return Content(ex.Message);
            }
        }

        [HttpPost]
        public ActionResult GestisciOmaggio(Omaggio omaggio)
        {
            string result = "";
            digiGappEntities db = new digiGappEntities();
            var ctr = this.VerificaDati(omaggio, db);

            if (ctr != null)
            {

                return Content(ctr);
            }
            B2RaiPlace_RegistroOmaggi_Omaggio omag;
            string operazione = "";
            if (omaggio.Id > 0)
            {
                omag = db.B2RaiPlace_RegistroOmaggi_Omaggio.Find(omaggio.Id);

                if (omag == null)
                {
                    return HttpNotFound();
                }
                operazione = "aggiornato";
            }
            else omag = new B2RaiPlace_RegistroOmaggi_Omaggio();
            try
            {
                if (omaggio.Motivo_id == 0)
                {
                    B2RaiPlace_RegistroOmaggi_Motivo nuovomotivo = new B2RaiPlace_RegistroOmaggi_Motivo();
                    nuovomotivo.Descrizione = omaggio.Motivo.Text;
                    db.B2RaiPlace_RegistroOmaggi_Motivo.Add(nuovomotivo);
                    var prova = db.B2RaiPlace_RegistroOmaggi_Motivo.ToList();
                    omaggio.Motivo_id = nuovomotivo.Id_Motivo_Omaggio;
                    omag.Id_Motivo_Omaggio = nuovomotivo.Id_Motivo_Omaggio;
                }
                else
                {
                    omag.Id_Motivo_Omaggio = omaggio.Motivo_id;
                    omag.B2RaiPlace_RegistroOmaggi_Motivo = db.B2RaiPlace_RegistroOmaggi_Motivo.Find(omaggio.Motivo_id);
                }
                omag.Note_Motivo = omaggio.Note_Altro;
                omag.Id_Tipo_Omaggio = omaggio.Tipo_id;
                omag.Data_Inserimento = DateTime.Now;
                omag.Data_Ricezione = new DateTime(Convert.ToInt16(omaggio.Data_Ricezione.Substring(6, 4)), Convert.ToInt16(omaggio.Data_Ricezione.Substring(3, 2)), Convert.ToInt16(omaggio.Data_Ricezione.Substring(0, 2)));
                omag.Descrizione = omaggio.Descrizione;
                omag.Ente_Beneficiario = omaggio.Ente_Beneficiario;
                omag.Flag_UfficioSpedizioni = omaggio.Flag_UfficioSpedizioni;
                omag.Flag_accetto = Convert.ToBoolean(omaggio.Flag_Accetto);
                omag.Note_accetto = omaggio.Note_Accetto;
                omag.Mail_Responsabile = omaggio.Mail_Responsabile;
                omag.Mittente = omaggio.Mittente;
                omag.Note = omaggio.Note;
                omag.Valore = Convert.ToDecimal(omaggio.Valore);
                omag.Id_UfficioSpedizioni = omaggio.UfficioSpedizioni;
                omag.Flag_validita = true;
                

                if (omaggio.Utente.Id_Utente > 0)
                {
                    var utedb = db.B2RaiPlace_RegistroOmaggi_Anagrafica.Find(omaggio.Utente.Id_Utente);
                    if (omaggio.UtenteEsterno)
                    {
                        utedb.Cognome = omaggio.Utente.Cognome;
                        utedb.Nome = omaggio.Utente.Nome;
                        utedb.Indirizzo_Mail = omaggio.Utente.Indirizzo_Mail;
                    }
                    omag.B2RaiPlace_RegistroOmaggi_Anagrafica = utedb;

                }
                else
                {
                    omag.B2RaiPlace_RegistroOmaggi_Anagrafica = omaggio.Utente;
                    db.B2RaiPlace_RegistroOmaggi_Anagrafica.Add(omaggio.Utente);
                }

                if (omaggio.Id == 0)
                {
                    db.B2RaiPlace_RegistroOmaggi_Omaggio.Add(omag);
                    operazione = "aggiunto";
                }

                db.SaveChanges();
                if (omaggio.Id == 0)
                {
                    InviaMail("ute", operazione, omag);
                }
                if (!string.IsNullOrEmpty(omaggio.Mail_Responsabile))
                {
                    InviaMail("resp", operazione, omag);

                }
                result = "ok";
            }
            catch (Exception ex)
            {
                result = ex.Message;
				Logger.LogErrori( new MyRai_LogErrori()
				{
					applicativo = "Portale",
					data = DateTime.Now,
					error_message = ex.Message,
					feedback = "",
					matricola = CommonHelper.GetCurrentUserMatricola(),
					provenienza = "GestisciOmaggio"
				} );
            }

            return Content(result);
        }
        
		[HttpGet]
        public ActionResult DeleteOmaggio(int prog)
        {
            digiGappEntities db = new digiGappEntities();
            if (prog < 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var omaggio = db.B2RaiPlace_RegistroOmaggi_Omaggio.Find(prog);
            if (omaggio == null)
            {
                return HttpNotFound();
            }
            try
            {
                omaggio.Flag_validita = false;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
				Logger.LogErrori( new MyRai_LogErrori()
				{
					applicativo = "Portale",
					data = DateTime.Now,
					error_message = ex.ToString(),
					feedback = "",
					matricola = CommonHelper.GetCurrentUserMatricola(),
					provenienza = "GestisciOmaggio"
				} );
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return RedirectToAction("Index");
        }

		public static List<ListItem> GetTipiOmaggio ( int id = 0 )
		{
			digiGappEntities db = new digiGappEntities();
			List<ListItem> lista = new List<ListItem>();

			var elenco = db.B2RaiPlace_RegistroOmaggi_Tipo.ToList();
			foreach ( var tipo in elenco )
			{
				{
					ListItem item = new ListItem()
					{
						Value = tipo.Id_Tipo_Omaggio.ToString(),
						Text = tipo.Descrizione,
						Selected = ( tipo.Id_Tipo_Omaggio == id )
					};
					lista.Add( item );
				}
			}
			return lista;
		}

		public static List<ListItem> GetMotiviOmaggio ( int id = 0 )
		{
			digiGappEntities db = new digiGappEntities();
			List<ListItem> lista = new List<ListItem>();
			foreach ( var motivo in db.B2RaiPlace_RegistroOmaggi_Motivo.ToList() )
			{
				ListItem item = new ListItem()
				{
					Value = motivo.Id_Motivo_Omaggio.ToString(),
					Text = motivo.Descrizione,
					Selected = ( motivo.Id_Motivo_Omaggio == id ),

				};
				lista.Add( item );

			}
			return lista;
		}

        private string VerificaDati(Omaggio omaggio, digiGappEntities db)
        {
            string error = null;
            if (string.IsNullOrEmpty(omaggio.Data_Ricezione.Trim()))
            {
                error += "campo Data ricezione obbligatorio;";

            }
            if (!(omaggio.Tipo_id > 0))
            {
                error += "Selezionare campo Tipo omaggio;";

            }
            if (!(omaggio.Motivo_id > 0 || !(string.IsNullOrEmpty(omaggio.Motivo.Text.TrimEnd()))))
            {
                error += "Selezionare campo Occasione ricezione;";

            }
            if (string.IsNullOrEmpty(omaggio.Mittente.Trim()))
            {
                error += "campo Mittente obbligatorio;";

            }
            if (string.IsNullOrEmpty(omaggio.Valore.Trim()))
            {
                error += "campo Valore obbligatorio;";

            }
            else
            {
                decimal number;//=0.00m;
                if (!Decimal.TryParse(omaggio.Valore, out number))
                {
                    error += "campo Valore non valido;";
                }
                if (number <= 0)
                {
                    error += "immettere un Valore significativo;";
                }

            }

            if (omaggio.Flag_UfficioSpedizioni == true || omaggio.Flag_UfficioSpedizioni == false)
            {
                if (omaggio.Flag_UfficioSpedizioni == true)
                {
                    if (string.IsNullOrEmpty(omaggio.UfficioSpedizioni.Trim()))
                    {
                        error += "campo Ufficio spedizioni obbligatorio;";

                    }
                    //int value;
                    //if (!int.TryParse(omaggio.UfficioSpedizioni, out value))
                    //{
                    //    error += "campo Ufficio spedizioni non valido;";
                    //}
                    //if (value <= 0)
                    //{
                    //    error += "immettere un Ufficio spedizioni significativo;";
                    //}
                }
            }
            else
            {
                error += "selezionare se pervenuto tramite ufficio spedizioni;";
            }
            if (omaggio.Flag_Accetto == true || omaggio.Flag_Accetto == false)
            {
                //
                if (omaggio.Flag_Accetto == false && string.IsNullOrEmpty(omaggio.Note_Accetto))
                {
                    error += "Indicare il motivo del rifiuto";
                }
            }
            else
            {
                error += "Indicare se accettare o rifiutare l'omaggio;";
            }
            if (db.B2RaiPlace_RegistroOmaggi_Tipo.Find(omaggio.Tipo_id).Descrizione.ToLower() == "denaro")
            {
                omaggio.Descrizione = null;

            }
            else
            {
                if (string.IsNullOrEmpty(omaggio.Descrizione.Trim()))
                {
                    error += "campo descrizione obbligatorio;";
                }
                omaggio.Ente_Beneficiario = null;
            }
            if (string.IsNullOrEmpty(omaggio.Utente.Nome.Trim()))
            {
                error += "campo Nome obbligatorio;";

            }
            if (string.IsNullOrEmpty(omaggio.Utente.Cognome.Trim()))
            {
                error += "campo Cognome obbligatorio;";

            }
            try
            {
                var mail = new MailAddress(omaggio.Utente.Indirizzo_Mail);

            }
            catch
            {
                error += "campo Indirizzo mail non valido;";
            }
            if (!string.IsNullOrEmpty(omaggio.Mail_Responsabile))
            {
                try
                {
                    var mail = new MailAddress(omaggio.Mail_Responsabile);

                }
                catch
                {
                    error += "campo Mail responsabile non valido;";
                }
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
        
		private static List<Omaggio> CreaListaOmaggi(List<B2RaiPlace_RegistroOmaggi_Omaggio> omaggi)
        {
            List<Omaggio> listaomaggi = new List<Omaggio>();

            foreach (var item in omaggi)
            {
                Omaggio omaggio = new Omaggio()
                {
                    Id = item.Id,
                    Utente = item.B2RaiPlace_RegistroOmaggi_Anagrafica,
                    Data_Ricezione = item.Data_Ricezione.ToString("dd/MM/yyyy"),
                    Motivo_id = item.Id_Motivo_Omaggio,
                    Motivo = new SelectListItem()
                    {
                        Value = item.B2RaiPlace_RegistroOmaggi_Motivo.Id_Motivo_Omaggio.ToString(),
                        Text = item.B2RaiPlace_RegistroOmaggi_Motivo.Descrizione,
                        Selected = true
                    },
                    Mittente = item.Mittente,
                    Tipo_id = item.Id_Tipo_Omaggio,
                    Tipo = new SelectListItem()
                    {
                        Value = item.B2RaiPlace_RegistroOmaggi_Tipo.Id_Tipo_Omaggio.ToString(),
                        Text = item.B2RaiPlace_RegistroOmaggi_Tipo.Descrizione,
                        Selected = true
                    },
                    Descrizione = item.Descrizione,
                    Valore = String.Format("{0:0.00}", item.Valore),
                    Flag_UfficioSpedizioni = item.Flag_UfficioSpedizioni,
                    Flag_Accetto= Convert.ToBoolean(item.Flag_accetto),
                    UfficioSpedizioni = item.Id_UfficioSpedizioni,
                    Ente_Beneficiario = item.Ente_Beneficiario,
                    Note = item.Note,
                    Note_Accetto=item.Note_accetto,
                    Data_Inserimento=item.Data_Inserimento,
                    Mail_Responsabile=item.Mail_Responsabile,
                    Note_Altro= item.Note_Motivo
                };
                listaomaggi.Add(omaggio);
            }

            return listaomaggi;
        }
        
		private static string InviaMail(string tipomail, string operazione, B2RaiPlace_RegistroOmaggi_Omaggio omaggio)
        {
			try
			{

				if (tipomail == "ute")
				{
					MailSender invia = new MailSender();
					Email eml = new Email();
					eml.From = omaggio.B2RaiPlace_RegistroOmaggi_Anagrafica.Indirizzo_Mail;
					eml.ContentType = "text/html";
					eml.Priority = 2;
					eml.SendWhen = DateTime.Now.AddSeconds(1);

					eml.Subject = "Gestione Omaggi";
					eml.toList = new string[] { omaggio.B2RaiPlace_RegistroOmaggi_Anagrafica.Indirizzo_Mail };
					string utente = omaggio.B2RaiPlace_RegistroOmaggi_Anagrafica.Nome + " " + omaggio.B2RaiPlace_RegistroOmaggi_Anagrafica.Cognome;

					string[] AccountUtenteServizio = CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio);
					eml.Body = CommonHelper.GetParametri<string>(EnumParametriSistema.MailTemplateOmaggiUte)[0];

					string desc = "";
					if (omaggio.B2RaiPlace_RegistroOmaggi_Tipo.Descrizione.ToLower() != "denaro")
					{
						desc = "Descrizione: <b>" + omaggio.Descrizione + "</b><br>";
					}
					string ente = "";
					if (omaggio.B2RaiPlace_RegistroOmaggi_Tipo.Descrizione.ToLower() == "denaro")
					{
						ente = "Ente beneficiario: <b>" + omaggio.Ente_Beneficiario + "</b><br>";
					}
					string ufficio = "";
					if (omaggio.Flag_UfficioSpedizioni == true)
					{
						ufficio = "Omaggio pervenuto tramite ufficio spedizioni: <b>" + omaggio.Id_UfficioSpedizioni + "</b></br>";
					}
					string accettato = String.Empty;
					if (omaggio.Flag_accetto==true)
					{
						accettato = "<br>Stato: <b>Accettato</b><br>";
					}
					else {
						accettato = "<br>Stato: <b>Rifiutato</b><br>";
					}
                
					string motivo = String.Empty;
					if (!string.IsNullOrEmpty(omaggio.Note_accetto))
					{
						motivo = "Motivo: <b>" + omaggio.Note_accetto + "</b><br>";
					}

					eml.Body = eml.Body.Replace("#ute","Utente: <b>" + utente + "</b><br>").Replace("#dataric", "Data Ricezione: <b>" + omaggio.Data_Ricezione.ToString("dd/MM/yyyy") + "</b><br>").Replace("#tipo", "Tipologia:<b>" + omaggio.B2RaiPlace_RegistroOmaggi_Tipo.Descrizione + "</b><br>").Replace("#desc", desc).Replace("#val", "Valore:<b>" + omaggio.Valore.ToString() +"</b><br>").Replace("#mot", "Motivo:<b>" + omaggio.B2RaiPlace_RegistroOmaggi_Motivo.Descrizione + "</b><br>").Replace("#mit", "Mittente:<b> " + omaggio.Mittente + "</b><br>").Replace("#ente", ente).Replace("#uffsped", ufficio).Replace("#acc",accettato).Replace("#amot",motivo);

					invia.Credentials = new System.Net.NetworkCredential(AccountUtenteServizio[0], AccountUtenteServizio[1], "RAI");

					try
					{
						invia.Send(eml);
						return null;
					}
					catch (Exception ex)
					{

						return ex.ToString();
					}
				}
				if (tipomail == "resp" && omaggio.Mail_Responsabile != null)
				{
					MailSender invia = new MailSender();
					Email eml = new Email();
					eml.From = omaggio.B2RaiPlace_RegistroOmaggi_Anagrafica.Indirizzo_Mail;
					eml.ContentType = "text/html";
					eml.Priority = 2;
					eml.SendWhen = DateTime.Now.AddSeconds(1);

					eml.Subject = "Gestione Omaggi";
					eml.toList = new string[] { omaggio.Mail_Responsabile };
					string utente = omaggio.B2RaiPlace_RegistroOmaggi_Anagrafica.Nome + " " + omaggio.B2RaiPlace_RegistroOmaggi_Anagrafica.Cognome;

					string[] AccountUtenteServizio = CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio);
					eml.Body = CommonHelper.GetParametri<string>(EnumParametriSistema.MailTemplateOmaggiUte)[0];

					string desc = "";
					if (omaggio.B2RaiPlace_RegistroOmaggi_Tipo.Descrizione.ToLower() != "denaro")
					{
						desc = "Descrizione: <b>" + omaggio.Descrizione + "</b><br>";
					}
					string ente = "";
					if (omaggio.B2RaiPlace_RegistroOmaggi_Tipo.Descrizione.ToLower() == "denaro")
					{
						ente = "Ente beneficiario: <b>" + omaggio.Ente_Beneficiario + "</b><br>";
					}
					string ufficio = "";
					if (omaggio.Flag_UfficioSpedizioni == true)
					{
						ufficio = "Omaggio pervenuto tramite ufficio spedizioni: <b>" + omaggio.Id_UfficioSpedizioni + "</b></br>";
					}
					string accettato = String.Empty;
					if (omaggio.Flag_accetto == true)
					{
						accettato = "<br>Stato: <b>Accettato</b><br>";
					}
					else
					{
						accettato = "<br>Stato: <b>Rifiutato</b><br>";
					}

					string motivo = String.Empty;
					if (!string.IsNullOrEmpty(omaggio.Note_accetto))
					{
						motivo = "Motivo: <b>" + omaggio.Note_accetto + "</b><br>";
					}

					eml.Body = eml.Body.Replace("#ute", "Utente: <b>" + utente + "</b><br>").Replace("#dataric", "Data Ricezione: <b>" + omaggio.Data_Ricezione.ToString("dd/MM/yyyy") + "</b><br>").Replace("#tipo", "Tipologia:<b>" + omaggio.B2RaiPlace_RegistroOmaggi_Tipo.Descrizione + "</b><br>").Replace("#desc", desc).Replace("#val", "Valore:<b>" + omaggio.Valore.ToString() + "</b><br>").Replace("#mot", "Motivo:<b>" + omaggio.B2RaiPlace_RegistroOmaggi_Motivo.Descrizione + "</b><br>").Replace("#mit", "Mittente:<b> " + omaggio.Mittente + "</b><br>").Replace("#ente", ente).Replace("#uffsped", ufficio).Replace("#acc", accettato).Replace("#amot", motivo);


					invia.Credentials = new System.Net.NetworkCredential(AccountUtenteServizio[0], AccountUtenteServizio[1], "RAI");

					try
					{
						invia.Send(eml);
						return null;
					}
					catch (Exception ex)
					{
						Logger.LogErrori( new MyRai_LogErrori()
						{
							applicativo = "Portale",
							data = DateTime.Now,
							error_message = ex.Message,
							feedback = "",
							matricola = CommonHelper.GetCurrentUserMatricola(),
							provenienza = "Invio mail Omaggi"
						} );
						return ex.ToString();
					}
				}
			}
			catch ( Exception ex )
			{
				Logger.LogErrori( new MyRai_LogErrori()
				{
					applicativo = "Portale",
					data = DateTime.Now,
					error_message = ex.Message,
					feedback = "",
					matricola = CommonHelper.GetCurrentUserMatricola(),
					provenienza = "Invio mail Omaggi"
				} );
			}
            return null;
        }
    }
}