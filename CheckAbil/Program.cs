using CheckAbil.it.rai.servizi.raiconnectcoll;
using ClosedXML.Excel;
using myRaiData.Incentivi;
using myRaiDataTalentia;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel.Description;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CheckAbil
{
    class Abilitati
    {
        public Abilitati()
        {
            Direzioni = new List<AbilLevel>();
        }
        public string Matricola { get; set; }
        public string Cognome { get; set; }
        public string Nome { get; set; }
        public string SottoFunzioni { get; set; }
        public string TipologiaPersonale { get; set; }
        public string CodSottofunzion { get; set; }
        public string CategorieIncluse { get; set; }
        public string CategorieEscluse { get; set; }
        public string SedeServizio { get; set; }
        public string Servizio { get; set; }
        public List<AbilLevel> Direzioni { get; set; }
    }
    class AbilLevel
    {
        public AbilLevel()
        {
            Direzioni = new List<Tuple<string, string>>();
        }
        public string Funzione { get; set; }
        public bool AllDir { get; set; }
        public List<Tuple<string, string>> Direzioni { get; set; }
    }

    [Serializable]
    public class filtri_ricerca
    {
        public string identificativo { get; set; }
        public string oggetto { get; set; }
        public string mittente { get; set; }
        public string destinatario { get; set; }
        public string data_protocollo_dal { get; set; }
        public string data_protocollo_al { get; set; }
        public string matricola { get; set; }
    }

    [Serializable]
    public class destClass
    {
        public string destinatario { get; set; }
    }
    [Serializable]
    public class protocollo
    {
        public protocollo()
        {
            tipo_protocollo = 2;
        }
        public int tipo_protocollo { get; set; }
        public string oggetto { get; set; }
        public string mittente { get; set; }

        [XmlElement("destinatari", typeof(destClass))]
        public List<destClass> destinatari { get; set; }
        public string data_spedizione { get; set; }
    }

    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }

    class Program
    {
        static void Main(string[] args)
        {
            //string directory = @"\\ZTOCERV948\c$\FRONT_END\CM\ANA";
            //var tmpfiles =  Directory.GetFiles(directory);


            ////URL:  http://raiconnectcoll.servizi.rai.it/rai_ruo_ws.asmx
            //string user = "srv_raiconnect_ruo";
            //string pwd = "6E6asOXO";

            //rai_ruo_ws client = new rai_ruo_ws();
            //client.Credentials = new System.Net.NetworkCredential(user, pwd);
            ////client.UseDefaultCredentials = true;

            //#region CreaProtocollo
            //protocollo prot = new protocollo();
            //prot.mittente = "103650";
            //prot.destinatari = new List<destClass>()
            //{
            //    new destClass(){destinatario="103650"}
            //};
            //prot.oggetto = "RUO - Protocollo di test";
            //prot.data_spedizione = "2020-11-01";
            //string strProt = SerializeObject(prot);
            //var tmpProt = client.creaProtocollo("42936", user, strProt);
            //#endregion

            //#region Ricerca
            //filtri_ricerca filtri = new filtri_ricerca();
            //string strFiltri = SerializeObject(filtri);
            //var tmp = client.eseguiRicerca("42936", "P103650", "1500", "1", strFiltri);
            //#endregion


            //string funzione = "INCENTIVI";
            //ElencoAbil("INCENTIVI");
            //ElencoAbilPRetrib();
            ElencoAbil("HRCE");

            //CheckModelHRCE_DEMA();
            //CheckModelHRIS_SW();
            //CheckModelINC_EXTRA();

            //LoadAbil_NewVersion();
        }

        public static void LoadAbil_NewVersion()
        {
            var db = new IncentiviEntities();
            string percorso = @"C:\Users\Nik\Desktop\HRIS su HRGA - Copia.xlsx";
            var wb = new XLWorkbook(percorso);
            var ws = wb.Worksheet("Elenco funzioni");

            int row = 1;
            string codFunzione = "";
            string codSub = "";
            string desSub = "";

            Dictionary<string, int> dictFunz = db.XR_HRIS_ABIL_FUNZIONE.ToDictionary(x => x.COD_FUNZIONE, x => x.ID_FUNZIONE);

            while (!String.IsNullOrWhiteSpace(codFunzione = ws.Cell(++row, 1).GetValue<string>()))
            {
                if (ws.Cell(row, "I").GetValue<string>() != "x")
                    continue;

                codSub = ws.Cell(row, 5).GetString();// Value<string>();
                desSub = ws.Cell(row, 8).GetValue<string>();

                if (!db.XR_HRIS_ABIL_SUBFUNZIONE.Any(x => x.COD_SUBFUNZIONE == codSub && x.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == codFunzione))
                {
                    XR_HRIS_ABIL_SUBFUNZIONE subFunz = new XR_HRIS_ABIL_SUBFUNZIONE()
                    {
                        ID_FUNZIONE = dictFunz[codFunzione],
                        COD_SUBFUNZIONE = codSub,
                        DES_SUBFUNZIONE = desSub,
                        IND_ATTIVO = true,
                        IND_CREATE = !codSub.EndsWith("VIS"),
                        IND_READ = true,
                        IND_DELETE = !codSub.EndsWith("VIS"),
                        IND_UPDATE = !codSub.EndsWith("VIS")
                    };
                    db.XR_HRIS_ABIL_SUBFUNZIONE.Add(subFunz);
                    db.SaveChanges();
                }
            }

            var profili = db.XR_HRIS_ABIL_PROFILO.ToList();
            var listSubFunz = db.XR_HRIS_ABIL_SUBFUNZIONE.Where(x => x.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE.StartsWith("HRIS_")).ToDictionary(x => x.COD_SUBFUNZIONE, x => x);
            Dictionary<string, string[]> mapOldFunc = new Dictionary<string, string[]>();
            mapOldFunc.Add("AGG_ANAG", new string[] { "ANAGES", "RESGES", "DOMGES", "STUGES", "RECGES" });
            mapOldFunc.Add("APPRENDISTATO", new string[] { "PFIGES" });
            mapOldFunc.Add("DATI_BANCARI", new string[] { "BNKGES" });
            mapOldFunc.Add("EVID_ANAG", new string[] { "ANAVIS", "RESVIS", "DOMVIS", "STUVIS", "RECVIS" });
            mapOldFunc.Add("IMMA", new string[] { "IMMGES" });
            //mapOldFunc.Add("INCARICHI_FSUPER", "");//?
            mapOldFunc.Add("RESIDENZA_AC", new string[] { "RESADM" });
            //mapOldFunc.Add("AGG_INQUADRAMENTO", "");

            var elemToAdd = new List<XR_HRIS_ABIL_PROFILO_ASSOCIAZIONI>();

            foreach (var profilo in profili)
            {
                foreach (var assoc in profilo.XR_HRIS_ABIL_PROFILO_ASSOCIAZIONI.Where(x => x.ID_SUBFUNZ != null).Select(x => x.XR_HRIS_ABIL_SUBFUNZIONE))
                {
                    if (mapOldFunc.TryGetValue(assoc.COD_SUBFUNZIONE, out var vals))
                    {
                        foreach (var val in vals)
                        {
                            var subFunzId = listSubFunz[val].ID_SUBFUNZ;
                            elemToAdd.Add(new XR_HRIS_ABIL_PROFILO_ASSOCIAZIONI()
                            {
                                ID_PROFILO = profilo.ID_PROFILO,
                                ID_SUBFUNZ = subFunzId,
                                IND_ATTIVO = true
                            });
                        }
                    }
                }
            }

            foreach (var item in elemToAdd)
            {
                db.XR_HRIS_ABIL_PROFILO_ASSOCIAZIONI.Add(item);
            }
            db.SaveChanges();
        }

        public static void CheckModelINC_EXTRA()
        {
            /*   var db = new TalentiaEntities();
               var elencoAbil = db.XR_HRIS_ABIL.Where(x => x.XR_HRIS_ABIL_SUBFUNZIONE.COD_SUBFUNZIONE == "CONTENZIOSO" && x.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == "INC_EXTRA").ToList();
               foreach (var item in elencoAbil)
               {
                   item.TIP_INCLUSE = "1";
                   db.SaveChanges();
               }
               */
        }

        public static void CheckModelHRIS_SW()
        {
            var db = new TalentiaEntities();


        }

        public static void CheckModelHRCE_DEMA()
        {
            var db = new TalentiaEntities();
            //HrgaIntranet.Sedi sedi = new HrgaIntranet.Sedi();

            //List<string> lines = new List<string>();

            //string[] modelli = new string[] { "M_DIR_AREA_STAFF", "M_DIR_AREA_PRODUZIONE", "M_DIR_AREA_EDITORIALE" };
            //foreach (var item in modelli)
            //{
            //    var modello = db.XR_HRIS_ABIL_MODELLO.FirstOrDefault(x => x.CODICE == item);
            //    var listMatr = modello.XR_HRIS_ABIL_ASSOC_MODELLO.Select(x => x.XR_HRIS_ABIL.MATRICOLA);

            //    foreach (var matr in listMatr)
            //    {
            //        var abils = db.XR_HRIS_ABIL.Where(x => x.MATRICOLA == matr && x.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == "DEMA");
            //        if (abils!=null && abils.Any())
            //        {
            //            foreach (var abil in abils)
            //            {
            //                abil.XR_HRIS_ABIL_ASSOC_MODELLO.Add(new XR_HRIS_ABIL_ASSOC_MODELLO()
            //                {
            //                    ID_MODELLO = modello.ID_MODELLO
            //                });
            //            }
            //        }
            //    }
            //}
            //db.SaveChanges();

            //HrgaIntranet.Sedi sedi = new HrgaIntranet.Sedi();
            //var abilDema = db.XR_HRIS_ABIL.Where(x => x.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == "DEMA");
            //foreach (var item in abilDema.GroupBy(x=>x.MATRICOLA))
            //{
            //    var response = sedi.Get_LivelliAccesso_Net("P" + item.Key, "HRCE", "");
            //    if (response!=null && response.DT_LivelliAccesso!=null)
            //    {
            //        var grLivAcc = String.Join(",", response.DT_LivelliAccesso.AsEnumerable().Select(x => x.Field<string>("Cod").Trim()).Distinct());
            //        foreach (var abil in item.Where(x=>x.XR_HRIS_ABIL_SUBFUNZIONE.COD_SUBFUNZIONE!="ADM"))
            //        {
            //            abil.GR_CATEGORIE = grLivAcc;
            //        }
            //    }
            //}
            //db.SaveChanges();

            //var qualFilter = db.XR_HRIS_QUAL_FILTER.ToList();
            //var abilDema = db.XR_HRIS_ABIL.Where(x => x.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == "DEMA" && x.GR_CATEGORIE!=null && x.GR_CATEGORIE!="");
            //foreach (var item in abilDema)
            //{
            //    string[] filters = item.GR_CATEGORIE.Split(',');
            //    string idFilter = String.Join(",", qualFilter.Where(x => filters.Contains(x.COD_QUAL_FILTER)).Select(x => x.ID_QUAL_FILTER.ToString()));
            //    item.GR_CATEGORIE = idFilter;
            //}
            //db.SaveChanges();

            /*Passaggio filtri categorie da filtro a modello*/
            //var elencoAbilDema = db.XR_HRIS_ABIL.Where(x => x.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == "DEMA" && x.GR_CATEGORIE!=null).ToList();
            //var modelli = db.XR_HRIS_ABIL_MODELLO.Where(x=>x.GR_CATEGORIE!=null).ToList();

            //foreach (var item in elencoAbilDema)
            //{
            //    //Sposto il filtro per categoria dalla sottofunzione al modello
            //    foreach (var cat in item.GR_CATEGORIE.Split(','))
            //    {
            //        var modello = modelli.FirstOrDefault(x => x.GR_CATEGORIE.Contains(cat));
            //        if (!item.XR_HRIS_ABIL_ASSOC_MODELLO.Any(x => x.ID_MODELLO == modello.ID_MODELLO))
            //            item.XR_HRIS_ABIL_ASSOC_MODELLO.Add(new XR_HRIS_ABIL_ASSOC_MODELLO()
            //            {
            //                ID_MODELLO = modello.ID_MODELLO
            //            });
            //    }
            //    item.GR_CATEGORIE = null;

            //    db.SaveChanges();
            //}

            /*Passaggio matricole da O1ADM a 01APPR*/
            //var elencoAbilDema = db.XR_HRIS_ABIL.Where(x => x.XR_HRIS_ABIL_SUBFUNZIONE.COD_SUBFUNZIONE == "01ADM" && x.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == "DEMA" && x.MATR_INCLUSE != null).ToList();
            //var subFuncAppr = db.XR_HRIS_ABIL_SUBFUNZIONE.FirstOrDefault(x => x.COD_SUBFUNZIONE == "01APPR" && x.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == "DEMA");

            //foreach (var item in elencoAbilDema)
            //{
            //    //verifico esistenza abilitazione
            //    var abilAppr = db.XR_HRIS_ABIL.FirstOrDefault(x => x.MATRICOLA == item.MATRICOLA && x.ID_SUBFUNZ == subFuncAppr.ID_SUBFUNZ);
            //    if (abilAppr==null)
            //    {
            //        abilAppr = new XR_HRIS_ABIL()
            //        {
            //            MATRICOLA = item.MATRICOLA,
            //            IND_ATTIVO = true,
            //            ID_SUBFUNZ = subFuncAppr.ID_SUBFUNZ,
            //            MATR_INCLUSE = item.MATR_INCLUSE,
            //        };
            //        db.XR_HRIS_ABIL.Add(abilAppr);
            //        db.SaveChanges();
            //    }
            //}

            //var elencoAbilDema = db.XR_HRIS_ABIL.Where(x => x.XR_HRIS_ABIL_SUBFUNZIONE.COD_SUBFUNZIONE == "01ADM" && x.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == "DEMA" && x.MATR_INCLUSE != null).ToList();
            //foreach (var item in elencoAbilDema)
            //{
            //    item.MATR_INCLUSE = null;
            //    db.SaveChanges();
            //}

            //var elencoAbilDema = db.XR_HRIS_ABIL.Where(x => x.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == "DEMA" && x.XR_HRIS_ABIL_ASSOC_MODELLO.Any()).ToList();
            //foreach (var item in elencoAbilDema)
            //{
            //    if (!String.IsNullOrWhiteSpace(item.GR_AREA) || !String.IsNullOrWhiteSpace(item.GR_CATEGORIE))
            //    {
            //        continue;
            //    }

            //    string abilGrCat = "";
            //    string abilGrDir = "";

            //    foreach (var assoc in item.XR_HRIS_ABIL_ASSOC_MODELLO)
            //    {
            //        if (!String.IsNullOrWhiteSpace(assoc.XR_HRIS_ABIL_MODELLO.GR_AREA))
            //            abilGrDir += "," + assoc.XR_HRIS_ABIL_MODELLO.GR_AREA;
            //        if (!String.IsNullOrWhiteSpace(assoc.XR_HRIS_ABIL_MODELLO.GR_CATEGORIE))
            //            abilGrCat += "," + assoc.XR_HRIS_ABIL_MODELLO.GR_CATEGORIE;
            //    }

            //    string tmpCat = String.Join(",", abilGrCat.Split(',').Where(x => !String.IsNullOrWhiteSpace(x)));
            //    item.GR_CATEGORIE = tmpCat;

            //    string tmpDir = String.Join(",", abilGrDir.Split(',').Where(x => !String.IsNullOrWhiteSpace(x)));
            //    item.GR_AREA = tmpDir;

            //    var itemAss = item.XR_HRIS_ABIL_ASSOC_MODELLO.ToList();
            //    foreach (var itemAssoc in itemAss)
            //    {
            //        db.XR_HRIS_ABIL_ASSOC_MODELLO.Remove(itemAssoc);
            //    }

            //    db.SaveChanges();
            //}

            //var elencoAbilDema = db.XR_HRIS_ABIL.Where(x => x.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == "DEMA" && x.XR_HRIS_ABIL_ASSOC_MODELLO.Any()).ToList();
            //foreach (var item in elencoAbilDema)
            //{

            //}
        }

        private static string SerializeObject<T>(T oggetto) where T : class
        {
            string result;
            var emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            var serializer = new XmlSerializer(oggetto.GetType());
            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            using (Utf8StringWriter sw = new Utf8StringWriter())
            {
                using (var wri = XmlWriter.Create(sw, settings))
                {
                    serializer.Serialize(wri, oggetto, emptyNamespaces);
                    result = sw.ToString();
                }
            }

            return result;
        }

        static void ElencoAbil(string funzione)
        {
            XLWorkbook workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Elenco abilitazioni");

            int row = 1;
            worksheet.Cell(row, 1).SetValue("Matricola");
            worksheet.Cell(row, 2).SetValue("Cognome");
            worksheet.Cell(row, 3).SetValue("Nome");
            worksheet.Cell(row, 4).SetValue("Sottofunzioni");
            worksheet.Cell(row, 5).SetValue("Categorie incluse");
            worksheet.Cell(row, 6).SetValue("Categorie escluse");
            worksheet.Cell(row, 7).SetValue("Servizio");

            HrgaIntranet.Sedi sediIntranet = new HrgaIntranet.Sedi();
            HrgaServizi.Sedi sediServizi = new HrgaServizi.Sedi();

            HrgaIntranet.UtentiAssociati resp = sediIntranet.Get_UtentiAssociati_Funzioni_Net(funzione, "");
            if (resp != null && resp.DT_UtentiAssociati != null)
            {
                foreach (DataRow utente in resp.DT_UtentiAssociati.Rows)
                {
                    row++;
                    string pMatricola = utente.Field<string>("Logon_id");
                    worksheet.Cell(row, 1).SetValue(pMatricola);
                    worksheet.Cell(row, 2).SetValue(utente.Field<string>("Cognome"));
                    worksheet.Cell(row, 3).SetValue(utente.Field<string>("Nome"));

                    string cognome = utente.Field<string>("Cognome");

                    //HrgaIntranet.AutorizzazioniResponse respAuth = sediIntranet.Autorizzazioni_Net(pMatricola, funzione, "", "", "", "", "");
                    //if (respAuth != null && respAuth.DT_SottofunzioniAssociate != null)
                    //{
                    //    string abils = "";
                    //    foreach (DataRow sottofunzione in respAuth.DT_SottofunzioniAssociate.Rows)
                    //    {
                    //        abils += sottofunzione.Field<string>("COD") + "|";
                    //    }
                    //    worksheet.Cell(row, 4).SetValue(abils);
                    //}

                    HrgaServizi.LivelliAccesso respCat = sediServizi.Get_LivelliAccesso_Net(pMatricola, funzione, "");
                    if (respCat != null && respCat.DT_LivelliAccesso != null)
                    {
                        EnumerableRowCollection<Tuple<string, string>> tuple = respCat.DT_LivelliAccesso.AsEnumerable().Select(x => new Tuple<string, string>(x.Field<string>("Categoria_Ammessa"), x.Field<string>("Categoria_Esclusa")));

                        string desCatDato = String.Join(",",respCat.DT_LivelliAccesso.AsEnumerable().Select(x => x.Field<string>("DESCRIZIONE_CATEGORIA_DATO")).Distinct());
                        worksheet.Cell(row, 8).SetValue(desCatDato);

                        IEnumerable<string> listIncluded = tuple.Where(x => !String.IsNullOrWhiteSpace(x.Item1)).SelectMany(x => x.Item1.Split(',')).Distinct();
                        IEnumerable<string> listExcluded = tuple.Where(x => !String.IsNullOrWhiteSpace(x.Item2)).SelectMany(x => x.Item2.Split(',')).Distinct().Where(y => !listIncluded.Contains(y));

                        worksheet.Cell(row, 5).SetValue(String.Join(",", listIncluded));
                        worksheet.Cell(row, 6).SetValue(String.Join(",", listExcluded));
                    }

                    //string abil = "";
                    //string respServ = sediServizi.Get_Servizio(pMatricola, funzione);
                    //if (!String.IsNullOrWhiteSpace(respServ))
                    //{
                    //    abil = respServ;
                    //}

                    //var respSedeServ = sediServizi.Get_CategoriaDato_Elenco_Net("sedeserv", pMatricola, funzione, "");
                    //if (respSedeServ != null && respSedeServ.Cod_Errore == "0" && respSedeServ.DT_Utenti_CategorieDatoAbilitate != null)
                    //{
                    //    if (respSedeServ.DT_Utenti_CategorieDatoAbilitate.AsEnumerable().Any(x => x["Cod"].ToString() == "TUTTO"))
                    //    {
                    //        abil += "TUTTO";
                    //    }
                    //    else if (respSedeServ.DT_Utenti_CategorieDatoAbilitate.AsEnumerable().Any())
                    //    {
                    //        abil += String.Join("|", respSedeServ.DT_Utenti_CategorieDatoAbilitate.AsEnumerable().Select(x => x["Cod"].ToString()));
                    //    }
                    //    else
                    //    {
                    //        var respsede = sediServizi.Get_CategoriaDato_Elenco_Net("sede", pMatricola, funzione, "");
                    //        if (respsede != null && respsede.Cod_Errore == "0" && respsede.DT_Utenti_CategorieDatoAbilitate != null)
                    //        {
                    //            abil += String.Join("|", respsede.DT_Utenti_CategorieDatoAbilitate.AsEnumerable().Select(x => x["Cod"].ToString()));

                    //        }
                    //    }
                    //}
                    //worksheet.Cell(row, 7).SetValue(abil);

                    //var respSedeServ = sediServizi.Get_CategoriaDato_Elenco_Net("sedeserv", pMatricola, funzione, "");
                    //if (respSedeServ != null && respSedeServ.Cod_Errore == "0" && respSedeServ.DT_CategorieDatoAbilitate != null)
                    //{
                    //    bool hasAll = false;
                    //    foreach (DataRow item in respSedeServ.DT_CategorieDatoAbilitate.Rows)
                    //    {
                    //        if (item.Field<string>("COD") == "TUTTO")
                    //        {
                    //            hasAll = true;
                    //            break;
                    //        }
                    //    }

                    //    if (hasAll)
                    //    {
                    //        worksheet.Cell(row, 7).SetValue("TUTTO");
                    //    }
                    //    else
                    //    {
                    //        string respServ = sediServizi.Get_Servizio(pMatricola, funzione);
                    //        worksheet.Cell(row, 7).SetValue(respServ);
                    //    }
                    //}
                }
            }
            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(@"c:\tmp\Elenco Abil " + funzione + ".xlsx");
            Process.Start(@"c:\tmp\Elenco Abil " + funzione + ".xlsx");
        }

        static void ElencoAbilPRetrib()
        {
            string funzione = "P_RETRIB";

            List<XR_PRV_DIREZIONE> dbDir = null;

            bool parlante = false;
            if (parlante)
            {
                using (var db = new IncentiviEntities())
                    dbDir = db.XR_PRV_DIREZIONE.Include("XR_PRV_AREA").ToList();
            }

            List<Tuple<string, string>> applicazione = new List<Tuple<string, string>>();
            applicazione.Add(new Tuple<string, string>("ADM", "Totale"));
            applicazione.Add(new Tuple<string, string>("RICHIESTE,GEST_RICH,VIS_RICH", "Richieste"));
            applicazione.Add(new Tuple<string, string>("BDGQIO,BDGRS", "Budget"));
            applicazione.Add(new Tuple<string, string>("LETTERE,VIS_LETT", "Lettere"));
            applicazione.Add(new Tuple<string, string>("VIS_AMM,GEST_AMM", "Amministrazione"));

            List<Tuple<string, string>> tipologiaDip = new List<Tuple<string, string>>();
            tipologiaDip.Add(new Tuple<string, string>("BDGQIO,GEST_QIO", "Quadri, impiegati e operai"));
            tipologiaDip.Add(new Tuple<string, string>("BDGRS,GEST_RS", "Risorse chiave"));
            tipologiaDip.Add(new Tuple<string, string>("BDGQIO,GEST_QIO", "Quadri, impiegati e operai"));
            tipologiaDip.Add(new Tuple<string, string>("VIS_QIO", "Quadri, impiegati e operai (visualizzazione)"));
            tipologiaDip.Add(new Tuple<string, string>("VIS_RS", "Risorse chiave (visualizzazione)"));

            XLWorkbook workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Elenco abilitazioni");
            var worksheetDir = workbook.Worksheets.Add("Elenco direzioni");

            int row = 1;
            worksheet.Cell(row, 1).SetValue("Matricola");
            worksheet.Cell(row, 2).SetValue("Cognome");
            worksheet.Cell(row, 3).SetValue("Nome");
            if (parlante)
                worksheet.Cell(row, 4).SetValue("Funzionalità");
            else
                worksheet.Cell(row, 4).SetValue("Sottofunzioni");

            if (parlante)
            {
                worksheet.Cell(row, 5).SetValue("Tipologia personale");
                worksheet.Cell(row, 6).SetValue("Aree");
            }
            else
            {
                worksheet.Cell(row, 5).SetValue("Categorie incluse");
                worksheet.Cell(row, 6).SetValue("Categorie escluse");

            }

            worksheetDir.Cell(row, 1).SetValue("Matricola");
            worksheetDir.Cell(row, 2).SetValue("Cognome");
            worksheetDir.Cell(row, 3).SetValue("Nome");
            if (parlante)
            {
                worksheetDir.Cell(row, 4).SetValue("Direzioni");
            }
            else
            {
                worksheetDir.Cell(row, 4).SetValue("SedeServizio");
                worksheetDir.Cell(row, 5).SetValue("Servizio");
            }


            HrgaIntranet.Sedi sediIntranet = new HrgaIntranet.Sedi();
            HrgaServizi.Sedi sediServizi = new HrgaServizi.Sedi();

            HrgaIntranet.UtentiAssociati resp = sediIntranet.Get_UtentiAssociati_Funzioni_Net(funzione, "");
            if (resp != null && resp.DT_UtentiAssociati != null)
            {
                List<Abilitati> writers = new List<Abilitati>();

                foreach (DataRow utente in resp.DT_UtentiAssociati.Rows)
                {
                    string pMatricola = utente.Field<string>("Logon_id");

                    Abilitati writer = new Abilitati();
                    writer.Matricola = pMatricola;
                    writer.Cognome = utente.Field<string>("Cognome");
                    writer.Nome = utente.Field<string>("Nome");

                    HrgaIntranet.AutorizzazioniResponse respAuth = sediIntranet.Autorizzazioni_Net(pMatricola, funzione, "", "", "", "", "");
                    string abils = "";
                    if (respAuth != null && respAuth.DT_SottofunzioniAssociate != null)
                    {
                        List<Tuple<string, string>> subApp = new List<Tuple<string, string>>();
                        List<Tuple<string, string>> subType = new List<Tuple<string, string>>();

                        foreach (DataRow sottofunzione in respAuth.DT_SottofunzioniAssociate.Rows)
                        {
                            if (!String.IsNullOrWhiteSpace(abils)) abils += "|";
                            string cod = sottofunzione.Field<string>("COD");
                            abils += cod;

                            subApp.AddRange(applicazione.Where(x => x.Item1.Contains(cod)));
                            subType.AddRange(tipologiaDip.Where(x => x.Item1.Contains(cod)));
                        }

                        if (parlante)
                        {
                            if (subApp.Any(x => x.Item1 == "ADM"))
                            {
                                writer.SottoFunzioni = "Visibilità totale";
                            }
                            else
                            {
                                writer.SottoFunzioni = String.Join("\r\n", subApp.Select(x => x.Item2).Distinct());
                                writer.TipologiaPersonale = String.Join("\r\n", subType.Select(x => x.Item2).Distinct());
                            }

                            writer.CodSottofunzion = abils;
                        }
                        else
                            writer.SottoFunzioni = abils;

                        if (!parlante)
                        {
                            HrgaServizi.LivelliAccesso respCat = sediServizi.Get_LivelliAccesso_Net(pMatricola, funzione, "");
                            if (respCat != null && respCat.DT_LivelliAccesso != null)
                            {
                                EnumerableRowCollection<Tuple<string, string>> tuple = respCat.DT_LivelliAccesso.AsEnumerable().Select(x => new Tuple<string, string>(x.Field<string>("Categoria_Ammessa"), x.Field<string>("Categoria_Esclusa")));

                                IEnumerable<string> listIncluded = tuple.Where(x => !String.IsNullOrWhiteSpace(x.Item1)).SelectMany(x => x.Item1.Split(',')).Distinct();
                                IEnumerable<string> listExcluded = tuple.Where(x => !String.IsNullOrWhiteSpace(x.Item2)).SelectMany(x => x.Item2.Split(',')).Distinct().Where(y => !listIncluded.Contains(y));

                                writer.CategorieIncluse = String.Join(",", listIncluded);
                                writer.CategorieEscluse = String.Join(",", listExcluded);
                            }
                        }

                        string sedeServ = "";
                        var respSedeServ = sediServizi.Get_CategoriaDato_Elenco_Net("sedeserv", pMatricola, funzione, abils);
                        if (respSedeServ != null && respSedeServ.Cod_Errore == "0" && respSedeServ.DT_CategorieDatoAbilitate != null)
                        {
                            var tmp = respSedeServ.DT_Utenti_CategorieDatoAbilitate.AsEnumerable()
                                .Select(x => new { SubFunc = x["Codice_sottofunzione"].ToString(), Cod = x["Cod"].ToString() });

                            foreach (var item in tmp)
                            {
                                if (!String.IsNullOrWhiteSpace(item.SubFunc) && abils.Contains(item.SubFunc))
                                {
                                    if (item.Cod == "TUTTO")
                                    {
                                        if (!String.IsNullOrWhiteSpace(sedeServ)) sedeServ += "\r\n";
                                        sedeServ += item.SubFunc + ": TUTTO";
                                        writer.Direzioni.Add(new AbilLevel() { Funzione = item.SubFunc, AllDir = true });
                                    }
                                }
                            }
                        }

                        writer.SedeServizio = sedeServ;

                        string serv = "";
                        var respServ = sediServizi.Get_CategoriaDato_Elenco_Net("serv", pMatricola, funzione, abils);
                        if (respServ != null && respServ.Cod_Errore == "0" && respServ.DT_Utenti_CategorieDatoAbilitate != null)
                        {
                            var tmp = respServ.DT_Utenti_CategorieDatoAbilitate.AsEnumerable()
                                .Select(x => new { SubFunc = x["Codice_sottofunzione"].ToString(), Cod = x["Cod"].ToString(), Des = x["DESCRIZIONE_CATEGORIA_DATO"].ToString() })
                                .GroupBy(y => y.SubFunc);

                            foreach (var item in tmp)
                            {
                                if (!String.IsNullOrWhiteSpace(item.Key) && abils.Contains(item.Key) && !writer.Direzioni.Any(x => x.Funzione == item.Key))
                                {
                                    if (!String.IsNullOrWhiteSpace(serv)) serv += "\r\n";
                                    serv += item.Key + ": ";

                                    AbilLevel abilLevel = new AbilLevel();
                                    abilLevel.Funzione = item.Key;
                                    foreach (var dir in item)
                                    {
                                        serv += dir.Cod + " - " + dir.Des + "|";
                                        abilLevel.Direzioni.Add(new Tuple<string, string>(dir.Cod, dir.Des));
                                    }
                                    writer.Direzioni.Add(abilLevel);
                                }
                            }
                        }

                        writer.Servizio = serv;

                        writers.Add(writer);
                    }
                }

                foreach (var writer in writers.OrderBy(x => x.Matricola))
                {
                    if (parlante)
                    {
                        if (writer.CodSottofunzion.Contains("ADM"))
                            continue;
                    }

                    row++;
                    worksheet.Cell(row, 1).SetValue(writer.Matricola);
                    worksheet.Cell(row, 2).SetValue(writer.Cognome);
                    worksheet.Cell(row, 3).SetValue(writer.Nome);
                    worksheetDir.Cell(row, 1).SetValue(writer.Matricola);
                    worksheetDir.Cell(row, 2).SetValue(writer.Cognome);
                    worksheetDir.Cell(row, 3).SetValue(writer.Nome);

                    worksheet.Cell(row, 4).SetValue(writer.SottoFunzioni);
                    worksheet.Cell(row, 4).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

                    if (!parlante)
                    {
                        worksheet.Cell(row, 5).SetValue(writer.CategorieIncluse);
                        worksheet.Cell(row, 6).SetValue(writer.CategorieEscluse);
                        worksheetDir.Cell(row, 4).SetValue(writer.SedeServizio);
                        worksheetDir.Cell(row, 5).SetValue(writer.Servizio);
                    }
                    else
                    {
                        worksheet.Cell(row, 5).SetValue(writer.TipologiaPersonale);
                        worksheet.Cell(row, 5).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

                        List<string> aree = new List<string>();

                        if (writer.SottoFunzioni.ToLower().Contains("totale"))
                        {
                            worksheet.Cell(row, 5).SetValue("Tutte");
                            worksheetDir.Cell(row, 4).SetValue("Tutte");
                        }
                        else
                        {
                            foreach (var item in writer.Direzioni)
                            {
                                if (item.AllDir)
                                {
                                    aree.Add("RISORSE CHIAVE");
                                }
                                else
                                {
                                    if (!item.Funzione.Contains("RS"))
                                        aree.AddRange(dbDir.Where(x => item.Direzioni.Any(y => y.Item1 == x.CODICE) && !x.XR_PRV_AREA.NOME.Contains("RISORSE CHIAVE")).Select(x => x.XR_PRV_AREA.NOME.Replace("AREA", "")).Distinct());
                                    else
                                        aree.Add("RISORSE CHIAVE");
                                }
                            }

                            worksheet.Cell(row, 6).SetValue(String.Join("\r\n", aree.Distinct()));
                            worksheet.Cell(row, 6).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                            worksheetDir.Cell(row, 4).SetValue(String.Join(",", writer.Direzioni.SelectMany(x => x.Direzioni).Distinct().OrderBy(x => x.Item1).Select(x => x.Item1 + " - " + x.Item2)));
                        }
                    }
                }
                worksheet.Columns().AdjustToContents();
                worksheet.Rows().AdjustToContents();

                worksheetDir.Columns().AdjustToContents();
                worksheetDir.Rows().AdjustToContents();

                workbook.SaveAs(@"c:\tmp\Elenco Abil " + funzione + " 2.xlsx");
                Process.Start(@"c:\tmp\Elenco Abil " + funzione + " 2.xlsx");
            }
        }
    }
}
