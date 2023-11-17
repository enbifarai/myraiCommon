using myRaiData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.Net;
using myRai.DataAccess;
using myRaiHelper;
using myRaiCommonModel.raiplace;
using myRaiServiceHub.it.rai.servizi.sendmail;
using myRai.Business;
using myRai.Models;

namespace myRai.Controllers
{
    public class AbbonamentiController : BaseCommonController
    {
        static string CurrentUserMatricola()
        {
            string m = CommonManager.GetCurrentUserMatricola();
            if (m.StartsWith("BP"))
                m = m.Substring(2);
            return m;
        }

        public ActionResult Index(string CittaAbbonamento = "")
        {
            digiGappEntities db = new digiGappEntities();
            try
            {
                //string matricola = myRai.Models.Utente.EsponiAnagrafica()._matricola;
                string matricola = CurrentUserMatricola();
                var dbabbonamenti = db.B2RaiPlace_Abbonamento_Richieste.Where(x => x.Matrciola == matricola).AsQueryable();
                MyAbbonamenti listaabbonamenti = CreaListaAbbonamenti(dbabbonamenti.OrderByDescending(x => x.DataGiornoInizio).ToList(), CittaAbbonamento);


                return View("~/Views/RaiPlace/Abbonamenti/Index.cshtml", listaabbonamenti);

            }
            catch (Exception ex)
            {

                return Content(ex.Message);
            }
        }
        private string VerificaDati(MyAbbonamenti abbonamento, digiGappEntities db)
        {
            string error = null;

            if (abbonamento.CittaAbbonamento.ToUpper()=="ROMA" && abbonamento.Abbonamenti[0].GiornoInizio>new DateTime(2020,12,01))
            {
                return "Dal 1 dicembre 2020 non è più possibile inoltrare richieste tramite la convenzione RAI - ATAC.\n" +
                        "Il dipendente può provvedere in autonomia all’attivazione/rinnovo della tessera e accedere al rimborso dalla piattaforma Welfare Experience.";
            }


            if (abbonamento.CittaAbbonamento.ToUpper() == "ROMA" && abbonamento.Abbonamenti[0].CodiceAbbonamento != null)
            {
                if (abbonamento.Abbonamenti[0].CodiceAbbonamento.Length != 8 && abbonamento.Abbonamenti[0].CodiceAbbonamento.Length != 16)
                {
                    error += "Il codice d'abbonamento da indicare è di 8 o 16 caratteri";

                }
            }
            AbbonamentiModel modello = abbonamento.Abbonamenti[0];
            if (modello.idAbbonamento == 0)
            {
                if (db.B2RaiPlace_Abbonamento_Richieste.Where(x => x.Matrciola == modello.Matricola && x.DataGiornoInizio == modello.GiornoInizio).Count() > 0)
                {
                    error += "La richiesta di abbonamento è già stata effettuata";
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
        [HttpPost]
        public ActionResult SetPolicy()
        {
            string result = "";
            digiGappEntities db = new digiGappEntities();
            //db.B2RaiPlace_AbbonamentoPolicy.
            B2RaiPlace_AbbonamentoPolicy policy = new B2RaiPlace_AbbonamentoPolicy();
            policy.Matricola = CurrentUserMatricola();// myRai.Models.Utente.EsponiAnagrafica()._matricola;
            policy.DataPolicy = DateTime.Now;
            policy.Flag_Cancellazione = false;
            db.B2RaiPlace_AbbonamentoPolicy.Add(policy);
            db.SaveChanges();
            return Content(result);
        }
        public ActionResult GetTestoPolicy(string CittaAbbonamento = "")
        {
            string result = "";
            digiGappEntities db = new digiGappEntities();
            if (CittaAbbonamento != "")
            {
                int n;
                bool valorecitta = int.TryParse(CittaAbbonamento, out n);
                if (valorecitta)
                {
                    int idCittaAbb = Convert.ToInt32(CittaAbbonamento);
                    B2RaiPlace_Abbonamento_CittaAbbonamento listcitta = db.B2RaiPlace_Abbonamento_CittaAbbonamento.Where(a => a.Id_Citta_Abbonamento == idCittaAbb).FirstOrDefault();
                    CittaAbbonamento = listcitta.CittaAbbonamento.ToUpper();
                }

            }
            else
            {
                CittaAbbonamento = myRai.Models.Utente.EsponiAnagrafica()._sedeApppartenenza;
            }

            if (CittaAbbonamento.ToUpper() == "ROMA")
            {
                result = CommonManager.GetParametri<string>(EnumParametriSistema.TestoPolicyAbbonamentiRoma)[0].ToString();
            }
            else if (CittaAbbonamento.ToUpper() == "TORINO")
            {
                result = CommonManager.GetParametri<string>(EnumParametriSistema.TestoPolicyAbbonamentiTorino)[0].ToString();
            }
            else
            {
                result = CommonManager.GetParametri<string>(EnumParametriSistema.TestoPolicyAbbonamenti)[0].ToString();
            }
            return Content(result);
        }
        public ActionResult GestisciAbbonamenti(MyAbbonamenti abbonamento)
        {
            string result = "";
            digiGappEntities db = new digiGappEntities();
            var ctr = this.VerificaDati(abbonamento, db);

            if (ctr != null)
            {

                return Content(ctr);
            }
            B2RaiPlace_Abbonamento_Richieste abbon;
            //trasofmrazione citta abbonamento in caso che vengo da utenti rai pubblicita
            if (abbonamento.CittaAbbonamento != "")
            {
                int n;
                bool valorecitta = int.TryParse(abbonamento.CittaAbbonamento, out n);
                if (valorecitta)
                {
                    int idCittaAbb = Convert.ToInt32(abbonamento.CittaAbbonamento);
                    B2RaiPlace_Abbonamento_CittaAbbonamento listcitta = db.B2RaiPlace_Abbonamento_CittaAbbonamento.Where(a => a.Id_Citta_Abbonamento == idCittaAbb).FirstOrDefault();
                    abbonamento.CittaAbbonamento = listcitta.CittaAbbonamento.ToUpper();
                }
            }


            string operazione = "";
            if (abbonamento.Abbonamenti[0].idAbbonamento > 0)
            {
                abbon = db.B2RaiPlace_Abbonamento_Richieste.Find(abbonamento.Abbonamenti[0].idAbbonamento);

                if (abbon == null)
                {
                    return HttpNotFound();
                }
                abbon.Cap = abbonamento.Abbonamenti[0].Cap;
                abbon.Cellulare = abbonamento.Abbonamenti[0].Cellulare;
                abbon.CF = abbonamento.Abbonamenti[0].CodiceFiscale;
                abbon.Classe = "Adulto";
                abbon.Cognome = abbonamento.Abbonamenti[0].Cognome;
                abbon.Comune = abbonamento.Abbonamenti[0].Comune;
                abbon.DataGiornoFine = Convert.ToDateTime(abbonamento.Abbonamenti[0].GiornoInizio.AddYears(1).AddDays(-1));
                abbon.DataGiornoInizio = abbonamento.Abbonamenti[0].GiornoInizio;
                abbon.DataNascita = abbonamento.Abbonamenti[0].DataNascita;
                abbon.ComuneNascita = abbonamento.Abbonamenti[0].ComuneNascita;
                abbon.ProvinciaNascita = abbonamento.Abbonamenti[0].ProvinciaNascita;
                abbon.Email = abbonamento.Abbonamenti[0].Email;
                abbon.Flag_Rinnovo = abbonamento.Abbonamenti[0].Rinnovo;
                abbon.Genere = abbonamento.Abbonamenti[0].Genere;
                abbon.Indirizzo = abbonamento.Abbonamenti[0].Indirizzo;
                abbon.Matrciola = abbonamento.Abbonamenti[0].Matricola;
                abbon.Nazione = abbonamento.Abbonamenti[0].Nazionalita;
                abbon.Nome = abbonamento.Abbonamenti[0].Nome;
                abbon.NumeroRate = abbonamento.Abbonamenti[0].NumeroRate;
                abbon.Provincia = abbonamento.Abbonamenti[0].Provincia;
                abbon.Telefono = abbonamento.Abbonamenti[0].Telefono;
                abbon.Fk_Id_Citta_Abbonamento = db.B2RaiPlace_Abbonamento_CittaAbbonamento.Where(a => a.CittaAbbonamento == abbonamento.CittaAbbonamento).FirstOrDefault().Id_Citta_Abbonamento;
                abbon.NumeroAbbonamento = abbonamento.Abbonamenti[0].CodiceAbbonamento;
                abbon.NumeroBipCard = abbonamento.Abbonamenti[0].NumeroBipCard;
                if (abbonamento.Abbonamenti[0].CodiceAbbonamento == null)
                {
                    abbon.Flag_Rinnovo = false;
                }
                else
                {
                    abbon.Flag_Rinnovo = true;
                }
                if (abbonamento.CittaAbbonamento.ToUpper() == "ROMA")
                {
                    abbon.Fk_Id_Zone_Abbonamento = Convert.ToInt32(abbonamento.Abbonamenti[0].IdZonaAbbonamento);
                }
                if (abbonamento.CittaAbbonamento.ToUpper() == "TORINO")
                {
                    abbon.Fk_Id_Tipo_Documento = abbonamento.Abbonamenti[0].IdTipoDocumento;
                    abbon.NumeroDocumento = abbonamento.Abbonamenti[0].NumeroDocumento;
                    abbon.EnteRilascioDocumento = abbonamento.Abbonamenti[0].EnteRilascioDocumento;
                    abbon.DataRilascioDocumento = abbonamento.Abbonamenti[0].DataRilascioDocumento;
                    abbon.Fk_Id_Vettore_Abbonamento = abbonamento.Abbonamenti[0].IdVettoreAbbonamento;
                    abbon.PercorsoDa = abbonamento.Abbonamenti[0].PercorsoDa;
                    abbon.PercorsoA = abbonamento.Abbonamenti[0].PercorsoA;

                }
                abbon.Id_Richiesta = abbonamento.Abbonamenti[0].idAbbonamento;
                abbon.DataRichiesta = DateTime.Now;

            }
            else abbon = new B2RaiPlace_Abbonamento_Richieste();
            try
            {

                if (abbon.Id_Richiesta == 0)
                {
                    abbon.Cap = abbonamento.Abbonamenti[0].Cap;
                    abbon.Cellulare = abbonamento.Abbonamenti[0].Cellulare;
                    abbon.CF = abbonamento.Abbonamenti[0].CodiceFiscale;
                    abbon.Classe = "Adulto";
                    abbon.Cognome = abbonamento.Abbonamenti[0].Cognome;
                    abbon.Comune = abbonamento.Abbonamenti[0].Comune;
                    abbon.ComuneNascita = abbonamento.Abbonamenti[0].ComuneNascita;
                    abbon.ProvinciaNascita = abbonamento.Abbonamenti[0].ProvinciaNascita;
                    abbon.DataGiornoFine = Convert.ToDateTime(abbonamento.Abbonamenti[0].GiornoInizio.AddYears(1).AddDays(-1));
                    abbon.DataGiornoInizio = abbonamento.Abbonamenti[0].GiornoInizio;
                    abbon.DataNascita = abbonamento.Abbonamenti[0].DataNascita;
                    abbon.Email = abbonamento.Abbonamenti[0].Email;
                    abbon.Flag_Rinnovo = abbonamento.Abbonamenti[0].Rinnovo;
                    abbon.Genere = abbonamento.Abbonamenti[0].Genere;
                    abbon.Indirizzo = abbonamento.Abbonamenti[0].Indirizzo;
                    abbon.Matrciola = abbonamento.Abbonamenti[0].Matricola;
                    abbon.Nazione = abbonamento.Abbonamenti[0].Nazionalita;
                    abbon.Nome = abbonamento.Abbonamenti[0].Nome;
                    abbon.NumeroRate = abbonamento.Abbonamenti[0].NumeroRate;
                    abbon.Provincia = abbonamento.Abbonamenti[0].Provincia;
                    abbon.Telefono = abbonamento.Abbonamenti[0].Telefono;
                    abbon.Fk_Id_Citta_Abbonamento = db.B2RaiPlace_Abbonamento_CittaAbbonamento.Where(a => a.CittaAbbonamento == abbonamento.CittaAbbonamento).FirstOrDefault().Id_Citta_Abbonamento;

                    if (abbonamento.Abbonamenti[0].CodiceAbbonamento == null)
                    {
                        abbon.Flag_Rinnovo = false;
                    }
                    else
                    {
                        abbon.Flag_Rinnovo = true;
                    }
                    abbon.NumeroAbbonamento = abbonamento.Abbonamenti[0].CodiceAbbonamento;
                    abbon.NumeroBipCard = abbonamento.Abbonamenti[0].NumeroBipCard;
                    if (abbonamento.CittaAbbonamento.ToUpper() == "ROMA")
                    {
                        abbon.Fk_Id_Zone_Abbonamento = Convert.ToInt32(abbonamento.Abbonamenti[0].IdZonaAbbonamento);
                    }
                    if (abbonamento.CittaAbbonamento.ToUpper() == "TORINO")
                    {
                        abbon.Fk_Id_Tipo_Documento = abbonamento.Abbonamenti[0].IdTipoDocumento;
                        abbon.NumeroDocumento = abbonamento.Abbonamenti[0].NumeroDocumento;
                        abbon.EnteRilascioDocumento = abbonamento.Abbonamenti[0].EnteRilascioDocumento;
                        abbon.DataRilascioDocumento = abbonamento.Abbonamenti[0].DataRilascioDocumento;
                        abbon.Fk_Id_Vettore_Abbonamento = abbonamento.Abbonamenti[0].IdVettoreAbbonamento;
                        abbon.PercorsoDa = abbonamento.Abbonamenti[0].PercorsoDa;
                        abbon.PercorsoA = abbonamento.Abbonamenti[0].PercorsoA;

                    }
                    //ABBONAMENTO ANNUALE
                    abbon.Fk_Id_Tipo_Abbonamento = 1;
                    abbon.DataRichiesta = DateTime.Now;

                    db.B2RaiPlace_Abbonamento_Richieste.Add(abbon);

                }

                if (abbonamento.CittaAbbonamento.ToUpper() == "ROMA")
                {
                    //ROMA
                    if (string.IsNullOrEmpty(abbon.NumeroAbbonamento))
                    {
                        operazione = "Nuovo";
                    }
                    else
                    {
                        operazione = "Rinnovo";
                    }
                }
                else
                {
                    //TORINO
                    if (string.IsNullOrEmpty(abbon.NumeroBipCard))
                    {
                        operazione = "Nuovo";
                    }
                    else
                    {
                        operazione = "Rinnovo";
                    }
                }

                db.SaveChanges();
                if ( DBHelper.Save( db , CommonHelper.GetCurrentUserMatricola( ) ) )
                {
                    if ( abbonamento.InviaMail )
                        SendMail( operazione , abbonamento );


                    if ( operazione == "Rinnovo" )
                    {
                        result = "okmodifica";
                    }
                    else
                    {
                        result = "ok";
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return Content(result);
        }
        private void SendMail(string operazione, MyAbbonamenti abbonamento)
        {
            digiGappEntities db = new digiGappEntities();
            MailSender invia = new MailSender();
            Email eml = new Email();
            eml.From = CommonManager.GetParametri<string>(EnumParametriSistema.MailAbbonamentiFrom)[0].ToString();

            eml.ContentType = "text/html";
            eml.Priority = 2;
            eml.SendWhen = DateTime.Now.AddSeconds(1);

            eml.Subject = operazione + " Abbonamento";
            eml.toList = new string[] { abbonamento.Abbonamenti[0].Email };
            //eml.toList = new string[] { "andrea.martis@3wlab.it" };
            string matricola = CommonManager.GetCurrentUserMatricola();
            if (abbonamento.CittaAbbonamento.ToUpper() == "ROMA")
            {
                eml.Body = CommonManager.GetParametri<string>(EnumParametriSistema.MailTemplateAbbonamentiRoma)[0].ToString();
                int IdZonaAbbonamento = Convert.ToInt32(abbonamento.Abbonamenti[0].IdZonaAbbonamento);
                eml.Body = eml.Body.Replace("#ute", myRai.Models.Utente.EsponiAnagrafica()._nome + " " + myRai.Models.Utente.EsponiAnagrafica()._cognome + "(" + matricola + ")").Replace("#ope", operazione).Replace("#tipologia", db.B2RaiPlace_Abbonamento_ZoneAbbonamento.Where(a => a.Id_Zone_Abbonamento == IdZonaAbbonamento).FirstOrDefault().ZoneAbbonamento).Replace("#giornoinizio", abbonamento.Abbonamenti[0].GiornoInizio.ToString("dd/MM/yyyy")).Replace("#numerorate", abbonamento.Abbonamenti[0].NumeroRate.ToString()).Replace("#data_nascita", abbonamento.Abbonamenti[0].DataNascita.ToString("dd/MM/yyyy")).Replace("#numerocard", abbonamento.Abbonamenti[0].CodiceAbbonamento).Replace("#datainserimento", DateTime.Now.ToString("dd/MM/yyyy")).Replace("#identificativo", abbonamento.Abbonamenti[0].idAbbonamento.ToString()).Replace("#email", abbonamento.Abbonamenti[0].Email);
                eml.ccList = new string[] { CommonManager.GetParametri<string>(EnumParametriSistema.MailCCRoma)[0].ToString() };
            }
            else
            {
                eml.Body = CommonManager.GetParametri<string>(EnumParametriSistema.MailTemplateAbbonamentiTorino)[0].ToString();
                int IdVettore = Convert.ToInt32(abbonamento.Abbonamenti[0].IdVettoreAbbonamento);
                eml.Body = eml.Body.Replace("#ute", myRai.Models.Utente.EsponiAnagrafica()._nome + " " + myRai.Models.Utente.EsponiAnagrafica()._cognome + "(" + matricola + ")").Replace("#ope", operazione).Replace("#vettore", db.B2RaiPlace_Abbonamento_VettoreAbbonamento.Where(a => a.Id_Vettore_Abbonamento == IdVettore).FirstOrDefault().VettoreAbbonamento).Replace("#giornoinizio", abbonamento.Abbonamenti[0].GiornoInizio.ToString("dd/MM/yyyy")).Replace("#numerorate", abbonamento.Abbonamenti[0].NumeroRate.ToString());
                eml.ccList = new string[] { CommonManager.GetParametri<string>(EnumParametriSistema.MailCCTorino)[0].ToString() };

            }
            string[] AccountUtenteServizio = CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio);
            invia.Credentials = new System.Net.NetworkCredential(AccountUtenteServizio[0], AccountUtenteServizio[1], "RAI");

            try
            {
                invia.Send(eml);

            }
            catch (Exception ex)
            {

            }

        }
        private void SendMailCanc(string operazione, B2RaiPlace_Abbonamento_Richieste abbonamento, string cittaAbbonamento, string zonaAbb, string vettAbb)
        {
            digiGappEntities db = new digiGappEntities();
            MailSender invia = new MailSender();
            Email eml = new Email();
            eml.From = CommonManager.GetParametri<string>(EnumParametriSistema.MailAbbonamentiFrom)[0].ToString();

            eml.ContentType = "text/html";
            eml.Priority = 2;
            eml.SendWhen = DateTime.Now.AddSeconds(1);

            eml.Subject = operazione + " Abbonamento";
            eml.toList = new string[] { abbonamento.Email };
            //eml.toList = new string[] { "andrea.martis@3wlab.it" };
            string matricola = CommonManager.GetCurrentUserMatricola();
            if (cittaAbbonamento == "ROMA")
            {
                eml.Body = CommonManager.GetParametri<string>(EnumParametriSistema.MailTemplateAbbonamentiRomaCanc)[0].ToString();
                eml.Body = eml.Body.Replace("#ute", myRai.Models.Utente.EsponiAnagrafica()._nome + " " + myRai.Models.Utente.EsponiAnagrafica()._cognome + "(" + matricola + ")")
                    .Replace("#ope", operazione)
                    .Replace("#tipologia", zonaAbb)
                    .Replace("#giornoinizio", abbonamento.DataGiornoInizio.ToString("dd/MM/yyyy"))
                    .Replace("#numerorate", abbonamento.NumeroRate.ToString())
                    .Replace("#data_nascita", abbonamento.DataNascita.ToString("dd/MM/yyyy"))
                    .Replace("#numerocard", abbonamento.NumeroAbbonamento)
                    .Replace("#datainserimento", DateTime.Now.ToString("dd/MM/yyyy"))
                    .Replace("#email", abbonamento.Email);
                eml.ccList = new string[] { CommonManager.GetParametri<string>(EnumParametriSistema.MailCCRoma)[0].ToString() };
            }
            else
            {
                eml.Body = CommonManager.GetParametri<string>(EnumParametriSistema.MailTemplateAbbonamentiTorinoCanc)[0].ToString();
                eml.Body = eml.Body.Replace("#ute", myRai.Models.Utente.EsponiAnagrafica()._nome + " " + myRai.Models.Utente.EsponiAnagrafica()._cognome + "(" + matricola + ")")
                    .Replace("#ope", operazione)
                    .Replace("#vettore", vettAbb)
                    .Replace("#giornoinizio", abbonamento.DataGiornoInizio.ToString("dd/MM/yyyy"))
                    .Replace("#numerorate", abbonamento.NumeroRate.ToString());
                eml.ccList = new string[] { CommonManager.GetParametri<string>(EnumParametriSistema.MailCCTorino)[0].ToString() };
            }
            string[] AccountUtenteServizio = CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio);
            invia.Credentials = new System.Net.NetworkCredential(AccountUtenteServizio[0], AccountUtenteServizio[1], "RAI");

            try
            {
                invia.Send(eml);

            }
            catch (Exception ex)
            {

        }

        }
        public ActionResult ShowGestisciAbbonamentoExtra(string matricola, string CittaAbbonamento, string InviaMail)
        {
            digiGappEntities db = new digiGappEntities();
            MyAbbonamenti abbonamento = new MyAbbonamenti();
            abbonamento.InviaMail = Convert.ToBoolean(InviaMail);
            abbonamento.RateUnicaSoluzione = false;



            try
            {
                int ContaAbbonamenti = db.B2RaiPlace_Abbonamento_Richieste.Where(x => x.Matrciola == matricola).Count();
                var q = db.B2RaiPlace_Abbonamento_Richieste.Where(x => x.Matrciola == matricola).OrderByDescending(t => t.DataGiornoFine).FirstOrDefault();
                DateTime dtGiornoInizio;
                if (DateTime.Now.Day > 2)
                {
                    dtGiornoInizio = Convert.ToDateTime("01/" + DateTime.Now.AddMonths(2).Month + "/" + DateTime.Now.AddMonths(2).Year);
                }
                else
                {
                    dtGiornoInizio = Convert.ToDateTime("01/" + DateTime.Now.AddMonths(1).Month + "/" + DateTime.Now.AddMonths(1).Year);

                }

                abbonamento.CittaAbbonamento = CittaAbbonamento;
                AbbonamentiModel modelabbonamento = new AbbonamentiModel();
                Utente.Anagrafica anagrafica = new Utente.Anagrafica();
                it.rai.servizi.hrgb.Service wsAnag = new it.rai.servizi.hrgb.Service();
                try
                {
                    wsAnag.Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
                    // string str_temp = wsAnag.EsponiAnagrafica("RAICV;" + matricola + ";;E;0");
                    string str_temp = ServiceWrapper.EsponiAnagraficaRaicvWrapped(matricola);

                    string[] temp = str_temp.ToString().Split(';');
                    //temp = null; //freak - forzatura
                    if ((temp != null) && (temp.Count() > 16))
                    {
                        anagrafica = Utente.CaricaAnagrafica(temp);
                    }
                    else
                    {
                        if ((temp != null) && (temp.Count() == 1))
                        {
                            return PartialView("~/Views/RaiPlace/Abbonamenti/subpartial/_gestisciAbbonamenti.cshtml", temp[0]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }

                modelabbonamento.CittaAbbonamento = CittaAbbonamento;
                modelabbonamento.Matricola = matricola;
                modelabbonamento.Nome = anagrafica._nome;
                modelabbonamento.Cognome = anagrafica._cognome;
                modelabbonamento.DataNascita = anagrafica._dataNascita == null ? DateTime.MinValue : Convert.ToDateTime(anagrafica._dataNascita);
                modelabbonamento.ComuneNascita = anagrafica._comuneNascita;
                modelabbonamento.ProvinciaNascita = anagrafica._provinciaNascita;
                modelabbonamento.Genere = anagrafica._genere;
                modelabbonamento.CodiceFiscale = anagrafica._cf;
                modelabbonamento.Indirizzo = anagrafica._indirizzoresidenza;
                modelabbonamento.Cap = anagrafica._capresidenza;
                modelabbonamento.Comune = anagrafica._comuneresidenza;
                modelabbonamento.Provincia = anagrafica._provinciaresidenza;
                modelabbonamento.Nazionalita = anagrafica._nazionalita;
                modelabbonamento.GiornoInizio = dtGiornoInizio;
                modelabbonamento.Nazionalita = anagrafica._nazionalita;
                modelabbonamento.Email = anagrafica._email;
                modelabbonamento.Rinnovo = ContaAbbonamenti > 0 ? true : false;
                if (anagrafica._contratto.ToUpper() == "TEMPO DETERMINATO")
                {
                    abbonamento.RateUnicaSoluzione = true;
                    modelabbonamento.NumeroRate = 0;
                }
                modelabbonamento.Policy = true;
                abbonamento.Abbonamenti = new List<AbbonamentiModel>();
                abbonamento.Abbonamenti.Add(modelabbonamento);



                return PartialView("~/Views/RaiPlace/Abbonamenti/subpartial/_gestisciAbbonamenti.cshtml", abbonamento);


            }
            catch (Exception ex)
            {

                return Content(ex.Message);

            }
        }
        public ActionResult InserisciAbbonamentoPubb(string CittaAbbonamento)
        {
            return RedirectToAction("Index", new { CittaAbbonamento = CittaAbbonamento });
        }



        public ActionResult ShowGestisciAbbonamento(int idAbbonamento, string cittaAbbonamento = "")
        {
            digiGappEntities db = new digiGappEntities();
            MyAbbonamenti abbonamento = new MyAbbonamenti();

            abbonamento.InviaMail = true;
            abbonamento.RateUnicaSoluzione = false;
            if (myRai.Models.Utente.EsponiAnagrafica()._contratto.ToUpper() == "TEMPO DETERMINATO")
                abbonamento.RateUnicaSoluzione = true;



            try
            {
                if (idAbbonamento == 0)
                {
                    //string matricola = myRai.Models.Utente.EsponiAnagrafica()._matricola;
                    string matricola = CurrentUserMatricola();
                    int ContaAbbonamenti = db.B2RaiPlace_Abbonamento_Richieste.Where(x => x.Matrciola == matricola).Count();
                    var q = db.B2RaiPlace_Abbonamento_Richieste.Where(x => x.Matrciola == matricola).OrderByDescending(t => t.DataGiornoFine).FirstOrDefault();
                    DateTime dtGiornoInizio;
                    if (cittaAbbonamento != "")
                    {
                        int n;
                        bool valorecitta = int.TryParse(cittaAbbonamento, out n);
                        if (valorecitta)
                        {
                            int idCittaAbb = Convert.ToInt32(cittaAbbonamento);
                            B2RaiPlace_Abbonamento_CittaAbbonamento listcitta = db.B2RaiPlace_Abbonamento_CittaAbbonamento.Where(a => a.Id_Citta_Abbonamento == idCittaAbb).FirstOrDefault();
                            abbonamento.CittaAbbonamento = listcitta.CittaAbbonamento.ToUpper();
                        }
                        else
                        {
                            abbonamento.CittaAbbonamento = cittaAbbonamento;
                        }
                    }
                    else
                    {
                        abbonamento.CittaAbbonamento = myRai.Models.Utente.EsponiAnagrafica()._sedeApppartenenza;
                    }

                    if (abbonamento.CittaAbbonamento.ToUpper() == "ROMA")
                    {
                        return Content("<span>" +
                                            "Dal 1 dicembre 2020 non è più possibile inoltrare richieste tramite la convenzione RAI - ATAC.<br/>" +
                                            "Il dipendente può provvedere in autonomia all’attivazione / rinnovo della tessera e accedere al rimborso dalla piattaforma Welfare Experience al link <br/>" +
                                            "<a href = \"http://www.raiplace.rai.it/pagine/welfare-e-benefit/\" >http://www.raiplace.rai.it/pagine/welfare-e-benefit/</a>" +
                                        "</span>");
                    }

                    if (abbonamento.CittaAbbonamento.ToUpper() == "TORINO")
                    {
                        var campagna = db.B2RaiPlace_Abbonamento_Campagna.FirstOrDefault(a => a.DataInizioCampagna <= DateTime.Today && a.DataFineCampagna >= DateTime.Today);

                        dtGiornoInizio = campagna.DataInizioValidita.HasValue ? campagna.DataInizioValidita.Value : new DateTime( DateTime.Now.AddMonths( 1 ).Year , DateTime.Now.AddMonths( 1 ).Month , 1 );
                    }
                    else
                    {
                        if (DateTime.Now.Day > 2)
                        {
                            dtGiornoInizio = Convert.ToDateTime("01/" + DateTime.Now.AddMonths(2).Month + "/" + DateTime.Now.AddMonths(2).Year);
                        }
                        else
                        {
                            dtGiornoInizio = Convert.ToDateTime("01/" + DateTime.Now.AddMonths(1).Month + "/" + DateTime.Now.AddMonths(1).Year);

                        }
                    }

                    //if (cittaAbbonamento.ToUpper() == "TORINO")
                    //{
                    //    dtGiornoInizio = new DateTime(2018, 10, 01);
                    //}

                    AbbonamentiModel modelabbonamento = new AbbonamentiModel();

                    modelabbonamento.Matricola = matricola;
                    modelabbonamento.Nome = myRai.Models.Utente.EsponiAnagrafica()._nome;
                    modelabbonamento.Cognome = myRai.Models.Utente.EsponiAnagrafica()._cognome;
                    modelabbonamento.DataNascita = myRai.Models.Utente.EsponiAnagrafica()._dataNascita == null ? DateTime.MinValue : Convert.ToDateTime(myRai.Models.Utente.EsponiAnagrafica()._dataNascita);
                    modelabbonamento.ComuneNascita = myRai.Models.Utente.EsponiAnagrafica()._comuneNascita;
                    modelabbonamento.ProvinciaNascita = myRai.Models.Utente.EsponiAnagrafica()._provinciaNascita;
                    modelabbonamento.Genere = myRai.Models.Utente.EsponiAnagrafica()._genere;
                    modelabbonamento.CodiceFiscale = myRai.Models.Utente.EsponiAnagrafica()._cf;
                    modelabbonamento.Indirizzo = myRai.Models.Utente.EsponiAnagrafica()._indirizzoresidenza;
                    modelabbonamento.Cap = myRai.Models.Utente.EsponiAnagrafica()._capresidenza;
                    modelabbonamento.Comune = myRai.Models.Utente.EsponiAnagrafica()._comuneresidenza;
                    modelabbonamento.Provincia = myRai.Models.Utente.EsponiAnagrafica()._provinciaresidenza;
                    modelabbonamento.Nazionalita = myRai.Models.Utente.EsponiAnagrafica()._nazionalita;
                    modelabbonamento.GiornoInizio = dtGiornoInizio;
                    modelabbonamento.Nazionalita = myRai.Models.Utente.EsponiAnagrafica()._nazionalita;
                    modelabbonamento.Email = myRai.Models.Utente.EsponiAnagrafica()._email;
                    modelabbonamento.Rinnovo = ContaAbbonamenti > 0 ? true : false;
                    if (abbonamento.RateUnicaSoluzione)
                        modelabbonamento.NumeroRate = 0;

                    modelabbonamento.Policy = db.B2RaiPlace_AbbonamentoPolicy.Where(a => a.Matricola == matricola).Count() > 0 ? true : false;
                    abbonamento.Abbonamenti = new List<AbbonamentiModel>();
                    abbonamento.Abbonamenti.Add(modelabbonamento);


                }
                else
                {

                    var abbon = db.B2RaiPlace_Abbonamento_Richieste.Find(idAbbonamento);
                    string matricola = abbon.Matrciola;
                    int ContaAbbonamenti = db.B2RaiPlace_Abbonamento_Richieste.Where(x => x.Matrciola == matricola).Count();
                    abbonamento.CittaAbbonamento = abbon.B2RaiPlace_Abbonamento_CittaAbbonamento.CittaAbbonamento;
                    AbbonamentiModel modelabbonamento = new AbbonamentiModel();

                    modelabbonamento.Matricola = matricola;
                    modelabbonamento.Nome = abbon.Nome;
                    modelabbonamento.Cognome = abbon.Cognome;
                    modelabbonamento.DataNascita = abbon.DataNascita;
                    modelabbonamento.ComuneNascita = abbon.ComuneNascita;
                    modelabbonamento.ProvinciaNascita = abbon.ProvinciaNascita;
                    modelabbonamento.Genere = abbon.Genere;
                    modelabbonamento.CodiceFiscale = abbon.CF;
                    modelabbonamento.Indirizzo = abbon.Indirizzo;
                    modelabbonamento.Cap = abbon.Cap;
                    modelabbonamento.Comune = abbon.Comune;
                    modelabbonamento.Provincia = abbon.Provincia;
                    modelabbonamento.Nazionalita = abbon.Nazione;
                    modelabbonamento.GiornoInizio = abbon.DataGiornoInizio;
                    modelabbonamento.Email = abbon.Email;
                    modelabbonamento.Rinnovo = ContaAbbonamenti > 0 ? true : false;
                    modelabbonamento.Cellulare = abbon.Cellulare;
                    modelabbonamento.Telefono = abbon.Telefono;
                    if (abbon.Fk_Id_Zone_Abbonamento != null)
                        modelabbonamento.IdZonaAbbonamento = Convert.ToString(abbon.Fk_Id_Zone_Abbonamento);
                    if (abbon.Fk_Id_Vettore_Abbonamento != null)
                        modelabbonamento.IdVettoreAbbonamento = Convert.ToInt32(abbon.Fk_Id_Vettore_Abbonamento);
                    if (abbon.Fk_Id_Tipo_Documento != null)
                        modelabbonamento.IdTipoDocumento = Convert.ToInt32(abbon.Fk_Id_Tipo_Documento);
                    modelabbonamento.NumeroDocumento = abbon.NumeroDocumento;
                    modelabbonamento.EnteRilascioDocumento = abbon.EnteRilascioDocumento;
                    modelabbonamento.DataRilascioDocumento = abbon.DataRilascioDocumento;
                    modelabbonamento.PercorsoA = abbon.PercorsoA;
                    modelabbonamento.PercorsoDa = abbon.PercorsoDa;
                    modelabbonamento.NumeroRate = abbon.NumeroRate;
                    modelabbonamento.CodiceAbbonamento = abbon.NumeroAbbonamento;
                    modelabbonamento.NumeroBipCard = abbon.NumeroBipCard;
                    modelabbonamento.idAbbonamento = idAbbonamento;
                    if (abbonamento.RateUnicaSoluzione)
                        modelabbonamento.NumeroRate = 0;
                    modelabbonamento.Policy = db.B2RaiPlace_AbbonamentoPolicy.Where(a => a.Matricola == matricola).Count() > 0 ? true : false;
                    abbonamento.Abbonamenti = new List<AbbonamentiModel>();
                    abbonamento.Abbonamenti.Add(modelabbonamento);

                }


                return PartialView("~/Views/RaiPlace/Abbonamenti/subpartial/_gestisciAbbonamenti.cshtml", abbonamento);


            }
            catch (Exception ex)
            {

                return Content(ex.Message);

            }
        }

        public static List<ListItem> GetZoneAbbonamento()
        {
            digiGappEntities db = new digiGappEntities();
            List<ListItem> lista = new List<ListItem>();

            var elenco = db.B2RaiPlace_Abbonamento_ZoneAbbonamento.ToList();
            foreach (var zone in elenco)
            {
                {
                    ListItem item = new ListItem()
                    {
                        Value = zone.Id_Zone_Abbonamento.ToString(),
                        Text = zone.ZoneAbbonamento
                    };
                    lista.Add(item);
                }



            }

            return lista;
        }
        public static List<ListItem> getCittaAbbonamento(string cittaEscluse = "")
        {
            digiGappEntities db = new digiGappEntities();
            List<ListItem> lista = new List<ListItem>();

            var elenco = db.B2RaiPlace_Abbonamento_CittaAbbonamento.Where(x => cittaEscluse == "" || !cittaEscluse.Contains(x.CittaAbbonamento.ToUpper())).ToList();
            foreach (var citta in elenco)
            {
                    ListItem item = new ListItem()
                    {
                        Value = citta.Id_Citta_Abbonamento.ToString(),
                        Text = citta.CittaAbbonamento
                    };
                    lista.Add(item);
                }

            return lista;
        }
        public static List<ListItem> getVettoreAbbonamento()
        {
            digiGappEntities db = new digiGappEntities();
            List<ListItem> lista = new List<ListItem>();

            var elenco = db.B2RaiPlace_Abbonamento_VettoreAbbonamento.ToList();
            foreach (var vettore in elenco)
            {
                {
                    ListItem item = new ListItem()
                    {
                        Value = vettore.Id_Vettore_Abbonamento.ToString(),
                        Text = vettore.VettoreAbbonamento
                    };
                    lista.Add(item);
                }
            }

            return lista;
        }
        public static List<ListItem> GetTipoDocumento()
        {
            digiGappEntities db = new digiGappEntities();
            List<ListItem> lista = new List<ListItem>();

            var elenco = db.B2RaiPlace_Abbonamento_TipoDocumento.ToList();
            foreach (var documento in elenco)
            {
                {
                    ListItem item = new ListItem()
                    {
                        Value = documento.Id_Tipo_Documento.ToString(),
                        Text = documento.TipoDocumento
                    };
                    lista.Add(item);
                }



            }

            return lista;
        }
        public static List<ListItem> GetVettoreAbbonamento()
        {
            digiGappEntities db = new digiGappEntities();
            List<ListItem> lista = new List<ListItem>();
            DateTime dtOggi = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            var elenco = db.B2RaiPlace_Abbonamento_CampagnaVettore.Where(a => a.B2RaiPlace_Abbonamento_Campagna.DataInizioCampagna <= dtOggi && a.B2RaiPlace_Abbonamento_Campagna.DataFineCampagna >= dtOggi).Select(a => new { a.B2RaiPlace_Abbonamento_VettoreAbbonamento.Id_Vettore_Abbonamento, a.B2RaiPlace_Abbonamento_VettoreAbbonamento.VettoreAbbonamento }).ToList();

            foreach (var documento in elenco.Distinct())
            {
                {
                    ListItem item = new ListItem()
                    {
                        Value = documento.Id_Vettore_Abbonamento.ToString(),
                        Text = documento.VettoreAbbonamento
                    };
                    lista.Add(item);
                }


            }

            return lista;
        }
        public static List<ListItem> GeNumeroRate()
        {

            List<ListItem> lista = new List<ListItem>();


            for (int i = 1; i <= 6; i++)

            {
                {
                    ListItem item = new ListItem()
                    {
                        Value = i.ToString(),
                        Text = i.ToString()
                    };
                    lista.Add(item);
                }

            }

            return lista;
        }
        [HttpGet]
        public ActionResult DeleteAbbonamento(int prog)
        {
            digiGappEntities db = new digiGappEntities();
            if (prog < 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var abbonamento = db.B2RaiPlace_Abbonamento_Richieste
                .Include("B2RaiPlace_Abbonamento_CittaAbbonamento")
                .Include("B2RaiPlace_Abbonamento_VettoreAbbonamento")
                .Include("B2RaiPlace_Abbonamento_ZoneAbbonamento")
                .FirstOrDefault(x => x.Id_Richiesta == prog);
            if (abbonamento == null)
            {
                return HttpNotFound();
            }
            try
            {
                string operazione = "";
                string citta = abbonamento.B2RaiPlace_Abbonamento_CittaAbbonamento.CittaAbbonamento.ToUpper();
                string zonaAbb = "";
                string vettAbb = "";
                if (citta == "ROMA")
                {
                    zonaAbb = abbonamento.B2RaiPlace_Abbonamento_ZoneAbbonamento.ZoneAbbonamento;
                    //ROMA
                    if (string.IsNullOrEmpty(abbonamento.NumeroAbbonamento))
                    {
                        operazione = "Nuovo";
                    }
                    else
                    {
                        operazione = "Rinnovo";
                    }
                }
                else
                {
                    vettAbb = abbonamento.B2RaiPlace_Abbonamento_CittaAbbonamento.CittaAbbonamento;
                    //TORINO
                    if (string.IsNullOrEmpty(abbonamento.NumeroBipCard))
                    {
                        operazione = "Nuovo";
                    }
                    else
                    {
                        operazione = "Rinnovo";
                    }
                }


                db.B2RaiPlace_Abbonamento_Richieste.Remove(abbonamento);
                DBHelper.Save(db, CommonManager.GetCurrentUserMatricola());
                SendMailCanc(operazione, abbonamento, citta, zonaAbb, vettAbb);
                //db.SaveChanges();
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return RedirectToAction("Index");
        }
        private static MyAbbonamenti CreaListaAbbonamenti(List<B2RaiPlace_Abbonamento_Richieste> richieste, string CittaAbbonamento)
        {
            MyAbbonamenti abbonamento = new MyAbbonamenti();
            List<AbbonamentiModel> lista = new List<AbbonamentiModel>();
            digiGappEntities db = new digiGappEntities();
            string cittaperAbbonarsi = "";
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
                    Approvata = item.Flag_Approvata
                };
                lista.Add(riga);
            }
            abbonamento.Abbonamenti = lista;
            abbonamento.CreaAbbonamento = false;

            //string matricola = myRai.Models.Utente.EsponiAnagrafica()._matricola;
            string matricola = CurrentUserMatricola();
            if (CittaAbbonamento != "")
            {
                int n;
                bool valorecitta = int.TryParse(CittaAbbonamento, out n);
                if (valorecitta)
                {
                    int idCittaAbb = Convert.ToInt32(CittaAbbonamento);
                    B2RaiPlace_Abbonamento_CittaAbbonamento listcitta = db.B2RaiPlace_Abbonamento_CittaAbbonamento.Where(a => a.Id_Citta_Abbonamento == idCittaAbb).FirstOrDefault();
                    cittaperAbbonarsi = listcitta.CittaAbbonamento.ToUpper();
                }
                else
                {
                    abbonamento.CittaAbbonamento = CittaAbbonamento;
                }

            }
            else
            {
                cittaperAbbonarsi = myRai.Models.Utente.EsponiAnagrafica()._sedeApppartenenza;
            }
            abbonamento.CittaAbbonamento = cittaperAbbonarsi;
            if (cittaperAbbonarsi.ToUpper() == "ROMA")
            {
                //dall’11/12/2020 è attiva la piattaforma Welfare pertanto non sarà più possibile inserire richieste dal portale Rai Per Me
                abbonamento.CreaAbbonamento = false;

                //if (lista.Count == 0)
                //{
                //    abbonamento.CreaAbbonamento = true;
                //}
                //else
                //{
                //    var q = db.B2RaiPlace_Abbonamento_Richieste.Where(x => x.Matrciola == matricola).OrderByDescending(t => t.DataGiornoFine).FirstOrDefault();
                //    DateTime dateFine = new DateTime(q.DataGiornoFine.Date.Year, q.DataGiornoFine.Date.Month, 3);
                //    if (DateTime.Now.AddMonths(2) > dateFine)
                //    {
                //        abbonamento.CreaAbbonamento = true;
                //    }
                //}

            }

            if (cittaperAbbonarsi.ToUpper() == "TORINO")
            {

                //verifico data presenza in campagna 
                var listCampagne = db.B2RaiPlace_Abbonamento_Campagna.Where(a => a.DataInizioCampagna <= DateTime.Today && a.DataFineCampagna >= DateTime.Today);

                //Se esiste una campagna attiva
                if (listCampagne.Count() > 0)
                {
                    //Se l'utente non ha nessuna richiesta di abbonamento
                    if (lista.Count == 0)
                    {
                        abbonamento.CreaAbbonamento = true;
                    }
                    else
                    {
                        var campagnaAttiva = listCampagne.First();
                        var q = db.B2RaiPlace_Abbonamento_Richieste.Where(x => x.Matrciola == matricola).OrderByDescending(t => t.DataGiornoFine).FirstOrDefault();
                        if (campagnaAttiva.DataInizioValidita.HasValue)
                        {
                            if (q.DataGiornoFine < campagnaAttiva.DataInizioValidita.Value)
                                abbonamento.CreaAbbonamento = true;
                        }
                        else
                        {
                            if (DateTime.Now.AddMonths(2) > q.DataGiornoFine.Date)
                                abbonamento.CreaAbbonamento = true;
                        }
                    }
                }
            }
            return abbonamento;

        }


    }
}
