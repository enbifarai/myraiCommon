using myRai.Business;
using myRaiCommonManager;
using myRaiCommonModel;
using myRaiData;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace myRai.Controllers.api
{
    public class GiornataInfoMobile
    {
        public GiornataInfoMobile()
        {
            this.timbrature = new List<timbraturaMobile>();
            this.eccezioni = new List<eccezioneMobile>();
            this.note = new List<NotaSegreteriaMobile>();
        }
        public string orarioreale { get; set; }
        public string orarioprevisto { get; set; }
        public string orariorealedesc { get; set; }
        public string orarioprevistodesc { get; set; }
        public string intervallomensa { get; set; }
        public string carenza { get; set; }
        public string mp { get; set; }
        public string mensafruita { get; set; }
        public List<timbraturaMobile> timbrature { get; set; }
        public List<eccezioneMobile> eccezioni { get; set; }
        public string MensaSede { get; set; }

        public List<NotaSegreteriaMobile> note { get; set; }

    }
    public class eccezioneMobile
    {
        public string codiceEccezione { get; set; }
        public string descEccezione { get; set; }
        public string quantita { get; set; }
        public string dalle { get; set; }
        public string alle { get; set; }
    }
    public class timbraturaMobile
    {
        public string orarioIn { get; set; }
        public string insediamentoIn { get; set; }
        public string insediamentoDescIn { get; set; }
        public string orarioOut { get; set; }
        public string insediamentoOut { get; set; }
        public string insediamentoDescOut { get; set; }
    }
    public class NotaSegreteriaMobile
    {
        public string datacreazione { get; set; }
        public string datagiornata { get; set; }
        public string matricolaMittente { get; set; }
        public string nominativoMittente { get; set; }
        public string messaggio { get; set; }
        public string sede { get; set; }
    }
    public class MobileController : ApiController
    {
        public MobileController()
        {

        }


        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.HttpGet]
        public SaveCarrelloResponse SaveCarrello(int idpdf, string matricola, int addFlag)
        {
            var db = new digiGappEntities();
            matricola = IsSimulata(matricola).Trim('P');
            if (addFlag == 1)
            {
                if (db.MyRai_Carrello.Any(x => x.id_archivio_pdf == idpdf))
                    return new SaveCarrelloResponse() { Esito = false, Errore = "Gia esistente" };

                if ( ! DaFirmareManager.CheckConsecutivo(idpdf, matricola))
                {
                    return new SaveCarrelloResponse() { Esito = false,
                        Errore = "Il pdf non rispetta la consecutività delle convalide, inserire nel carrello uno o più periodi antecedenti a quello selezionato"
                    };
                }
               
                try
                {
                    MyRai_Carrello c = new MyRai_Carrello()
                    {
                        data_pdf_aggiunto = DateTime.Now,
                        matricola = matricola,
                        id_archivio_pdf = idpdf
                    };
                    db.MyRai_Carrello.Add(c);
                    db.SaveChanges();
                    return new SaveCarrelloResponse() { Esito = true };
                }
                catch (Exception ex)
                {
                    return new SaveCarrelloResponse() { Esito = false, Errore = ex.Message };
                }
            }
            else
            {
                if (DaFirmareManager.CheckRemovibile(idpdf, matricola) != "OK")
                {
                    return new SaveCarrelloResponse()
                    {
                        Esito = false,
                        Errore = "Non è possibile rimuovere il documento dal carrello perchè verrebbero generate discontinuità nelle convalide"
                    };
                }
                var item =db.MyRai_Carrello.Where(x => x.id_archivio_pdf == idpdf).FirstOrDefault();
                try
                {
                    if (item != null)
                    {
                        db.MyRai_Carrello.Remove(item);
                        db.SaveChanges();
                    }
                    return new SaveCarrelloResponse() { Esito = true };
                }
                catch (Exception ex)
                {
                    return new SaveCarrelloResponse() { Esito = false, Errore = ex.Message };
                }
            }
            
        }

        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.HttpGet]
        public string SaveMSeg(int idNota, string matr, string nom, string nota)
        {
            MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client cl = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
            var db = new digiGappEntities();
            var notaDB = db.MyRai_Note_Richieste.Where(x => x.Id == idNota).FirstOrDefault();
            if (notaDB == null)
                return "Errore Dati";


            var par = db.MyRai_ParametriSistema.Where(x => x.Chiave == "AccountUtenteServizio").FirstOrDefault();

            cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(
                par.Valore1, par.Valore2
                );
            MyRaiServiceInterface.MyRaiServiceReference1.InserisciNotaRichiestaResponse response
                = new MyRaiServiceInterface.MyRaiServiceReference1.InserisciNotaRichiestaResponse();

            if (notaDB.Mittente == matr)
            {
                response = cl.InserisciNotaRichiesta(matr, nom, nota, notaDB.DataGiornata, notaDB.SedeGapp, notaDB.Destinatario, "Segreteria");
            }
            else
            {
                response = cl.InserisciNotaRichiesta(matr, nom, nota, notaDB.DataGiornata, notaDB.SedeGapp, notaDB.Mittente, "Segreteria");
            }
            if (response.Esito == true)
                return "ok";
            else
                return response.Errore;
        }


        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.HttpGet]
        public GiornataInfoMobile GetGio(string matr, string data, int idnotainiz)
        {
            matr = IsSimulata(matr);
            var giornata = myRaiCommonManager.EccezioniManager.GetGiornataForBatch(data, matr);
            MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client cl = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
            var db = new digiGappEntities();
            var par = db.MyRai_ParametriSistema.Where(x => x.Chiave == "AccountUtenteServizio").FirstOrDefault();

            cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(
                par.Valore1, par.Valore2
                );

            //MyRaiServiceInterface.MyRaiServiceReference1.getOrarioResponse responseOrario =
            //    cl.getOrario(giornata.giornata.orarioTeorico, giornata.giornata.data.ToString("ddMMyyyy"), matr, "BU", 75);

            GiornataInfoMobile G = new GiornataInfoMobile();
            G.orarioreale = giornata.giornata.orarioReale;
            G.orariorealedesc = giornata.giornata.descOrarioReale;
            G.orarioprevisto = giornata.giornata.orarioTeorico;
            G.orarioprevistodesc = db.L2D_ORARIO.Where(x => x.cod_orario == G.orarioprevisto).Select(x => x.desc_orario).FirstOrDefault();
            G.intervallomensa = giornata.orario.intervallo_mensa;
            G.carenza = giornata.giornata.carenza;
            G.mp = giornata.giornata.maggiorPresenza;
            DateTime D;
            DateTime.TryParseExact(data.Replace("/", ""), "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out D);
            var scontrino = CommonManager.MensaFruitaDataMatr(D, matr);
            if (scontrino == null)
                G.mensafruita = "No";
            else
                G.mensafruita = scontrino.TransactionDateTime.ToString("HH.mm");
            if (scontrino != null)
            {
                string[] linee = scontrino.XMLorig.Split(new string[] { "<Line>" }, StringSplitOptions.None)
                    .Where(item => item != "</Line>").ToArray();

                if (linee != null && linee.Length > 2)
                {
                    G.MensaSede = (linee[2] != null ? linee[2].Replace("</Line>", "") : "");
                    if (linee.Length > 3 && !String.IsNullOrWhiteSpace(linee[3]))
                        G.MensaSede += " - " + linee[3].Substring(linee[3].IndexOf('-') + 1).Replace("</Line>", "");
                }

            }
            if (giornata.timbrature != null && giornata.timbrature.Any())
            {
                foreach (var t in giornata.timbrature)
                {
                    timbraturaMobile tm = new timbraturaMobile();
                    if (t.entrata != null)
                    {
                        tm.orarioIn = t.entrata.orario;
                        tm.insediamentoIn = t.entrata.insediamento;
                        tm.insediamentoDescIn = t.entrata.descrittivaInsediamento;
                    }
                    if (t.uscita != null)
                    {
                        tm.orarioOut = t.uscita.orario;
                        tm.insediamentoOut = t.uscita.insediamento;
                        tm.insediamentoDescOut = t.uscita.descrittivaInsediamento;
                    }
                    G.timbrature.Add(tm);
                }
            }
            if (giornata.eccezioni != null && giornata.eccezioni.Any())
            {
                foreach (var ecc in giornata.eccezioni)
                {
                    eccezioneMobile e = new eccezioneMobile()
                    {
                        alle = ecc.alle,
                        codiceEccezione = ecc.cod,
                        dalle = ecc.dalle,
                        descEccezione = ecc.descrittiva_lunga,
                        quantita = ecc.qta
                    };
                    G.eccezioni.Add(e);
                }

            }
            var notainiziale = db.MyRai_Note_Richieste.Where(x => x.Id == idnotainiz).FirstOrDefault();
            if (notainiziale != null)
            {
                List<MyRai_Note_Richieste> note = new List<MyRai_Note_Richieste>();

                note = db.MyRai_Note_Richieste.Where(x => x.DataGiornata == notainiziale.DataGiornata &&
                (x.Mittente == notainiziale.Mittente || x.Destinatario == notainiziale.Mittente)
                )

                .OrderBy(x => x.DataCreazione)
                .ToList();


                foreach (var nota in note)
                {
                    G.note.Add(new NotaSegreteriaMobile()
                    {
                        datacreazione = nota.DataCreazione.ToString("ddMMyyyy HH.mm"),
                        datagiornata = nota.DataGiornata.ToString("ddMMyyyy"),
                        matricolaMittente = nota.Mittente,
                        nominativoMittente = nota.DescrizioneMittente,
                        messaggio = nota.Messaggio,
                        sede = nota.SedeGapp
                    });
                }
            }

            return G;
        }

        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.HttpGet]

        public ApprovazioniResponse Approva(bool appr, string ids, string matr, string mot)
        {
            matr = IsSimulata(matr);
            if (String.IsNullOrWhiteSpace(ids) || String.IsNullOrWhiteSpace(matr))
                return new ApprovazioniResponse() { Esito = false, Errore = "Dati non completi per le approvazioni" };

            if (appr == false && String.IsNullOrWhiteSpace(mot))
                return new ApprovazioniResponse() { Esito = false, Errore = "Motivazione mancante" };


            digiGappEntities db = new digiGappEntities();

            int stato = (appr ? 20 : 50);
            DateTime D = DateTime.Now;
            try
            {
                foreach (string id in ids.Split(','))
                {
                    int idint;
                    if (!Int32.TryParse(id, out idint))
                        continue;

                    if (db.MyRai_ApprovazioneMassiva.Any(x => x.IdRichiesta == idint))
                        continue;

                    MyRai_ApprovazioneMassiva app = new MyRai_ApprovazioneMassiva();
                    app.IdNuovoStatoRichiesta = stato;
                    app.IdRichiesta = Convert.ToInt32(id);
                    app.Nota = mot;
                    app.MatricolaApprovatore = matr;
                    app.DataApprovazione = D;
                    app.Provenienza = "Mobile";
                    app.Status = 1;
                    db.MyRai_ApprovazioneMassiva.Add(app);
                }
                db.SaveChanges();
                return new ApprovazioniResponse() { Esito = true };
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "Mobile",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = matr,
                    provenienza = "Approva"
                }, matr);
                return new ApprovazioniResponse() { Esito = false, Errore = ex.Message };
            }
        }




        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.HttpGet]
        public string SetLettura(int id, string matr, string nominativo)
        {
            matr = IsSimulata(matr);
            MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client cl = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
            var db = new digiGappEntities();
            var par = db.MyRai_ParametriSistema.Where(x => x.Chiave == "AccountUtenteServizio").FirstOrDefault();

            cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(
                par.Valore1, par.Valore2
                );
            MyRaiServiceInterface.MyRaiServiceReference1.SetLetturaResponse resp =
                cl.SetLettura(id, matr, nominativo, true);

            if (resp.Esito == true)
                return "OK";
            else
                return resp.Errore;

        }

        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.HttpGet]
        public NoteSegreteriaResponse GetMSeg(string matr)
        {
            matr = IsSimulata(matr);
            //api/mobile/getmseg?matr=103650   
            NoteSegreteriaResponse response = MobileManager.GetMessaggiSegreteria(matr, true);
            return response;

        }

        private string IsSimulata(string matr)
        {
            string matrSimulata = matr;

            digiGappEntities db = new digiGappEntities();
            var sim = db.MyRai_ParametriSistema.Where(x => x.Chiave == "matricolasimulata" && x.Valore2 == matr).FirstOrDefault();
            if (sim != null)
                matrSimulata = sim.Valore1.Split('\\')[1];

            return matrSimulata;
        }

        [System.Web.Http.AcceptVerbs("GET")]
        [System.Web.Http.HttpGet]
        public MobileResponse Register([FromUri] RegistrationInfo R)
        {
            digiGappEntities db = new digiGappEntities();
            //var sim = db.MyRai_ParametriSistema.Where(x => x.Chiave == "matricolasimulata" && x.Valore2 == R.pmatricola).FirstOrDefault();
            //if (sim!=null) 
            //    R.pmatricola=sim.Valore1.Split ('\\')[1];

            R.pmatricola = IsSimulata(R.pmatricola);

            if (R.pmatricola == null) R.pmatricola = "";

            string req = "";
            if (Request != null && Request.RequestUri != null) req = Request.RequestUri.ToString();


            try
            {
                var l = new MyRai_LogAzioni()
                {
                    applicativo = "MOBILE",
                    data = DateTime.Now,
                    matricola = R.pmatricola,
                    operazione = "RegistrazioneMobile",
                    descrizione_operazione = req + " - " + Newtonsoft.Json.JsonConvert.SerializeObject(R),
                    provenienza = "Register"
                };
                db.MyRai_LogAzioni.Add(l);
                db.SaveChanges();
            }
            catch (Exception) { }


            if (String.IsNullOrWhiteSpace(R.deviceID))
            {
                var err = new MyRai_LogErrori()
                {
                    applicativo = "MOBILE",
                    data = DateTime.Now,
                    error_message = "deviceID non specificato",
                    matricola = R.pmatricola,
                    provenienza = "Register"
                };
                db.MyRai_LogErrori.Add(err);
                db.SaveChanges();
                return new MobileResponse() { esito = false, error = "DeviceID non specificato" };
            }

            if (String.IsNullOrWhiteSpace(R.pmatricola))
            {
                var err = new MyRai_LogErrori()
                {
                    applicativo = "MOBILE",
                    data = DateTime.Now,
                    error_message = "pmatricola non specificato",
                    matricola = " ",
                    provenienza = "Register"
                };
                db.MyRai_LogErrori.Add(err);
                db.SaveChanges();
                return new MobileResponse() { esito = false, error = "pmatricola non specificato" };
            }


            MobileResponse resp = new MobileResponse();

            var mob = db.MyRai_MobileRegistration.Where(x => x.pmatricola == R.pmatricola && x.device_id == R.deviceID).FirstOrDefault();
            if (mob == null)
            {
                MyRai_MobileRegistration Reg = new MyRai_MobileRegistration()
                {
                    abilitato = true,
                    device_id = R.deviceID,
                    device_idiom = R.idiom,
                    device_manufacturer = R.manufacturer,
                    device_model = R.devicemodel,
                    device_name = R.deviceName,
                    device_phonenbr = null,// R.phonenbr,
                    device_platform = R.platform,
                    device_type = R.deviceType,
                    device_version = R.version,
                    fingerprint_available = R.HasFingerPrint,
                    last_access = DateTime.Now,
                    pmatricola = R.pmatricola,
                    token = R.token,
                    token_datetime = !String.IsNullOrWhiteSpace(R.token) ? DateTime.Now : default(DateTime?)
                };
                db.MyRai_MobileRegistration.Add(Reg);
            }
            else
            {
                if (!mob.abilitato)
                {
                    var err = new MyRai_LogErrori()
                    {
                        applicativo = "MOBILE",
                        data = DateTime.Now,
                        error_message = "Dispositivo disabilitato: " + mob.device_id,
                        matricola = R.pmatricola,
                        provenienza = "Register"
                    };
                    db.MyRai_LogErrori.Add(err);
                    db.SaveChanges();

                    resp.esito = false;
                    resp.error = "Questo dispositivo non è abilitato all'accesso.";
                    return resp;
                }
                mob.last_access = DateTime.Now;
                if (mob.token != R.token && !String.IsNullOrWhiteSpace(R.token))
                {
                    mob.token = R.token;
                    mob.token_datetime = !String.IsNullOrWhiteSpace(R.token) ? DateTime.Now : default(DateTime?);
                }
                if (mob.fingerprint_available != R.HasFingerPrint)
                    mob.fingerprint_available = R.HasFingerPrint;
            }

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                var err = new MyRai_LogErrori()
                {
                    applicativo = "MOBILE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = R.pmatricola,
                    provenienza = "Register"
                };
                db.MyRai_LogErrori.Add(err);
                db.SaveChanges();

                resp.esito = false;
                resp.error = ex.Message;
            }

            if (R.needAnagrafica)
            {
                it.rai.servizi.hrgb.Service service = new it.rai.servizi.hrgb.Service();
                resp.AnagraficaCSV = service.EsponiAnagrafica("RAICV;" + R.pmatricola.Substring(1) + ";;E;0");
                resp.ImageBase64 = CommonManager.GetImmagineBase64ForApp(R.pmatricola.Substring(1));
                resp.IsSeg = BatchManager.GetSediSegreteria(R.pmatricola.Substring(1)).Any();
                resp.IsUffPers = BatchManager.GetSediUffPers(R.pmatricola.Substring(1)).Any();
                var ut = BatchManager.GetUserData(R.pmatricola.Substring(1));
                if (ut != null && !String.IsNullOrWhiteSpace(ut.tipo_dipendente))
                {
                    resp.TipoDip = ut.tipo_dipendente;
                }
                if (R.pmatricola != null)
                {
                    Abilitazioni AB = BatchManager.getAbilitazioni();
                    var sedi = AB.ListaAbilitazioni
                       .Where(x => x.MatrLivello1.Any(z => z.Matricola.ToUpper() == R.pmatricola.ToUpper()))
                       .Select(a => a.Sede).ToList();

                    resp.IsL1 = sedi.Any();

                    var sediL2 = AB.ListaAbilitazioni
                       .Where(x => x.MatrLivello2.Any(z => z.Matricola.ToUpper() == R.pmatricola.ToUpper()))
                       .Select(a => a.Sede).ToList();
                    resp.IsL2 = sediL2.Any();
                }

            }
            return resp;

        }
    }


    public class MobileResponse
    {
        public MobileResponse()
        {
            this.esito = true;
        }
        public bool esito { get; set; }
        public string error { get; set; }

        public string AnagraficaCSV { get; set; }
        public string ImageBase64 { get; set; }
        public bool IsSeg { get; set; }
        public bool IsUffPers { get; set; }
        public bool IsL1 { get; set; }
        public bool IsL2 { get; set; }
        public string TipoDip { get; set; }

    }

    public class RegistrationInfo
    {
        public bool HasFingerPrint;
        public string phonenbr { get; set; }
        public string deviceID { get; set; }
        public string devicemodel { get; set; }
        public string manufacturer { get; set; }
        public string deviceName { get; set; }
        public string version { get; set; }
        public string platform { get; set; }
        public string idiom { get; set; }
        public string deviceType { get; set; }
        public string token { get; set; }

        public string pmatricola { get; set; }

        public bool needAnagrafica { get; set; }

    }
    public class ApprovazioniResponse
    {
        public bool Esito { get; set; }
        public string Errore { get; set; }
    }
    public class SaveCarrelloResponse : ApprovazioniResponse
    {
}
}