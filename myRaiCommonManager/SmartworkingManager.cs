using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using DocumentFormat.OpenXml.Office.Word;
using DocumentFormat.OpenXml.Spreadsheet;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Models.Smartworking;
using myRaiCommonManager.Model.Smartworking;
using myRaiCommonModel;
using myRaiData;
using myRaiData.Incentivi;
using myRaiHelper;

namespace myRaiCommonManager
{
    public class StatoRapporto_API_chain
    {
        public int order { get; set; }
        public string TipologiaAPI { get; set; }
        public myRaiDataTalentia.XR_STATO_RAPPORTO StatoRapporto { get; set; }
    }
     




    public class ModificaComunicazioneModel
    {
        public DateTime? DataInizio { get; set; }
        public DateTime? DataFine { get; set; }
        public String Matricola { get; set; }

        public String CodiceComunicazione { get; set; }
    }
    public class NuovaComunicazioneModel
    {
        public DateTime? DataInizio { get; set; }
        public DateTime? DataFine { get; set; }
        public String Matricola { get; set; }
    }
    internal class DatiSWHrdw
    {
        public string matricola_dp { get; set; }
        public string voce_te { get; set; }
        public DateTime? data_cessazione { get; set; }
        public string cod_mansione { get; set; }
        public string desc_mansione { get; set; }
        public string istituto_assicuratore_mansione { get; set; }
        public string tipo_minimo { get; set; }
        public string codice_tipo_forma_contr { get; set; }
        public string cod_serv_cont { get; set; }
        public string desc_serv_cont { get; set; }
        public string cod_macro_categoria { get; set; }
        public string desc_macro_categoria { get; set; }
        public string cod_categoria { get; set; }
        public string desc_categoria { get; set; }
        public string desc_liv_professionale { get; set; }
        public string desc_aggregato_sede { get; set; }
        public string desc_sede { get; set; }
        public string ccl { get; set; }
        public string societa { get; set; }
        public string cod_insediamento { get; set; }
        public string desc_insediamento { get; set; }
    }
    public class SmartworkingManager
    {


