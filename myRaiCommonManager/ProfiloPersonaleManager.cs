using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using myRai.DataAccess;
using myRaiCommonModel;
using myRaiData;
using System.Data.SqlClient;
using myRaiHelper;
using myRai.Data.CurriculumVitae;
using myRai.Business;
using myRaiData.Incentivi;
using CMINFOANAG_EXT = myRaiData.Incentivi.CMINFOANAG_EXT;

namespace myRaiCommonManager
{
    public class WebClientApiRaiPlace : WebClient
    {
        public string url { get; set; }
        public ApiResponse RaiPlaceResponse { get; set; }
        public WebClientApiRaiPlace(string Keystring, string url, string username = null, string password = null)
        {
            this.url = url;
            this.Headers.Add("keystring", Keystring);
            CredentialCache credCache = new CredentialCache();
            credCache.Add(
                new Uri(url),
                "Negotiate",
                username != null ? new NetworkCredential(username, password, "RAI")
								: CredentialCache.DefaultNetworkCredentials
                );
           
            this.Credentials = credCache;
            this.RaiPlaceResponse = new ApiResponse();
        }

        public ApiResponse ExecuteCall()
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse>(this.DownloadString(url));
        }

        protected override WebResponse GetWebResponse (WebRequest request)
        {
            try
            {
                request.Method = "POST";
                return base.GetWebResponse(request);
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                 applicativo="PORTALE",
                  data=DateTime.Now ,
                   error_message=ex.ToString(),
                    matricola = CommonManager.GetCurrentUserMatricola(),
                     provenienza="WebClientApiRaiPlace.GetWebResponse"
                });
                return null;
            }
        }
        
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest w = (HttpWebRequest)base.GetWebRequest(address);
            w.Method = "POST";
            w.AllowAutoRedirect = true;
            w.CookieContainer = new CookieContainer();
            return w;
        }

        public class ApiResponse
        {
            public string esito { get; set; }
            public string code { get; set; }
            public string message { get; set; }
            public string data { get; set; }
        }
    }

    public class ProfiloPersonaleManager
    {
        private static MyRai_InvioNotifiche GetImpostazioneLivello1(EnumTipoEventoNotifica tipoEvento)
        {
            var db = new digiGappEntities();
            string matr = CommonManager.GetCurrentUserMatricola();
            string tipoDestinatario = EnumTipoDestinatarioNotifiche.ESS_LIV1.ToString();
            string tipoEv = tipoEvento.ToString();

            MyRai_InvioNotifiche item = db.MyRai_InvioNotifiche.Where(x =>
                       (x.Matricola == matr || x.Matricola == "*") &&
                       x.TipoDestinatario == tipoDestinatario &&
                       x.TipoEvento == tipoEv)
                       .OrderByDescending(x => x.Matricola)
                       .FirstOrDefault();

            return item;
        }

        private static ProfiloPersonaleModel GetImpostazioniNotificheDipendente(ProfiloPersonaleModel model)
        {
            var db = new digiGappEntities();
            string matr = CommonManager.GetCurrentUserMatricola();
            string tipoDestinatario = EnumTipoDestinatarioNotifiche.ESS_DIP.ToString();
            string APPR = EnumTipoEventoNotifica.APPR.ToString();
            string RIF = EnumTipoEventoNotifica.RIF.ToString();
            string SCAD = EnumTipoEventoNotifica.SCAD.ToString();

            MyRai_InvioNotifiche ImpostazioniAPPR = db.MyRai_InvioNotifiche.Where(x =>
                        (x.Matricola == matr) &&
                        x.TipoDestinatario == tipoDestinatario &&
                        x.TipoEvento == APPR
                        ).FirstOrDefault();
            if (ImpostazioniAPPR == null)
            {
                ImpostazioniAPPR = db.MyRai_InvioNotifiche.Where(x =>
                        (x.Matricola == "*") &&
                        x.TipoDestinatario == tipoDestinatario &&
                        x.TipoEvento == APPR
                        ).FirstOrDefault();
            }
            model.ImpostazioniDIP_APPR = ImpostazioniAPPR;


            if (
                (model.ImpostazioniDIP_APPR.TipoInvio == "G" || model.ImpostazioniDIP_APPR.TipoInvio == "S")
                && !String.IsNullOrWhiteSpace(model.ImpostazioniDIP_APPR.OraMinuti)
                && model.ImpostazioniDIP_APPR.OraMinuti.Length == 4)
            {
                model.OreDipAppr = model.ImpostazioniDIP_APPR.OraMinuti.Substring(0, 2);
                model.MinDipAppr = model.ImpostazioniDIP_APPR.OraMinuti.Substring(2, 2);
            }

            MyRai_InvioNotifiche ImpostazioniRIF = db.MyRai_InvioNotifiche.Where(x =>
                       (x.Matricola == matr) &&
                       x.TipoDestinatario == tipoDestinatario &&
                       x.TipoEvento == RIF
                       ).FirstOrDefault();
            if (ImpostazioniRIF == null)
            {
                ImpostazioniRIF = db.MyRai_InvioNotifiche.Where(x =>
                        (x.Matricola == "*") &&
                        x.TipoDestinatario == tipoDestinatario &&
                        x.TipoEvento == RIF
                        ).FirstOrDefault();
            }

            model.ImpostazioniDIP_RIF = ImpostazioniRIF;
            
			if (
               (model.ImpostazioniDIP_RIF.TipoInvio == "G" || model.ImpostazioniDIP_RIF.TipoInvio == "S")
               && !String.IsNullOrWhiteSpace(model.ImpostazioniDIP_RIF.OraMinuti)
               && model.ImpostazioniDIP_RIF.OraMinuti.Length == 4)
            {
                model.OreDipRif = model.ImpostazioniDIP_RIF.OraMinuti.Substring(0, 2);
                model.MinDipRif = model.ImpostazioniDIP_RIF.OraMinuti.Substring(2, 2);
            }

            MyRai_InvioNotifiche ImpostazioniSCAD = db.MyRai_InvioNotifiche.Where(x =>
                      (x.Matricola == matr) &&
                      x.TipoDestinatario == tipoDestinatario &&
                      x.TipoEvento == SCAD
                      ).FirstOrDefault();

            if (ImpostazioniSCAD == null)
            {
                ImpostazioniSCAD = db.MyRai_InvioNotifiche.Where(x =>
                        (x.Matricola == "*") &&
                        x.TipoDestinatario == tipoDestinatario &&
                        x.TipoEvento == SCAD
                        ).FirstOrDefault();
            }
            model.ImpostazioniDIP_SCAD = ImpostazioniSCAD;

            if (
              (model.ImpostazioniDIP_SCAD.TipoInvio == "G" || model.ImpostazioniDIP_SCAD.TipoInvio == "S")
              && !String.IsNullOrWhiteSpace(model.ImpostazioniDIP_SCAD.OraMinuti)
              && model.ImpostazioniDIP_SCAD.OraMinuti.Length == 4)
            {
                model.OreDipScad = model.ImpostazioniDIP_SCAD.OraMinuti.Substring(0, 2);
                model.MinDipScad = model.ImpostazioniDIP_SCAD.OraMinuti.Substring(2, 2);
            }

            return model;
        }


        public static List<SelectListItem> GetComuni(string filter, string value)
        {
            List<SelectListItem> result = new List<SelectListItem>();

            var db = ProfiloPersonaleManager.GetTalentiaDB();

            if (!String.IsNullOrWhiteSpace(value))
                result.AddRange(db.TB_COMUNE.Where(x => x.COD_CITTA == value).ToList()
                    .Select(x => new SelectListItem()
                    {
                        Value = x.COD_CITTA,
                        Text = x.DES_CITTA.TitleCase() + ", " + x.COD_PROV_STATE /*+ ", "  + x.COD_CITTA*/,
                        Selected = true
                    }
                    ));
            else
                result.AddRange(db.TB_COMUNE.Where(x => (x.DES_CITTA + ", " + x.COD_PROV_STATE /*+ ", " + x.COD_CITTA*/).StartsWith(filter)).ToList()
                    .Select(x => new SelectListItem()
                    {
                        Value = x.COD_CITTA,
                        Text = x.DES_CITTA.TitleCase() + ", " + x.COD_PROV_STATE /*+ ", " + x.COD_CITTA*/
                    }));

            return result;
        }


        public static DataSet GetProfiloPersonaleFromDB(string matricola)
        {
            try
            {
                SqlCommand sqlCommand = new SqlCommand();
                SqlDataAdapter da = new SqlDataAdapter(sqlCommand);

                DataSet ds = new DataSet();
                using (SqlConnection sqlConnection = new SqlConnection())
                {
                    
                    sqlConnection.ConnectionString = CommonHelper.GetAppSettings("connectionProfPers").ToString();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.CommandText = "sp_XR_WSGETRAPPBAN";

                    sqlCommand.Parameters.Add("@parametro1", SqlDbType.VarChar);
                    sqlCommand.Parameters["@parametro1"].Value = matricola;
                    da.Fill(ds);
                }
                return ds;
            }

            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                 applicativo="portale",
                  data=DateTime.Now,
                   error_message=ex.ToString(),
                    matricola = CommonManager.GetCurrentUserMatricola(),
                 provenienza = "GetProfiloPersonaleFromDB"

                });
                return null;
            }
        }

        public static myRaiData.Incentivi.IncentiviEntities GetTalentiaDB()
        {
            var flagdb = myRai.Business.CommonManager.GetParametri<string>(EnumParametriSistema.FlagTalentiaCezanne);
            if (flagdb[0].Equals("0"))
            {
                return new myRaiData.Incentivi.IncentiviEntities("IncentiviEntities_Cezanne");
            }
            else
            {
                return new myRaiData.Incentivi.IncentiviEntities("IncentiviEntities_Talentia");
            }
        }


        public static Domicilio GetDomicilioTalentia(string matricola)
        {

            

            Domicilio dom = new Domicilio();

            var db = ProfiloPersonaleManager.GetTalentiaDB();

            myRaiData.Incentivi.SINTESI1 y = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT.Equals(matricola));

            if (y != null)
            {

                var domicilio = db.ANAGPERS.FirstOrDefault(x => x.ID_PERSONA == y.ID_PERSONA);


                if (domicilio!=null)
                {

                    var comune = db.TB_COMUNE.FirstOrDefault(x => x.COD_CITTA == domicilio.COD_CITTADOM);
                    if (comune != null)
                    {
                        dom.prov = comune.COD_PROV_STATE;
                        dom.citta = comune.DES_CITTA;
                        dom.stato = comune.COD_SIGLANAZIONE.ToUpper();

                    }
                    else
                    {

                        dom.prov = "-";
                        dom.citta = "-";
                        dom.stato = "-";
                    }
                    dom.Indirizzo = domicilio.DES_INDIRDOM;

                    dom.cap = domicilio.CAP_CAPDOM;


                }
                else
                {
                    dom.prov = "-";
                    dom.citta = "-";
                    dom.stato = "-";
                    dom.Indirizzo = "-";
                    dom.cap = "-";

                }

            }
            else
            {
                dom.prov = "-";
                dom.citta = "-";
                dom.stato = "-";
                dom.Indirizzo = "-";
                dom.cap = "-";


            }





            return dom;
        }

        public static Residenza GetResidenzaTalentia(string matricola)
        {

            DateTime fine = new DateTime(2999, 12, 31);

            Residenza res = new Residenza();

            var db = ProfiloPersonaleManager.GetTalentiaDB();

            myRaiData.Incentivi.SINTESI1 y = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT.Equals(matricola));

            if (y != null)
            {

                var resid = db.RESIDENZA.Where(x => x.ID_PERSONA == y.ID_PERSONA).ToList();


                if (resid != null && resid.Count() > 0)
                {


                    foreach (var item in resid)
                    {
                        if ( item.DTA_FINE.Equals(fine))
                        {
                            var comune = db.TB_COMUNE.FirstOrDefault(x => x.COD_CITTA == item.COD_CITTA);
                            if (comune != null) { 
                            res.prov = comune.COD_PROV_STATE;
                            res.citta = comune.DES_CITTA;
                            res.stato = comune.COD_SIGLANAZIONE.ToUpper();

                            }
                            else
                            {

                                res.prov = "-";
                                res.citta = "-";
                                res.stato = "-";
                            }

                            res.Indirizzo = item.DES_INDIRRESID;
                            res.cap = item.CAP_CAPRESID;


                        }





                    }

                }
                else
                {
                    res.prov = "-";
                    res.citta = "-";
                    res.stato = "-";
                    res.Indirizzo = "-";
                    res.cap = "-";

                }

            }
            else
            {
                res.prov = "-";
                res.citta = "-";
                res.stato = "-";
                res.Indirizzo = "-";
                res.cap = "-";


            }





            return res;
        }

        public static List<ContoCorrente> GetListaContoCorrenteTalentia(string matricola)
        {

            DateTime fine = new DateTime(2999, 12, 31);

            List<ContoCorrente> conti = new List<ContoCorrente>();

            var db = ProfiloPersonaleManager.GetTalentiaDB();

            myRaiData.Incentivi.SINTESI1 y = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT.Equals(matricola));

            if (y != null)
            {

                var datibancari = db.XR_DATIBANCARI.Where(x => x.ID_PERSONA == y.ID_PERSONA).ToList();


                if (datibancari != null && datibancari.Count() > 0)
                {


                    foreach (var item in datibancari)
                    {
                        if (item.COD_TIPOCONTO.Equals("C") && item.DTA_FINE.Equals(fine))
                        {
                            myRaiData.Incentivi.XR_ANAGBANCA anagBanca = db.XR_ANAGBANCA.Include("TB_COMUNE").FirstOrDefault(x => x.COD_ABI == item.COD_ABI && x.COD_CAB == item.COD_CAB);
                            ContoCorrente contocorrente = new ContoCorrente();
                            if (anagBanca != null)
                            {
                                contocorrente.Indirizzo_Agenzia = anagBanca.DES_INDIRIZZO;
                                contocorrente.Citta_Agenzia = anagBanca.TB_COMUNE != null ? anagBanca.TB_COMUNE.DES_CITTA : "";
                                contocorrente.Agenzia = anagBanca.DES_RAG_SOCIALE;


                            }
                            else
                            {

                                contocorrente.Indirizzo_Agenzia = "-";
                                contocorrente.Citta_Agenzia = "-";
                                contocorrente.Agenzia = "-";

                            }

                            contocorrente.Tipologia = "accredito stipendio";
                            contocorrente.Iban = item.COD_IBAN;
                            contocorrente.Attivo_dal = item.DTA_INIZIO.ToString();
                            contocorrente.flag_congelato = item.IND_CONGELATO;
                            contocorrente.flag_vincolato = item.IND_VINCOLATO;
                            contocorrente.Codice_Utilizzo = "01";

                            if (conti != null) conti.Add(contocorrente);

                        }

                        else

                        if (item.COD_TIPOCONTO.Equals("R") && item.DTA_FINE.Equals(fine))
                        {
                            var util = db.XR_UTILCONTO.Where(x => x.ID_XR_DATIBANCARI == item.ID_XR_DATIBANCARI).ToList();
                            myRaiData.Incentivi.XR_ANAGBANCA anagBanca = db.XR_ANAGBANCA.Include("TB_COMUNE").FirstOrDefault(x => x.COD_ABI == item.COD_ABI && x.COD_CAB == item.COD_CAB);

                            foreach (var anticipo in util)
                            {
                                myRaiData.Incentivi.XR_ANAGBANCA anagBanca2 = db.XR_ANAGBANCA.FirstOrDefault(x => x.COD_ABI == item.COD_ABI && x.COD_CAB == item.COD_CAB);
                                ContoCorrente contocorrente = new ContoCorrente();
                                if (anagBanca2 != null)
                                {
                                    contocorrente.Indirizzo_Agenzia = anagBanca.DES_INDIRIZZO;
                                    contocorrente.Citta_Agenzia = anagBanca.TB_COMUNE != null ? anagBanca.TB_COMUNE.DES_CITTA : "";
                                    contocorrente.Agenzia = anagBanca.DES_RAG_SOCIALE;

                                }
                                else
                                {

                                    contocorrente.Indirizzo_Agenzia = "-";
                                    contocorrente.Citta_Agenzia = "-";
                                    contocorrente.Agenzia = "-";

                                }

                                if (anticipo.COD_UTILCONTO.Equals("02"))
                                {
                                    contocorrente.Tipologia = "anticipo trasferte";
                                }
                                else
                                if (anticipo.COD_UTILCONTO.Equals("03"))
                                {
                                    contocorrente.Tipologia = "anticipo spese di produzione";
                                }


                                contocorrente.Iban = item.COD_IBAN;
                                contocorrente.Attivo_dal = item.DTA_INIZIO.ToString();
                                contocorrente.flag_congelato = item.IND_CONGELATO;
                                contocorrente.flag_vincolato = item.IND_VINCOLATO;
                                contocorrente.Codice_Utilizzo = anticipo.COD_UTILCONTO;


                                if (conti != null) conti.Add(contocorrente);



                            }



                        }



                    }

                }
                else
                {
                    ContoCorrente contocorrente = new ContoCorrente();
                    contocorrente.Tipologia = "-";
                    contocorrente.Indirizzo_Agenzia = "-";
                    contocorrente.Citta_Agenzia = "-";
                    contocorrente.Agenzia = "-";
                    contocorrente.Iban = "-";
                    contocorrente.Attivo_dal = "-";
                    contocorrente.flag_congelato = "N";
                    contocorrente.flag_vincolato = "N";
                    if (conti != null) conti.Add(contocorrente);

                }

            }
            else
            {
                ContoCorrente contocorrente = new ContoCorrente();
                contocorrente.Tipologia = "-";
                contocorrente.Indirizzo_Agenzia = "-";
                contocorrente.Citta_Agenzia = "-";
                contocorrente.Agenzia = "-";
                contocorrente.Iban = "-";
                contocorrente.Attivo_dal = "-";
                contocorrente.flag_congelato = "N";
                contocorrente.flag_vincolato = "N";
                if (conti != null) conti.Add(contocorrente);

            }





            return conti;
        }

        public static ProfiloPersonaleModel GetListaIbanDaConv(int matr)
        {
            ProfiloPersonaleModel model = new ProfiloPersonaleModel();

            model.conticorrentidaconv = new List<ContoCorrenteDaConv>();



            var db = ProfiloPersonaleManager.GetTalentiaDB();



            var ccDB = db.CMINFOANAG_EXT.Where(c => c.COD_CONVALIDA_CC.Equals("0") && c.ID_PERSONA == matr && (c.COD_CMEVENTO.Equals("INSCC")|| c.COD_CMEVENTO.Equals("MODCC")|| c.COD_CMEVENTO.Equals("DELCC"))).ToList();


            if (ccDB != null)
            {

                foreach (var conto in ccDB)
                {

                    ContoCorrenteDaConv ccC = new ContoCorrenteDaConv();

                    ccC.contocDB = conto;

                    ccC.ccVis = new ContoCorrente();

                    if (ccC.contocDB.COD_UTILIZZO.Equals("01"))
                    {
                        ccC.ccVis.Tipologia =
                            "Accredito Stipendio";
                    }
                    else

                    if (ccC.contocDB.COD_UTILIZZO.Equals("02"))
                    {
                        ccC.ccVis.Tipologia = "Anticipo Trasferte";

                    }
                    else

                    if (ccC.contocDB.COD_UTILIZZO.Equals("03"))
                    {
                        ccC.ccVis.Tipologia = "Anticipo Spese di produzione";
                    }

                    ccC.ccVis.Iban = ccC.contocDB.COD_IBAN;

                    myRaiData.Incentivi.XR_ANAGBANCA anagBanca = db.XR_ANAGBANCA.Include("TB_COMUNE").FirstOrDefault(x => x.COD_ABI == ccC.contocDB.COD_ABI && x.COD_CAB == ccC.contocDB.COD_CAB);
                    
                    if (anagBanca != null)
                    {
                        ccC.ccVis.Agenzia = anagBanca.DES_RAG_SOCIALE;
                        ccC.ccVis.Indirizzo_Agenzia = anagBanca.DES_INDIRIZZO;
                        ccC.ccVis.Citta_Agenzia = anagBanca.TB_COMUNE != null ? anagBanca.TB_COMUNE.DES_CITTA : "";
                    }
                    else
                    {
                        ccC.ccVis.Agenzia = "Non presente in anagrafica";
                        ccC.ccVis.Indirizzo_Agenzia = "Non presente in anagrafica";
                        // ccC.ccVis.Citta_Agenzia = "Non presente in anagrafica";
                    }

                    model.conticorrentidaconv.Add(ccC);

                }

            }


            return model;
        }

        public static List<Residenza> GetResidenzaDaConv(string matr)
        {

            var db = ProfiloPersonaleManager.GetTalentiaDB();
            List<myRaiData.Incentivi.CMINFOANAG_EXT> mod_residenza = new List<myRaiData.Incentivi.CMINFOANAG_EXT>();
            List<Residenza> daconv = new List<Residenza>();


            myRaiData.Incentivi.SINTESI1 y = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT.Equals(matr));

            List<CMINFOANAG_EXT> resDB = db.CMINFOANAG_EXT.Where(c => c.COD_CONVALIDA_CC.Equals("0") && c.ID_PERSONA == y.ID_PERSONA && (c.COD_CMEVENTO.Equals("MODRES"))).ToList();


            if (resDB != null)
            {

                foreach (var item in resDB)
                {

                    Residenza res = new Residenza();

                    var comune = db.TB_COMUNE.FirstOrDefault(x => x.COD_CITTA == item.COD_CITTARES);
                    if (comune != null)
                    {
                        res.prov = comune.COD_PROV_STATE;
                        res.citta = comune.DES_CITTA;
                        res.stato = comune.COD_SIGLANAZIONE.ToUpper();

                    }
                    else
                    {

                        res.prov = "-";
                        res.citta = "-";
                        res.stato = "-";
                    }

                    res.Indirizzo = item.DES_INDIRRES;
                    res.cap = item.CAP_CAPRES;
                    res.ID_EVENTO = item.ID_EVENTO;

                    if (item.IND_ASSEGNA_DOM == "S")
                    {
                        res.anchedom = "S";
                    }
                    else
                    {
                        res.anchedom = "N";
                    }

                    daconv.Add(res);


                }

            }
            else
            {
                daconv = null;
            }


            return daconv;
        }


        public static List<Domicilio> GetDomicilioDaConv(string matr)
        {

            var db = ProfiloPersonaleManager.GetTalentiaDB();
            List<myRaiData.Incentivi.CMINFOANAG_EXT> mod_domicilio = new List<myRaiData.Incentivi.CMINFOANAG_EXT>();
            List<Domicilio> daconv = new List<Domicilio>();

            myRaiData.Incentivi.SINTESI1 y = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT.Equals(matr));

            List<CMINFOANAG_EXT> domDB = db.CMINFOANAG_EXT.Where(c => c.COD_CONVALIDA_CC.Equals("0") && c.ID_PERSONA == y.ID_PERSONA && (c.COD_CMEVENTO.Equals("MODDOM"))).ToList();


            if (domDB != null)
            {

                foreach (var item in domDB)
                {
                    Domicilio dom = new Domicilio();

                    var comune = db.TB_COMUNE.FirstOrDefault(x => x.COD_CITTA == item.COD_CITTADOM);
                    if (comune != null)
                    {
                        dom.prov = comune.COD_PROV_STATE;
                        dom.citta = comune.DES_CITTA;
                        dom.stato = comune.COD_SIGLANAZIONE.ToUpper();

                    }
                    else
                    {

                        dom.prov = "-";
                        dom.citta = "-";
                        dom.stato = "-";
                    }

                    dom.Indirizzo = item.DES_INDIRDOM;
                    dom.cap = item.CAP_CAPDOM;
                    dom.ID_EVENTO = item.ID_EVENTO;


                    daconv.Add(dom);


                }

            }
            else
            {
                daconv = null;
            }


            return daconv;
        }

        public static Boolean ModificaDomSospesa(string matr)
        {
            int anno = DateTime.Today.Year;
            int mese = DateTime.Today.Month;
            int giorno = DateTime.Today.Day;
            var db = ProfiloPersonaleManager.GetTalentiaDB();
            List<myRaiData.Incentivi.CMINFOANAG_EXT> mod_domicilio = new List<myRaiData.Incentivi.CMINFOANAG_EXT>();
            

            myRaiData.Incentivi.SINTESI1 y = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT.Equals(matr));

            List<CMINFOANAG_EXT> domDB = db.CMINFOANAG_EXT.Where(c => c.COD_CONVALIDA_CC.Equals("1") && c.ID_PERSONA == y.ID_PERSONA && (c.COD_CMEVENTO.Equals("MODDOM")) && (c.DTA_CONVALIDA != null) && (c.DTA_APPLICAZIONE == null)).OrderByDescending(c=>c.DTA_CONVALIDA).ToList();


            if (domDB != null && domDB.Count() > 0)
            {
                return true;
                
                

            }
            else
            {
                return false;
            }


            
        }

        public static List<CMINFOANAG_EXT> GetListaCellDaConv(string matr)
        {

            var db = ProfiloPersonaleManager.GetTalentiaDB();
            List <myRaiData.Incentivi.CMINFOANAG_EXT> recapiti  = new List<myRaiData.Incentivi.CMINFOANAG_EXT>();
            
            myRaiData.Incentivi.SINTESI1 y = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT.Equals(matr));

            List<CMINFOANAG_EXT> cellDB = db.CMINFOANAG_EXT.Where(c => c.COD_CONVALIDA_CC.Equals("0") && c.ID_PERSONA == y.ID_PERSONA && (c.COD_CMEVENTO.Equals("INSCEL") || c.COD_CMEVENTO.Equals("MODCEL") || c.COD_CMEVENTO.Equals("DELCEL"))).ToList();


            if (cellDB != null)
            {

                foreach (var item in cellDB)
                {

                    CMINFOANAG_EXT cell = item;



                    if (item.TIPO_RECAPITO.Equals("02"))
                    {
                        cell.TIPO_RECAPITO = "Numero Aziendale";
                    }
                    else 
                    if (item.TIPO_RECAPITO.Equals("01"))
                    {
                        cell.TIPO_RECAPITO = "Numero Personale";
                    }
                    else
                    {
                        cell.TIPO_RECAPITO = "Non dichiarata";
                    }
          



                    recapiti.Add(cell);


                }

            }
            else
            {
                recapiti = null;
            }


            return recapiti;
        }


        public static List<XR_RECAPITI> GetListaRecapiti(int matr)
        {

            string matricola = matr.ToString();
            var db = ProfiloPersonaleManager.GetTalentiaDB();
            myRaiData.Incentivi.SINTESI1 y = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT.Equals(matricola));

            if (y != null)
            {

                var recapiti = db.XR_RECAPITI.Where(c => c.ID_PERSONA == y.ID_PERSONA).ToList();

                return recapiti;
            }
            else
                return null;





            
        }

        public static Cellulare GetCellulare(int matr) {
            Cellulare cell = new Cellulare();
            cell = null;
            DateTime fine = new DateTime(2999, 12, 31);

            List<XR_RECAPITI> recap = GetListaRecapiti(matr);

            if (recap != null && recap.Count()>0)
            {

                foreach (var r in recap)
                {

                   if((r.TIPO_RECAPITO.Equals("01") || r.TIPO_RECAPITO.Equals("02") || r.TIPO_RECAPITO.Equals("00")) && r.DTA_FINE.Equals(fine))
                    {   
                        cell = new Cellulare();
                        cell.prefisso = r.DES_PREFISSO;
                        cell.numero = r.DES_CELLULARE;
                        if (r.TIPO_RECAPITO.Equals("01"))
                        {
                            cell.tipologia = "Numero Personale";
                           
                        }
                        else if (r.TIPO_RECAPITO.Equals("02"))
                        {
                            cell.tipologia = "Numero Aziendale";
                        }
                        else
                        {
                            cell.tipologia = "Non dichiarata";
                        }
                    }

                }

            }
            else
            {
                cell = null;
            }



            return cell;
        }

        public static ProfiloPersonaleModel GetProfiloPersonaleModel()
        {

            myRai.Data.CurriculumVitae.cv_ModelEntities cvEnt = new myRai.Data.CurriculumVitae.cv_ModelEntities();
          
            myRaiServiceHub.it.rai.servizi.hrce.hrce_ws hrcews = new myRaiServiceHub.it.rai.servizi.hrce.hrce_ws();

            myRaiServiceHub.it.rai.servizi.hrce.retData retdata = new myRaiServiceHub.it.rai.servizi.hrce.retData();

            hrcews.Credentials = CommonHelper.GetUtenteServizioCredentials();

            List<string> matricole = new List<string>();
            string matricolaUtente = myRai.Business.CommonManager.GetCurrentUserMatricola();
            matricole.Add(matricolaUtente);
            string[] AppKeyhrce = CommonManager.GetParametri<string>(EnumParametriSistema.AppKeyhrce);

            Boolean usaServ = CommonManager.GetParametro<Boolean>(EnumParametriSistema.UsaServizioPerProfiloPersonale);
            if (usaServ)
                retdata = hrcews.getDatiUtente(AppKeyhrce[0], matricole.ToArray());
            else
                retdata.ds  = GetProfiloPersonaleFromDB(matricolaUtente);

            myRaiCommonModel.ProfiloPersonaleModel model = new ProfiloPersonaleModel();
            if (retdata.esito == 0)
            {
                if (retdata.ds != null)
                {
                    DataTable dr_conticorrente = retdata.ds.Tables["Table"];
                    model.ContiCorrente = new List<ContoCorrente>();

                    DataTable dr_recapiti = retdata.ds.Tables["Table1"];

                    if (dr_recapiti.Rows.Count > 0 && dr_recapiti.Rows[0]["MATRICOLA"].ToString().Trim() == matricolaUtente)
                    {

                        for (int i = 0; i < dr_conticorrente.Rows.Count; i++)
                        {
                            ContoCorrente contocorrente = new ContoCorrente();
                            contocorrente.Tipologia = dr_conticorrente.Rows[i]["Tipologia"].ToString();
                            contocorrente.Indirizzo_Agenzia = dr_conticorrente.Rows[i]["AG_Indirizzo"].ToString();
                            contocorrente.Citta_Agenzia = dr_conticorrente.Rows[i]["AG_Citta"].ToString();
                            contocorrente.Agenzia = dr_conticorrente.Rows[i]["Agenzia"].ToString();
                            contocorrente.Iban = dr_conticorrente.Rows[i]["Iban"].ToString();
                            contocorrente.Attivo_dal = dr_conticorrente.Rows[i]["Attivo_dal"].ToString();
                            model.ContiCorrente.Add(contocorrente);
                        }


                        for (int i = 0; i < dr_recapiti.Rows.Count; i++)
                        {
                            model.Residenza = new Residenza();
                            model.Residenza.Indirizzo = dr_recapiti.Rows[i]["Indirizzores"].ToString();
                            model.Residenza.prov = dr_recapiti.Rows[i]["provres"].ToString();
                            model.Residenza.citta = dr_recapiti.Rows[i]["cittares"].ToString();
                            model.Residenza.stato = dr_recapiti.Rows[i]["statores"].ToString();
                            model.Residenza.cap = dr_recapiti.Rows[i]["capres"].ToString();

                            model.Domicilio = new Domicilio();
                            model.Domicilio.Indirizzo = dr_recapiti.Rows[i]["Indirizzodom"].ToString();
                            model.Domicilio.prov = dr_recapiti.Rows[i]["provdom"].ToString();
                            model.Domicilio.citta = dr_recapiti.Rows[i]["cittadom"].ToString();
                            model.Domicilio.stato = dr_recapiti.Rows[i]["statodom"].ToString();
                            model.Domicilio.cap = dr_recapiti.Rows[i]["capdom"].ToString();
                        }
                    }
                }
               
                else
                {
                    string S = "";
                    ContoCorrente contocorrente = new ContoCorrente();
                    contocorrente.Tipologia = "-";
                    contocorrente.Indirizzo_Agenzia = "-";
                    contocorrente.Citta_Agenzia = "-";
                    contocorrente.Agenzia = "-";
                    contocorrente.Iban = "-";
                    contocorrente.Attivo_dal = "-";
                    if (model.ContiCorrente !=null)  model.ContiCorrente.Add(contocorrente);

                    model.Residenza = new Residenza();
                    model.Residenza.Indirizzo = "-";
                    model.Residenza.prov = "-";
                    model.Residenza.citta = "-";
                    model.Residenza.stato = "-";
                    model.Residenza.cap = "-";

                    model.Domicilio = new Domicilio();
                    model.Domicilio.Indirizzo = "-";
                    model.Domicilio.prov = "-";
                    model.Domicilio.citta = "-";
                    model.Domicilio.stato = "-";
                    model.Domicilio.cap = "-";
               }

            }

            model.RichiestaConvalidata = false;
            model.RichiestaDaConvalidareFound = true;
            model.CodiceNonValido = true;
            model.ListaOre = GetListaOre();
            model.ListaMinuti = GetListaminuti();
            model = GetImpostazioniNotificheDipendente(model);
            model.ImpostazioniL1_INSR = GetImpostazioneLivello1(EnumTipoEventoNotifica.INSR);

            if ((model.ImpostazioniL1_INSR.TipoInvio == "G" || model.ImpostazioniL1_INSR.TipoInvio == "S")
               && !String.IsNullOrWhiteSpace(model.ImpostazioniL1_INSR.OraMinuti)
               && model.ImpostazioniL1_INSR.OraMinuti.Length == 4)
            {
                model.OreL1Insr = model.ImpostazioniL1_INSR.OraMinuti.Substring(0, 2);
                model.MinL1Insr = model.ImpostazioniL1_INSR.OraMinuti.Substring(2, 2);
            }

            model.ImpostazioniL1_INSS = GetImpostazioneLivello1(EnumTipoEventoNotifica.INSS);

            if ((model.ImpostazioniL1_INSS.TipoInvio == "G" || model.ImpostazioniL1_INSS.TipoInvio == "S")
              && !String.IsNullOrWhiteSpace(model.ImpostazioniL1_INSS.OraMinuti)
              && model.ImpostazioniL1_INSS.OraMinuti.Length == 4)
            {
                model.OreL1Inss = model.ImpostazioniL1_INSS.OraMinuti.Substring(0, 2);
                model.MinL1Inss = model.ImpostazioniL1_INSS.OraMinuti.Substring(2, 2);
            }

            model.ImpostazioniL1_SCAD = GetImpostazioneLivello1(EnumTipoEventoNotifica.SCAD);

            if ((model.ImpostazioniL1_SCAD.TipoInvio == "G" || model.ImpostazioniL1_SCAD.TipoInvio == "S")
             && !String.IsNullOrWhiteSpace(model.ImpostazioniL1_SCAD.OraMinuti)
             && model.ImpostazioniL1_SCAD.OraMinuti.Length == 4)
            {
                model.OreL1Scad = model.ImpostazioniL1_SCAD.OraMinuti.Substring(0, 2);
                model.MinL1Scad = model.ImpostazioniL1_SCAD.OraMinuti.Substring(2, 2);
            }

            model.ImpostazioniL1_URG = GetImpostazioneLivello1(EnumTipoEventoNotifica.URG);

            if ((model.ImpostazioniL1_URG.TipoInvio == "G" || model.ImpostazioniL1_URG.TipoInvio == "S")
             && !String.IsNullOrWhiteSpace(model.ImpostazioniL1_URG.OraMinuti)
             && model.ImpostazioniL1_URG.OraMinuti.Length == 4)
            {
                model.OreL1Urg = model.ImpostazioniL1_URG.OraMinuti.Substring(0, 2);
                model.MinL1Urg = model.ImpostazioniL1_URG.OraMinuti.Substring(2, 2);
            }

            return model;
        }

        public static SelectList GetListaOre()
        {
            int[] ore = CommonManager.GetParametri<int>(EnumParametriSistema.NotificheRangeOre);

            List<SelectListItem> lista = new List<SelectListItem>();
            for (int h = ore[0]; h <= ore[1]; h++)
            {
                lista.Add(new SelectListItem()
                {
                    Value = h.ToString().PadLeft(2, '0'),
                    Text = h.ToString().PadLeft(2, '0')
                });
            }
            return new SelectList(lista, "Value", "Text");
        }

        public static SelectList GetListaminuti()
        {
            List<SelectListItem> lista = new List<SelectListItem>();
            for (int h = 0; h < 1; h++)
            {
                lista.Add(new SelectListItem()
                {
                    Value = h.ToString().PadLeft(2, '0'),
                    Text = h.ToString().PadLeft(2, '0')
                });
            }
            return new SelectList(lista, "Value", "Text");
        }

        public static string ProfiloUpdate(string ClickedObjectID, string day, string ore, string min)
        {
            if (ClickedObjectID.StartsWith("radio"))
            {
                EnumTipoDestinatarioNotifiche tipoDEST = EnumTipoDestinatarioNotifiche.ESS_DIP;
                if (ClickedObjectID.Contains("-dip-")) tipoDEST = EnumTipoDestinatarioNotifiche.ESS_DIP;
                if (ClickedObjectID.Contains("-l1-")) tipoDEST = EnumTipoDestinatarioNotifiche.ESS_LIV1;

                EnumTipoEventoNotifica tipoNOTIF = EnumTipoEventoNotifica.APPR;
                if (ClickedObjectID.Contains("-app-")) tipoNOTIF = EnumTipoEventoNotifica.APPR;
                if (ClickedObjectID.Contains("-rif-")) tipoNOTIF = EnumTipoEventoNotifica.RIF;
                if (ClickedObjectID.Contains("-scad-")) tipoNOTIF = EnumTipoEventoNotifica.SCAD;
                if (ClickedObjectID.Contains("-insr-")) tipoNOTIF = EnumTipoEventoNotifica.INSR;
                if (ClickedObjectID.Contains("-inss-")) tipoNOTIF = EnumTipoEventoNotifica.INSS;
                if (ClickedObjectID.Contains("-urg-")) tipoNOTIF = EnumTipoEventoNotifica.URG;

                EnumTipoInvioNotifiche tipoINVIO = EnumTipoInvioNotifiche.I;
                if (ClickedObjectID.EndsWith("i")) tipoINVIO = EnumTipoInvioNotifiche.I;
                if (ClickedObjectID.EndsWith("i-n")) tipoINVIO = EnumTipoInvioNotifiche.N;
                if (ClickedObjectID.EndsWith("g")) tipoINVIO = EnumTipoInvioNotifiche.G;
                if (ClickedObjectID.EndsWith("s")) tipoINVIO = EnumTipoInvioNotifiche.S;

                int? dayweek = null;
                string esito = ProfiloUpdateDB(
                    tipoDEST,
                    tipoNOTIF,
                    tipoINVIO,
                    ore + min,
                    String.IsNullOrWhiteSpace(day) ? dayweek : Convert.ToInt32(day));

            }
            return null;
        }

        public static string ProfiloUpdateDB(EnumTipoDestinatarioNotifiche dest,
            EnumTipoEventoNotifica tipoEvento,
            EnumTipoInvioNotifiche tipoInvio,
            string oremin, int? day
            )
        {
            var db = new digiGappEntities();
            string matr = CommonManager.GetCurrentUserMatricola();
            string destinatario = dest.ToString();
            string evento = tipoEvento.ToString();
            string invio = tipoInvio.ToString();

            var row = db.MyRai_InvioNotifiche.Where(x => x.Matricola == matr &&
                                                x.TipoDestinatario == destinatario &&
                                                x.TipoEvento == evento &&
                                                x.InvioAttivo == true).FirstOrDefault();
            if (row != null)
            {
                row.GiornoDellaSettimana = day;
                row.OraMinuti = oremin;
                row.TipoInvio = invio;
            }
            else
            {
                MyRai_InvioNotifiche r = new MyRai_InvioNotifiche()
                {
                    InvioAttivo = true,
                    Matricola = matr,
                    TipoDestinatario = destinatario,
                    TipoEvento = evento,
                    GiornoDellaSettimana = day,
                    OraMinuti = oremin,
                    TipoInvio = invio
                };
                db.MyRai_InvioNotifiche.Add(r);
            }
            if (DBHelper.Save(db, matr)) return null;
            else return "Errore DB";
        }

        public static string TemaUpdate(string NomeTema)
        {
            string[] datiServizio = CommonManager.GetParametri<string>(EnumParametriSistema.ServizioTema);
            if ( ! String.IsNullOrWhiteSpace(datiServizio[1]))
            {
				string url = datiServizio[1]
				   .Replace( "@MATRICOLA",
                       CommonManager.GetCurrentUserMatricola())
				   .Replace( "@TEMA",
					  ( "0," +
                       CommonManager.GetParametro<string>(EnumParametriSistema.ServizioTemaMapping))
					   .ToLower()
					   .Split( ',' )
					   .ToList()
					   .IndexOf( NomeTema.ToLower() )
					   .ToString() );
                try
                {
                    string[] utenteServizio = CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio);

                    WebClientApiRaiPlace client = new WebClientApiRaiPlace(datiServizio[0], url, utenteServizio[0], utenteServizio[1]);
                    WebClientApiRaiPlace.ApiResponse response = client.ExecuteCall();
                    if (response == null) throw new Exception("Response null");
                    if (response.esito == null || response.esito.ToLower().Trim() != "ok")
                        throw new Exception(response.code + "-" + response.data + "-" + response.message);
                }
                catch (Exception ex)
                {
                    Logger.LogErrori(new MyRai_LogErrori()
                    {
                        applicativo = "PORTALE",
                        data = DateTime.Now,
                        error_message = ex.ToString(),
                        matricola = CommonManager.GetCurrentUserMatricola(),
                        provenienza = "TemaUpdate"
                    });
                    return ex.Message;
                }
            }

            var db = new digiGappEntities();
            string matr = CommonManager.GetCurrentUserMatricola();
            MyRai_ParametriPersonali exist = db.MyRai_ParametriPersonali.Where(a => a.matricola == matr && a.nome_parametro == "Tema").FirstOrDefault();
            if (exist == null)
            {
                MyRai_ParametriPersonali t = new MyRai_ParametriPersonali()
                    {
                    matricola = myRai.Business.CommonManager.GetCurrentUserMatricola(),
                        nome_parametro = "TEMA",
                        valore_parametro = NomeTema
                    };

                db.MyRai_ParametriPersonali.Add(t);
            }
            else
            {
                exist.valore_parametro = NomeTema;
            }

            if (DBHelper.Save(db, matr)) return null;
            else return "Errore DB";
        }

        public static bool PrivacyAccepted(string matricola)
        {
            DateTime? dataNuovoCV = getDataNuovoCV();

            if (dataNuovoCV != null)
            {
                cv_ModelEntities cvEnt = new cv_ModelEntities();

                var tcvLoginEntity = cvEnt.TCVLogin.Where(
                    x => x.Matricola == matricola
                    ).FirstOrDefault();

                if (tcvLoginEntity != null)
                {
                    bool result = false;

                    if (tcvLoginEntity.DataOraAgg != null && DateTime.Compare(tcvLoginEntity.DataOraAgg.Value, dataNuovoCV.Value) > 0
                        && tcvLoginEntity.StatoLogin.Trim().Equals(""))
                    {
                        result = true;
                    }
                    return result;
                }
                else
                {
                    TCVLogin tcvLogin = new TCVLogin();
                    
                    string matr = matricola;
                    if (matricola != null && matricola.Length > 6) 
                        matr = matricola.Substring(matricola.Length - 6);

                    tcvLogin.Matricola = matr;
                    tcvLogin.PMatricola = string.Format("P{0}", matr);
                    tcvLogin.Pwd = "";
                    tcvLogin.StatoLogin = "W";
                    tcvLogin.TipoAgg = "I";
                    cvEnt.TCVLogin.Add(tcvLogin);

                    try
                    {
                        //cvEnt.SaveChanges();
                        DBHelper.Save(cvEnt, matricola);
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return false;
        }

        public static bool AcceptPrivacy(string matricola)
        {
            try
            {
                cv_ModelEntities cvEnt = new cv_ModelEntities();

                var tcvLoginEntity = cvEnt.TCVLogin.Where(
                    x => x.Matricola == matricola
                    ).FirstOrDefault();

                if (tcvLoginEntity != null)
                {
                    tcvLoginEntity.StatoLogin = "";
                    tcvLoginEntity.DataOraAgg = DateTime.Now;

                    //cvEnt.SaveChanges();
                    return DBHelper.Save(cvEnt, matricola);
                }
            }
            catch (Exception)
            {
            }

            return false;
        }

        public static DateTime? PrivacyAcceptedAt(string matricola)
        {
            DateTime? DataOraAgg = null;

            try
            {
                myRai.Data.CurriculumVitae.cv_ModelEntities cvEnt = new myRai.Data.CurriculumVitae.cv_ModelEntities();

                var tcvLoginEntity = cvEnt.TCVLogin.Where(
                    x => x.Matricola == matricola
                    ).FirstOrDefault();

                if (tcvLoginEntity != null)
                {
                    DataOraAgg = tcvLoginEntity.DataOraAgg;
                }
            }
            catch (Exception)
            {
            }

            return DataOraAgg;
        }

        public static bool UpdateTCVLoginDataOraAgg(string matricola)
        {
            try
            {
                cv_ModelEntities cvEnt = new cv_ModelEntities();

                var tcvLoginEntity = cvEnt.TCVLogin.Where(
                    x => x.Matricola == matricola
                    ).FirstOrDefault();

                if (tcvLoginEntity != null)
                {
                    tcvLoginEntity.DataOraAgg = DateTime.Now;

                    //cvEnt.SaveChanges();
                    return DBHelper.Save(cvEnt, matricola);
                }
            }
            catch (Exception)
            {
            }

            return false;
        }

        public static DateTime? getDataNuovoCV()
        {
            DateTime? data_nuovo_cv = null;

            var db = new digiGappEntities();
            string matr = CommonManager.GetCurrentUserMatricola();

            string DATA_NUOVO_CV = CommonHelper.GetAppSettings("DATA_NUOVO_CV");

            if (!string.IsNullOrEmpty(DATA_NUOVO_CV))
            {
                MyRai_ParametriSistema exist = db.MyRai_ParametriSistema.Where(
                    a => a.Chiave == DATA_NUOVO_CV).FirstOrDefault();
                if (exist != null)
                {
                    try
                    {
                        data_nuovo_cv = DateTime.ParseExact(
                            exist.Valore1,
                            "dd/MM/yyyy HH:mm:ss",
                            new System.Globalization.CultureInfo("it-IT")
                            );

                        return data_nuovo_cv;
                    }
                    catch (Exception)
                    {
                        //TODO
                    }
                }
                else
                {
                    DateTime now = DateTime.Now;

                    MyRai_ParametriSistema mp = new MyRai_ParametriSistema()
                    {
                        Chiave = DATA_NUOVO_CV,
                        Valore1 = now.ToString("dd/MM/yyyy HH:mm:ss")
                    };


                    db.MyRai_ParametriSistema.Add(mp);

                    try
                    {
                        if (DBHelper.Save(db, matr))
                            data_nuovo_cv = now;
                    }
                    catch (Exception)
                    { }
                }
            }
            return data_nuovo_cv;
        }

    }

    
}