        private static string SerializeObject<T>(T oggetto) where T : class
        {
            string result;
            var emptyNamespaces = new XmlSerializerNamespaces(new[] { new XmlQualifiedName("", "http://mlps.gov.it/DataModels/InformationDelivery/SmartWorking/1.0") });
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

        private static bool GetPatInail(string aiCode, out string pat, out string inail)
        {
            bool result = true;
            pat = null;
            inail = null;

            switch (aiCode)
            {
                case "F":
                    pat = "7979998";
                    inail = "0541";
                    break;
                case "A":
                case "L":
                    pat = "93149632";
                    inail = "4220";
                    break;
                case "C":
                case "N":
                    pat = "93149632";
                    inail = "0530";
                    break;
                case "D":
                case "P":
                    pat = "32160456";
                    inail = "0722";
                    break;
                case "E":
                case "R":
                    pat = "93150473";
                    inail = "0723";
                    break;
                case "B":
                case "M":
                    pat = "32519983";
                    inail = "0511";
                    break;
                case "W":
                    pat = "0000000";
                    inail = "0000";
                    break;
                case "6":
                    pat = "99990001";
                    inail = "0000";
                    break;
                default:
                    result = false;
                    pat = "99990001";
                    inail = "0000";
                    break;
            }

            return result;
        }

        private static Dictionary<string, R_STRGROUP> _rStrGroup;
        private static bool GetDatiSocieta(string codice, out string cf, out string ragSoc)
        {
            bool result = false;
            cf = null;
            ragSoc = null;

            R_STRGROUP rStr = null;
            if (_rStrGroup == null)
            {
                IncentiviEntities db = new IncentiviEntities();
                rStr = db.R_STRGROUP.FirstOrDefault(x => x.COD_IMPRESA == codice && x.STRGROUP.DTA_INIZIO <= DateTime.Today && x.STRGROUP.DTA_FINE >= DateTime.Today);
            }
            else
                result = _rStrGroup.TryGetValue(codice, out rStr);

            if (rStr != null)
            {
                result = true;
                cf = rStr.PIV_PIVASOCIETA;
                ragSoc = rStr.NOT_PURPOSE;
            }

            return result;
        }

        private static Dictionary<string, DatiSWHrdw> _datiHrdw;
        private static bool GetTipologiaCcontrattuale(string matricola, out string tipoContr)
        {
            bool result = false;
            tipoContr = null;

            DatiSWHrdw matrData = null;

            if (_datiHrdw == null)
            {
                string query = $@"select t0.[matricola_dp]   
                                  ,substring(t1.[CODICINI],3,1) as voce_te   
	                              ,t14.codice_tipo_forma_contr
                                  ,t3.[tipo_minimo]   
                             FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0   
                             INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE] t1 ON(t1.[SKY_MATRICOLA] = t0.[sky_anagrafica_unica])   
                             INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_CATEGORIA_LIVELLO] t3 ON(t3.[sky_livello_categ] = t1.[sky_livello_categ])   
                             inner join [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_FORMA_CONTRATTO] T14 ON (t1.sky_forma_contratto = t14.[sky_forma_contratto])
                             where t1.data_inizio_validita <=DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) and t1.data_fine_validita >=DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE()))
                             and matricola_dp='{matricola}'";

                IncentiviEntities db = new IncentiviEntities();
                var res = db.Database.SqlQuery<DatiSWHrdw>(query);
                if (res != null && res.Any())
                    matrData = res.FirstOrDefault();
            }
            else
            {
                result = _datiHrdw.TryGetValue(matricola, out matrData);
            }

            if (matrData != null)
            {
                result = true;
                if (matrData.codice_tipo_forma_contr == "9")
                {
                    if (matrData.tipo_minimo == "5" || matrData.tipo_minimo == "7")
                        tipoContr = "A03";
                    else
                        tipoContr = "A01";
                }
                else
                    tipoContr = "A02";
            }

            return result;
        }
        private static void LoadDatiHrdw(IEnumerable<string> matricole)
        {
            _datiHrdw = new Dictionary<string, DatiSWHrdw>();
            string elencoMatr = "'" + String.Join("','", matricole) + "'";
            string query = $@"select t0.[matricola_dp]   
                                  ,substring(t1.[CODICINI],3,1) as voce_te   
	                              ,t14.codice_tipo_forma_contr
                                  ,t3.[tipo_minimo]   
                             FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0   
                             INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE] t1 ON(t1.[SKY_MATRICOLA] = t0.[sky_anagrafica_unica])   
                             INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_CATEGORIA_LIVELLO] t3 ON(t3.[sky_livello_categ] = t1.[sky_livello_categ])   
                             inner join [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_FORMA_CONTRATTO] T14 ON (t1.sky_forma_contratto = t14.[sky_forma_contratto])
                             where t1.data_inizio_validita <=DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE())) and t1.data_fine_validita >=DATEADD(dd, 0, DATEDIFF(dd, 0, GETDATE()))
                             and matricola_dp in ({elencoMatr})";

            IncentiviEntities db = new IncentiviEntities();
            var res = db.Database.SqlQuery<DatiSWHrdw>(query);
            if (res != null)
                _datiHrdw = res.ToDictionary(x => x.matricola_dp, x => x);
        }

        private static Dictionary<string, string> _datiAI;
        private static bool GetAiCode(string matricola, out string codice, out DateTime? lastUpdate)
        {
            bool result = false;
            codice = null;
            lastUpdate = null;

            if (_datiAI == null)
            {
                return AnagraficaManager.GetAiCode(matricola, out codice, out lastUpdate);
            }
            else
            {
                result = _datiAI.TryGetValue(matricola, out codice);
            }

            return result;
        }
        private static void LoadDatiAI(IEnumerable<string> matricole)
        {
            var elencoMatr = "''" + String.Join("'',''", matricole.Select(x => "0" + x)) + "''";

            var listDb2Qual = DB2Manager.SqlQuery<TQUALIFICA>(String.Format("SELECT * FROM OPENQUERY(DB2LINK, 'SELECT * FROM " + DB2Manager.GetPrefixTable() + ".TQUALIFICA WHERE matricola in ({0}) and DATINI<''{1:yyyy-MM-dd}'' and DATFIN>''{1:yyyy-MM-dd}''')", elencoMatr, DateTime.Today));
            var dbDigi = new digiGappEntities();
            var listCache = dbDigi.MyRai_CacheFunzioni.Where(x => matricole.Contains(x.oggetto) && x.funzione == "AssicurazioneInfortuni").ToList();

            foreach (var matr in matricole)
            {
                bool result = false;
                string codice = "";
                DateTime? lastUpdate = null;

                var db2Qual = listDb2Qual.FirstOrDefault(x => x.MATRICOLA == matr);
                if (db2Qual != null)
                {
                    result = true;
                    codice = db2Qual.ASS_INF;
                    lastUpdate = db2Qual.DATAGG;
                }

                DateTime rifDate = lastUpdate.HasValue ? lastUpdate.Value : DateTime.MinValue;
                var cacheCode = listCache.FirstOrDefault(x => x.oggetto == matr && x.funzione == "AssicurazioneInfortuni" && x.data_creazione > rifDate);
                if (cacheCode != null)
                {
                    result = true;
                    codice = cacheCode.dati_serial;
                    lastUpdate = cacheCode.data_creazione;
                }

                if (result)
                    _datiAI.Add(matr, codice);
            }

        }

        public static bool ComunicaInizioPeriodo(string matricola, DateTime dtSottoscrizione, DateTime dtInizio, DateTime dtFine, out string xml)
        {
            bool result = false;

            IncentiviEntities db = new IncentiviEntities();
            SINTESI1 sint = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == matricola);

            CreaComunicazione_Input input = new CreaComunicazione_Input();
            GetDatiSocieta(sint.COD_IMPRESACR, out string impCF, out string impRagSoc);
            input.SezioneDatoreLavoro = new SezioneDatoreLavoro()
            {
                CodiceFiscaleDatoreLavoro = impCF,      //16alfa-11num
                DenominazioneDatoreLavoro = impRagSoc   //200
            };

            input.SezioneLavoratore = new SezioneLavoratore()
            {
                CodiceFiscaleLavoratore = sint.CSF_CFSPERSONA,          //16alfa-11num
                NomeLavoratore = sint.DES_NOMEPERS.Trim().TitleCase(),         //50
                CognomeLavoratore = sint.DES_COGNOMEPERS.Trim().TitleCase(),   //50
                DataNascitaLavoratore = sint.DTA_NASCITAPERS.Value,     //19 - 2000-01-29T00:00:00
                CodComuneNascitaLavoratore = sint.COD_CITTANASC,        //4 - catastale
            };


            GetAiCode(matricola, out string aiCode, out DateTime? lastAiUpdate);
            GetPatInail(matricola, out string pat, out string inail);
            GetTipologiaCcontrattuale(matricola, out string tipoContr);
            input.SezioneRapportoLavoro = new SezioneRapportoLavoro_Crea()
            {
                DataInizioRapportoLavoro = new DateTime(),              //19 - 2000-01-29T00:00:00
                CodTipologiaRapportoLavoro = tipoContr,                 //A01 - TI, A02 - TD, A03 - Apprendisti
                PosizioneINAIL = pat,                                   //PAT
                TariffaINAIL = inail,                                   //INAIL
            };

            input.SezioneAccordoSmartWorking = new SezioneAccordoSmartWorking()
            {
                DataSottoscrizioneAccordo = dtSottoscrizione,           //19 - 2000-01-29T00:00:00
                DataInizioPeriodo = dtInizio,                           //19 - 2000-01-29T00:00:00
                DataFinePeriodo = dtFine,                               //19 - 2000-01-29T00:00:00 obbligatoria se sw a tempo det.
                TipologiaDurataPeriodo = "TD",                          //TI - sw tempo indetermintato, TD - sw tempo determinato
                StreamPDF = null
            };

            //Soggetto abilitato

            input.CodTipologiaComunicazione = "I";

            xml = SerializeObject(input);
            result = true;

            return result;
        }

        public static bool ComunicaModificaPeriodo(string matricola, string codiceComunicazione, DateTime dtSottoscrizione, DateTime dtInizio, DateTime dtFine, out string xml)
        {
            bool result = false;

            ModificaComunicazione_Input input = new ModificaComunicazione_Input();
            input.CodiceComunicazione = codiceComunicazione;

            AnagraficaManager.GetAiCode(matricola, out string aiCode, out DateTime? lastAiUpdate);
            GetPatInail(matricola, out string pat, out string inail);
            GetTipologiaCcontrattuale(matricola, out string tipoContr);
            input.SezioneRapportoLavoro = new SezioneRapportoLavoro_Modifica()
            {
                CodTipologiaRapportoLavoro = tipoContr,     //A01 - TI, A02 - TD, A03 - Apprendisti
                PosizioneINAIL = pat,                       //PAT
                TariffaINAIL = inail,                       //INAIL
            };

            input.SezioneAccordoSmartWorking = new SezioneAccordoSmartWorking_Input()
            {
                DataSottoscrizioneAccordo = dtSottoscrizione,           //19 - 2000-01-29T00:00:00
                DataFinePeriodo = dtFine,                               //19 - 2000-01-29T00:00:00 obbligatoria se sw a tempo det.
                TipologiaDurataPeriodo = "TD",                            //TI - sw tempo indetermintato, TD - sw tempo determinato
            };

            //Soggetto abilitato

            input.CodTipologiaComunicazione = "M";

            xml = SerializeObject(input);

            return result;
        }

        public static bool ComunicaAnnullamentoPeriodo(string codiceComunicazione, DateTime dtSottoscrizione, DateTime dtInizio, DateTime dtFine, out string xml)
        {
            bool result = false;

            AnnullaComunicazione_Input input = new AnnullaComunicazione_Input();
            input.CodiceComunicazione = codiceComunicazione;

            //Soggetto abilitato
            input.CodTipologiaComunicazione = "A";

            xml = SerializeObject(input);

            return result;
        }


        public static MemoryStream CreaZip()
        {
            MemoryStream outputMemStream = new MemoryStream();
            ZipOutputStream zipStream = new ZipOutputStream(outputMemStream);

            zipStream.SetLevel(3);

            IncentiviEntities dbCzn = new IncentiviEntities();
            _rStrGroup = dbCzn.R_STRGROUP.Where(x => x.STRGROUP.DTA_INIZIO <= DateTime.Today && x.STRGROUP.DTA_FINE >= DateTime.Today).ToDictionary(x => x.COD_IMPRESA, x => x);

            var dbTal = new myRaiDataTalentia.TalentiaEntities();
            var list = dbTal.XR_STATO_RAPPORTO
                                .Include("SINTESI1")
                                .Where(x => x.COD_STATO_RAPPORTO == "SW" && x.COD_TIPO_ACCORDO == "Consensuale" && x.DTA_NOTIF_ENTE == null).ToList();

            LoadDatiHrdw(list.Select(x => x.MATRICOLA).Distinct());
            LoadDatiAI(list.Select(x => x.MATRICOLA).Distinct());

            foreach (var item in list)
            {
                R_STRGROUP soc = null;
                _rStrGroup.TryGetValue(item.SINTESI1.COD_IMPRESACR, out soc);
                string fileName = soc.PIV_PIVASOCIETA + "-" + item.SINTESI1.CSF_CFSPERSONA;

                if (ComunicaInizioPeriodo(item.MATRICOLA, item.DTA_NOTIF_DIP.HasValue ? item.DTA_NOTIF_DIP.Value : DateTime.Today, item.DTA_INIZIO, item.DTA_FINE, out string xml))
                {
                    using (Stream xmlStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml)))
                    {
                        ZipEntry zipXmlEntry = new ZipEntry(Path.ChangeExtension(fileName, ".xml"));
                        zipXmlEntry.DateTime = DateTime.Now;
                        zipStream.PutNextEntry(zipXmlEntry);
                        StreamUtils.Copy(xmlStream, zipStream, new byte[4096]);
                        zipStream.CloseEntry();
                    }

                    var mod = dbTal.XR_MOD_DIPENDENTI.FirstOrDefault(x => x.MATRICOLA == item.MATRICOLA && x.COD_MODULO == "AccordoIndividualeDipendentiSW2022");
                    if (mod != null)
                    {
                        using (Stream pdfStream = new MemoryStream(mod.PDF_MODULO))
                        {
                            using (Stream outStream = new MemoryStream())
                            {


                                ZipEntry zipPdfEntry = new ZipEntry(Path.ChangeExtension(fileName, ".pdf"));
                                zipPdfEntry.DateTime = DateTime.Now;
                                zipStream.PutNextEntry(zipPdfEntry);
                                StreamUtils.Copy(pdfStream, zipStream, new byte[4096]);
                                zipStream.CloseEntry();
                            }
                        }
                    }
                }
            }

            zipStream.IsStreamOwner = false;
            zipStream.Close();

            outputMemStream.Position = 0;

            _rStrGroup = null;
            _datiHrdw = null;
            _datiAI = null;

            return outputMemStream;
        }

        public static List<GenericKeyValue> GetGenitoreVoci()
        {

            List<GenericKeyValue> L = new List<GenericKeyValue>();
            L.Add(new GenericKeyValue() { text = "Soggetto disoccupato", value = "1" });
            L.Add(new GenericKeyValue() { text = "Soggetto inoccupato (es. studente, casalinga)", value = "2" });
            L.Add(new GenericKeyValue() { text = "Libero professionista", value = "3" });
            L.Add(new GenericKeyValue() { text = "Imprenditore", value = "4" });
            L.Add(new GenericKeyValue() { text = "Artigiano", value = "5" });
            L.Add(new GenericKeyValue() { text = "Commerciante", value = "6" });
            L.Add(new GenericKeyValue() { text = "Iscritto alla gestione separata INPS", value = "7" });
            L.Add(new GenericKeyValue() { text = "Lavoratore autonomo dello spettacolo", value = "8" });
            L.Add(new GenericKeyValue() { text = "Lavoratore agricolo", value = "9" });
            L.Add(new GenericKeyValue() { text = "Lavoratore dipendente con mansioni incompatibili con il lavoro agile", value = "10" });
            L.Add(new GenericKeyValue() { text = "Lavoratore dipendente in servizio presso un datore di lavoro che non ricorre al lavoro agile", value = "11" });
            L.Add(new GenericKeyValue() { text = "Lavoratore dipendente non collocato dal datore di lavoro in regime di lavoro agile", value = "12" });
            L.Add(new GenericKeyValue() { text = "Altro", value = "13" });
            return L;
        }

      

 

        

        public static void CompilaCSV(WebClient wb)
        {
            string RICERCA_COMUNICAZIONI_URL = myRaiHelper.CommonHelper.GetParametro<string>(myRaiHelper.EnumParametriSistema.API_SW_RicercaComunicazioni);

            var dbtal = new myRaiDataTalentia.TalentiaEntities();
            var matricole = dbtal.XR_STATO_RAPPORTO.Select(x => x.MATRICOLA).Distinct().OrderBy(x => x).ToList();
            var sintesi = dbtal.SINTESI1.Select(x => new { x.COD_MATLIBROMAT, x.CSF_CFSPERSONA }).ToList();

            string fileName = @"C:\Users\a862039\OneDrive - Atos\Desktop\Rai\SWAPISW_READ_" + DateTime.Now.ToString("ddMMyyyy_HHmm") + ".csv";
            System.IO.File.AppendAllText(fileName,
                "Matricola, CF Azienda,Denominazione Azienda,CF Lavoratore,Cod Comune,Cod Comunicazione,Cognome,Nome,Data Nascita,Data Inizio Rapporto, Data Invio,"+
                "Data Inizio Periodo, Data Fine Accordo\r\n" );

            foreach (var matr in matricole)
            {
                string cf = sintesi.Where(x => x.COD_MATLIBROMAT == matr).Select(x => x.CSF_CFSPERSONA).FirstOrDefault();
                string response = wb.DownloadString(RICERCA_COMUNICAZIONI_URL + "?CFLavoratore=" + cf);
                myRaiHelper.RicercaComunicazione.RicercaComunicazioniResponse RCR = Newtonsoft.Json.JsonConvert.DeserializeObject<myRaiHelper.RicercaComunicazione.RicercaComunicazioniResponse>(response);
                if (RCR.Comunicazioni != null && RCR.Comunicazioni.Any())
                {
                    foreach (var c in RCR.Comunicazioni)
                    {
                        string Linea =matr+","+ c.CFAzienda + "," + c.denominazioneAzienda + "," + c.CFLavoratore + "," + c.codComuneStatoEsteroNascitaLavoratore
                            + ",'" + c.codiceComunicazione + "," + c.cognomeLavoratore + "," + c.nomeLavoratore + "," + c.dataNascitaLavoratore.ToString("dd/MM/yyyy") +
                            "," + c.dataInizioRapporto.ToString("dd/MM/yyyy") + "," + c.dataInvio.ToString("dd/MM/yyyy HH:mm") + "," +
                              c.dataInizioPeriodo.ToString("dd/MM/yyyy")+ "," + c.dataFineAccordo.ToString("dd/MM/yyyy") + "\r\n";

                        System.IO.File.AppendAllText(fileName, Linea);
                    }
                }
            }
        }
    }
}
