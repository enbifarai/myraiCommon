using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using myRai.DataAccess;
using myRaiCommonModel;
using myRaiCommonModel.cvModels.Pdf;
using myRaiCommonModel.Gestionale;
using myRaiCommonModel.Gestionale.HRDW;
using myRaiData;
using myRaiData.Incentivi;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Xceed.Words.NET;

namespace myRaiCommonManager
{
    public class PoliticheRetributiveManager
    {
        #region GestioneLog
        public static void LogChangeEffectiveDate(IncentiviEntities db, int idPratica, DateTime? oldDec, DateTime? newDec, string matricola = null)
        {
            if (db == null)
                db = new IncentiviEntities();

            string message = "";
            if (!oldDec.HasValue)
            {
                message = "Impostata data decorrenza " + newDec.Value.ToString("dd/MM/yyyy");
            }
            else
            {
                message = "Cambiata data decorrenza da " + oldDec.Value.ToString("dd/MM/yyyy") + " a " + newDec.Value.ToString("dd/MM/yyyy");
            }

            WriteLog(db, idPratica, message, matricola);

        }
        public static void LogChangeProvv(IncentiviEntities db, int idPratica, int oldProvv, int newProvv, string matricola = null)
        {
            if (db == null)
                db = new IncentiviEntities();

            var oldProvvDB = db.XR_PRV_PROV.FirstOrDefault(x => x.ID_PROV == oldProvv);
            var newProvvDB = db.XR_PRV_PROV.FirstOrDefault(x => x.ID_PROV == newProvv);

            string oldProvvStr = "'" + oldProvvDB.NOME + "'";
            if (oldProvvDB.CUSTOM.GetValueOrDefault())
                oldProvvStr += " (manuale)";

            string newProvvStr = "'" + newProvvDB.NOME + "'";
            if (newProvvDB.CUSTOM.GetValueOrDefault())
                newProvvStr += " (manuale)";

            string message = "Cambiato provvedimento da " + oldProvvStr + " a " + newProvvStr;

            WriteLog(db, idPratica, message, matricola);
        }
        public static void LogGestioneManuale(IncentiviEntities db, int idPratica, bool currentState, string matricola = null)
        {
            if (db == null)
                db = new IncentiviEntities();

            string message = "";
            if (currentState)
                message = "Attivata gestione manuale";
            else
                message = "Disattivata gestione manuale";

            WriteLog(db, idPratica, message, matricola);
        }

        public static void WriteLog(IncentiviEntities db, int idPratica, string message, string matricola = null)
        {
            if (db == null)
                db = new IncentiviEntities();

            string matr = matricola;
            if (String.IsNullOrWhiteSpace(matricola))
                matr = CommonHelper.GetCurrentUserMatricola();

            int idPersona = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == matr).ID_PERSONA;

            XR_PRV_LOG log = new XR_PRV_LOG();
            log.ID_PERSONA = idPersona;
            log.ID_DIPENDENTE = idPratica;
            log.MATRICOLA = matr;
            log.MESSAGGIO = message;
            log.TMS_TIMESTAMP = DateTime.Now;
            db.XR_PRV_LOG.Add(log);

            try
            {
                db.SaveChanges();
            }
            catch (Exception)
            {

            }
        }
        #endregion

        #region GestioneCauseVertenze
        public static bool HasCauseAperte(IncentiviEntities db, string matricola, int idDipendente)
        {
            if (db == null)
                db = new IncentiviEntities();

            return db.XR_PRV_DIPENDENTI_CAUSE.Any(x => x.MATRICOLA == matricola) || db.XR_PRV_DIPENDENTI_CAUSESERV.Any(x => x.ID_DIPENDENTE == idDipendente);
        }
        public static bool HasVertSindacali(IncentiviEntities db, string matricola)
        {
            if (db == null)
                db = new IncentiviEntities();

            return db.XR_PRV_DIPENDENTI_VERTENZE.Any(x => x.MATRICOLA == matricola);
        }
        public static bool HasVertStragiudiziali(IncentiviEntities db, int idDipendente)
        {
            if (db == null)
                db = new IncentiviEntities();

            return db.XR_PRV_DIPENDENTI_STRAGIUDIZIALE.Any(x => x.ID_DIPENDENTE == idDipendente);
        }
        public static bool HasProvvedimentoDisciplinari(IncentiviEntities db, int idDipendente)
        {
            if (db == null)
                db = new IncentiviEntities();

            return db.XR_PRV_DIPENDENTI_PROVVD.Any(x => x.ID_DIPENDENTE == idDipendente);
        }
        public static bool HasProvvedimentiDisciplinari(string matricola)
        {
            bool result = false;
            MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client cl = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
            cl.ClientCredentials.Windows.ClientCredential = CommonHelper.GetUtenteServizioCredentials();
            var resp = cl.GetProvvedimentiCause(CommonHelper.GetCurrentUserMatricola(), matricola);
            if (resp.Esito)
            {
                result = resp.PDaperti > 0;
            }
            return false;
        }
        #endregion

        #region GestioneLettere
        public static MemoryStream CreaLettere(out string nomeFile, int? idDirezione, int? idCamp, int? anno, List<int> provs = null)
        {
            IncentiviEntities db = new IncentiviEntities();
            IQueryable<XR_PRV_DIPENDENTI> pratiche = PoliticheRetributiveHelper.GetPratiche(db, true, "SINTESI1", "XR_PRV_OPERSTATI", "XR_PRV_PROV_EFFETTIVO");

            var notNessunProv = PoliticheRetributiveHelper.NotAnyOfProv();
            pratiche.Where(x => x.ID_CAMPAGNA > 2)
                        .Where(notNessunProv)
                        .Where(x => x.ID_TEMPLATE != null && x.ID_TEMPLATE > 0 && (x.IND_PRATICA_EXT == null || x.IND_PRATICA_EXT == false));

            string nomeDirezione = "";
            string nomeCampagna = "";
            string descrAnno = "";

            if (idDirezione.HasValue)
            {
                pratiche = pratiche.Where(x => x.ID_DIREZIONE == idDirezione.Value);
                XR_PRV_DIREZIONE dir = db.XR_PRV_DIREZIONE.FirstOrDefault(x => x.ID_DIREZIONE == idDirezione);
                nomeDirezione = " " + dir.NOME;
            }

            if (idCamp.HasValue && idCamp.Value > 0)
            {
                pratiche = pratiche.Where(x => x.ID_CAMPAGNA == idCamp);
                XR_PRV_CAMPAGNA campaign = db.XR_PRV_CAMPAGNA.FirstOrDefault(x => x.ID_CAMPAGNA == idCamp);
                nomeCampagna = " " + campaign.NOME;
            }

            if (anno.HasValue)
            {
                pratiche = pratiche.Where(x => x.DECORRENZA != null && x.DECORRENZA.Value.Year == anno.Value);
                descrAnno = " Decorrenza " + anno.Value.ToString();
            }

            if (provs != null && provs.Count() > 0)
            {
                var anyOfThisProv = PoliticheRetributiveHelper.AnyOfProv(PoliticheRetributiveHelper.GetProvByNumber(db, provs));
                pratiche = pratiche.Where(anyOfThisProv);
            }

            MemoryStream outputMemStream = CreaLetteraTotale(db, pratiche);

            string filterName = string.Format("{0}{1}{2}", nomeDirezione, nomeCampagna, descrAnno);
            nomeFile = String.Format("Lettere{0}.pdf", filterName);

            return outputMemStream;
        }

        public static MemoryStream CreaLettera(IncentiviEntities db, XR_PRV_DIPENDENTI pratica, out string nomeFile)
        {
            List<XR_PRV_TEMPLATE> templates = db.XR_PRV_TEMPLATE.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now).ToList();
            nomeFile = "Lettera " + pratica.XR_PRV_PROV_EFFETTIVO.DESCRIZIONE.TitleCase() + " " + pratica.SINTESI1.Nominativo() + ".pdf";

            string logoIcon = GetImageDoc(db.XR_PRV_TEMPLATE.FirstOrDefault(x => x.NOME == "LOGO").TEMPLATE);
            string infoSoc = GetImageDoc(db.XR_PRV_TEMPLATE.FirstOrDefault(x => x.NOME == "INTESTAZIONE").TEMPLATE);
            string footerSoc1 = GetTemplateText(templates, 0, "FOOTER_SOC_1", null, null);
            string footerSoc2 = GetTemplateText(templates, 0, "FOOTER_SOC_2", null, null);
            var baseTemplate = GetTemplate(templates, 0, "BASE", null, null);

            MemoryStream ms = new MemoryStream();
            string title = "";

            PdfPrinter pdf = new PdfPrinter(logoIcon, infoSoc, title, footerSoc1, footerSoc2);
            pdf.Apri(baseTemplate.TEMPLATE);
            CreaPaginaLettera(db, pratica, pdf, templates);

            pdf.Chiudi(out ms);

            return ms;
        }

        private static MemoryStream CreaLetteraTotale(IncentiviEntities db, IEnumerable<XR_PRV_DIPENDENTI> pratiche)
        {
            List<XR_PRV_TEMPLATE> templates = db.XR_PRV_TEMPLATE.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now).ToList();
            List<QUALIFICA> qualifiche = db.QUALIFICA.ToList();
            List<myRai.Data.CurriculumVitae.DFiguraPro> figureProf = null;
            List<Inquadramento> inquadramenti = new List<Inquadramento>();
            using (var ctx = new myRai.Data.CurriculumVitae.cv_ModelEntities())
            {
                figureProf = ctx.DFiguraPro.ToList();

                inquadramenti.AddRange(ctx.Database.SqlQuery<Inquadramento>("SELECT [cod_divisione],[des_divisione],[cod_direzione],[des_direzione],[cod_struttura],[des_struttura],[cod_sezione],[des_sezione] FROM [PERSDIP].[user_perseo].[TSezioneOpa]  where data_fine_validita = '99991231'"));
            }

            string logoIcon = GetImageDoc(db.XR_PRV_TEMPLATE.FirstOrDefault(x => x.NOME == "LOGO").TEMPLATE);
            string infoSoc = GetImageDoc(db.XR_PRV_TEMPLATE.FirstOrDefault(x => x.NOME == "INTESTAZIONE").TEMPLATE);
            string footerSoc1 = GetTemplateText(templates, 0, "FOOTER_SOC_1", null, null);
            string footerSoc2 = GetTemplateText(templates, 0, "FOOTER_SOC_2", null, null);
            var baseTemplate = GetTemplate(templates, 0, "BASE", null, null);

            Dictionary<string, string> direzioni = db.XR_PRV_DIREZIONE.Select(y => new { y.CODICE, y.NOME }).Distinct().ToDictionary(y => y.CODICE, y => y.NOME);

            MemoryStream ms = new MemoryStream();
            string title = "";

            PdfPrinter pdf = new PdfPrinter(logoIcon, infoSoc, title, footerSoc1, footerSoc2);
            pdf.Apri(baseTemplate.TEMPLATE);


            var groupList = pratiche.GroupBy(x => x.XR_PRV_DIREZIONE.CODICE);
            //foreach (var group in groupList)
            //{
            //    pdf.WriteLine(String.Format("{0} - {1}: {2}", group.Key, direzioni[group.Key], group.Count()));
            //}

            bool isAfterFirst = false;

            foreach (var direzione in groupList)
            {
                if (isAfterFirst) pdf.NewPage();

                pdf.AggiungiHeaderDirezione(direzione.Key, direzioni[direzione.Key], direzione.Count());

                foreach (var pratica in direzione.OrderBy(x => x.MATRICOLA))
                {
                    pdf.NewPage();
                    CreaPaginaLettera(db, pratica, pdf, templates, qualifiche, figureProf, inquadramenti);
                }

                isAfterFirst = true;
            }

            pdf.Chiudi(out ms);

            return ms;
        }

        public static MemoryStream CreaRiepilogoProvv(int esitoLettera, List<int> provs)
        {
            IncentiviEntities db = new IncentiviEntities();

            List<XR_PRV_TEMPLATE> templates = db.XR_PRV_TEMPLATE.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now).ToList();
            string logoIcon = GetImageDoc(db.XR_PRV_TEMPLATE.FirstOrDefault(x => x.NOME == "LOGO").TEMPLATE);
            string infoSoc = GetImageDoc(db.XR_PRV_TEMPLATE.FirstOrDefault(x => x.NOME == "INTESTAZIONE").TEMPLATE);
            string footerSoc1 = GetTemplateText(templates, 0, "FOOTER_SOC_1", null, null);
            string footerSoc2 = GetTemplateText(templates, 0, "FOOTER_SOC_2", null, null);
            var baseTemplate = GetTemplate(templates, 0, "BASE", null, null);

            var notNessunProv = PoliticheRetributiveHelper.NotAnyOfProv();
            var tmp = PoliticheRetributiveHelper.GetPratiche(db, true, "SINTESI1", "XR_PRV_OPERSTATI", "XR_PRV_PROV_EFFETTIVO")
                        .Where(x => x.ID_CAMPAGNA > 2)
                        .Where(notNessunProv)
                        .Where(x => x.ID_TEMPLATE != null && x.ID_TEMPLATE > 0
                                    && (x.IND_PRATICA_EXT == null || x.IND_PRATICA_EXT == false));

            ProvvStatoEnum provvStato = ProvvStatoEnum.Convalidato;

            switch (esitoLettera)
            {
                case 1:
                    provvStato = ProvvStatoEnum.Consegnato;
                    break;
                case 2:
                    provvStato = ProvvStatoEnum.Rifiutato;
                    break;
                default:
                    break;
            }

            tmp = tmp.Where(x => x.XR_PRV_OPERSTATI.Where(y => y.DATA_FINE_VALIDITA == null).Max(z => z.ID_STATO) == (int)provvStato);

            if (provs != null && provs.Count() > 0)
            {
                var anyOfThisProv = PoliticheRetributiveHelper.AnyOfProv(PoliticheRetributiveHelper.GetProvByNumber(db, provs));
                tmp = tmp.Where(anyOfThisProv);
            }

            var pratiche = tmp.OrderBy(x => x.MATRICOLA);

            MemoryStream ms = new MemoryStream();

            string title = "";

            PdfPrinter pdf = new PdfPrinter(logoIcon, infoSoc, title, footerSoc1, footerSoc2, 750);
            pdf.Apri(baseTemplate.TEMPLATE);

            pdf.WriteLine("\n\n");
            pdf.WriteLine("Oggetto: Riepilogo lettere consegnate", true);
            pdf.WriteLine("\n");

            pdf.AggiungiTableRiepilogoHeader();

            foreach (var pratica in pratiche)
            {
                pdf.AggiungiTableRiepilogo(pratica);
            }

            pdf.Chiudi(out ms);

            return ms;
        }

        public static void CreaDirectoryLettere()
        {
            IncentiviEntities db = new IncentiviEntities();
            string baseDir = @"c:\tmp\PRetrib_Lettere";
            if (!Directory.Exists(baseDir))
                Directory.CreateDirectory(baseDir);

            IEnumerable<XR_PRV_DIPENDENTI> pratiche = new List<XR_PRV_DIPENDENTI>();

            #region GetPratiche
            string elencoMatricole = "101636,789777";
            string elencoDirezioni = "'06', '57', '48', '12', '5F', '53', '20', '20_B', '64', '28', '08', '50', '19', '96', '26', '5D', '92', '30', '42', '89', '13', '54', '5B', '04', '97', '55', '38', '80', '5A', '47', '10', '32', '07', '45', '49', '24', '88', '58', '29', '5C'";

            var notNessunProv = PoliticheRetributiveHelper.NotAnyOfProv();
            pratiche = PoliticheRetributiveHelper.GetPratiche(db, true, "SINTESI1", "XR_PRV_OPERSTATI", "XR_PRV_PROV_EFFETTIVO", "XR_PRV_DIREZIONE", "XR_PRV_DIREZIONE.XR_PRV_AREA")
                        .Where(x => x.ID_CAMPAGNA > 2 && (elencoDirezioni.Contains(x.XR_PRV_DIREZIONE.CODICE) || elencoMatricole.Contains(x.MATRICOLA)))
                        .Where(notNessunProv)
                        .Where(x => x.ID_TEMPLATE != null && x.ID_TEMPLATE > 0 && (x.IND_PRATICA_EXT == null || x.IND_PRATICA_EXT == false));
            #endregion

            #region GetPrintData
            List<XR_PRV_TEMPLATE> templates = db.XR_PRV_TEMPLATE.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now).ToList();
            List<QUALIFICA> qualifiche = db.QUALIFICA.ToList();
            List<myRai.Data.CurriculumVitae.DFiguraPro> figureProf = null;
            List<Inquadramento> inquadramenti = new List<Inquadramento>();
            using (var ctx = new myRai.Data.CurriculumVitae.cv_ModelEntities())
            {
                figureProf = ctx.DFiguraPro.ToList();

                inquadramenti.AddRange(ctx.Database.SqlQuery<Inquadramento>("SELECT [cod_divisione],[des_divisione],[cod_direzione],[des_direzione],[cod_struttura],[des_struttura],[cod_sezione],[des_sezione] FROM [PERSDIP].[user_perseo].[TSezioneOpa]  where data_fine_validita = '99991231'"));
            }

            string logoIcon = GetImageDoc(db.XR_PRV_TEMPLATE.FirstOrDefault(x => x.NOME == "LOGO").TEMPLATE);
            string infoSoc = GetImageDoc(db.XR_PRV_TEMPLATE.FirstOrDefault(x => x.NOME == "INTESTAZIONE").TEMPLATE);
            string footerSoc1 = GetTemplateText(templates, 0, "FOOTER_SOC_1", null, null);
            string footerSoc2 = GetTemplateText(templates, 0, "FOOTER_SOC_2", null, null);
            var baseTemplate = GetTemplate(templates, 0, "BASE", null, null);

            Dictionary<string, string> direzioni = db.XR_PRV_DIREZIONE.Select(y => new { y.CODICE, y.NOME }).Distinct().ToDictionary(y => y.CODICE, y => y.NOME);
            #endregion 


            string fileSummaryName = Path.Combine(baseDir, "Riepilogo.txt");
            List<string> summary = new List<string>();

            var notAnyProm = PoliticheRetributiveHelper.NotAnyOfProv(PoliticheRetributiveHelper.GetProvBySigla(db, PoliticheRetributiveHelper.SIGLA_PROMOZIONI)).Compile();

            foreach (var area in pratiche.GroupBy(x => x.XR_PRV_DIREZIONE.XR_PRV_AREA.NOME).OrderBy(x => x.Key))
            {
                string nomeArea = area.Key;

                summary.Add(String.Format("{0} - {1} document{2}", nomeArea, area.Count(), area.Count() == 1 ? "o" : "i"));

                string areaDirectory = Path.Combine(baseDir, nomeArea);
                if (!Directory.Exists(areaDirectory))
                    Directory.CreateDirectory(areaDirectory);

                foreach (var direzione in area.GroupBy(x => x.XR_PRV_DIREZIONE.NOME).OrderBy(x => x.Key))
                {
                    string nomeDirezione = direzione.Key;

                    List<XR_PRV_DIPENDENTI> elencoPratiche = new List<XR_PRV_DIPENDENTI>();
                    if (nomeArea.Contains("RISORSE CHIAVE"))// && nomeDirezione=="RISORSE UMANE E ORGANIZZAZIONE")
                    {
                        elencoPratiche.AddRange(direzione.Where(notAnyProm));
                    }
                    else
                        elencoPratiche.AddRange(direzione);

                    string riepilogoDirezione = String.Format("{0} - {1} document{2}", nomeDirezione, elencoPratiche.Count(), elencoPratiche.Count() == 1 ? "o" : "i");

                    if (elencoPratiche.Count() == 0) continue;

                    summary.Add("- " + riepilogoDirezione);

                    string dirDirectory = Path.Combine(areaDirectory, nomeDirezione);

                    MemoryStream ms = new MemoryStream();
                    PdfPrinter pdf = new PdfPrinter(logoIcon, infoSoc, "", footerSoc1, footerSoc2);
                    pdf.Apri(baseTemplate.TEMPLATE);

                    pdf.WriteLine("\n");
                    pdf.WriteLine("\n");
                    pdf.WriteLine("\n");
                    pdf.WriteLine("\n");
                    pdf.WriteLine("\n");
                    pdf.WriteLine(riepilogoDirezione);

                    foreach (var pratica in elencoPratiche.OrderBy(x => x.MATRICOLA))
                    {
                        pdf.NewPage();
                        CreaPaginaLettera(db, pratica, pdf, templates, qualifiche, figureProf, inquadramenti);
                    }

                    pdf.Chiudi(out ms);

                    string nomeFile = nomeDirezione + ".pdf";
                    string praticaFileName = Path.Combine(areaDirectory, nomeFile);
                    using (FileStream fs = new FileStream(praticaFileName, FileMode.Create))
                    {
                        ms.CopyTo(fs);
                        fs.Close();
                    }
                }
            }

            File.WriteAllLines(fileSummaryName, summary);

        }

        public static void CreaPaginaLettera(IncentiviEntities db, XR_PRV_DIPENDENTI pratica, PdfPrinter pdf, List<XR_PRV_TEMPLATE> templates, List<QUALIFICA> qualifiche = null, List<myRai.Data.CurriculumVitae.DFiguraPro> figureProf = null, List<Inquadramento> inquadramenti = null)
        {
            string nominativo = pratica.SINTESI1.Nominativo();
            Lettera lettera = GetLetteraModel(db, pratica, true, templates);

            var effProv = PoliticheRetributiveHelper.GetDipProvEffettivo(pratica);

            bool isPromozione = effProv.XR_PRV_PROV.SIGLA == PoliticheRetributiveHelper.SIGLA_PROMOZIONI;

            string catPrevista = "";
            string qualSTD = "";
            string descrizioneQualifica = String.Empty;
            if (isPromozione)
            {
                if (!String.IsNullOrWhiteSpace(pratica.CAT_RICHIESTA))
                    catPrevista = pratica.CAT_RICHIESTA;
                else
                    catPrevista = effProv.CAT_PREVISTA;

                QUALIFICA qual = null;
                if (qualifiche != null)
                    qual = qualifiche.FirstOrDefault(x => x.COD_QUALIFICA == catPrevista);
                else
                    qual = db.QUALIFICA.FirstOrDefault(x => x.COD_QUALIFICA == catPrevista);
                if (qual != null)
                {
                    qualSTD = qual.COD_QUALSTD;
                    descrizioneQualifica = qual.DES_QUALIFICA;
                    descrizioneQualifica = descrizioneQualifica.Replace(qual.COD_QUALIFICA + " - ", "");
                }
            }

            iTextSharp.text.Image png = null;
            if (!String.IsNullOrWhiteSpace(lettera.SignImage))
            {
                //png = PdfPrinter.GetImage(lettera.SignImage, false);
                png = Image.GetInstance(lettera.SignImageByte);
                png.ScalePercent(21f);
            }
            //png.ScaleAbsoluteHeight(100);

            string figuraprof = "";
            Inquadramento inq = new Inquadramento();

            //DateTime dateRif = new DateTime(2019, 12, 18);

            List<DServizio> direzioni = new List<DServizio>();
            string dTx = "";

            PERSEOEntities perseoDB = new PERSEOEntities();

            using (var ctx = new myRai.Data.CurriculumVitae.cv_ModelEntities())
            {
                string codUnitaOrg = pratica.SINTESI1.COD_UNITAORG;

                //var recQual = db.INCARLAV.FirstOrDefault(x => x.ID_PERSONA == pratica.ID_PERSONA && x.DTA_INIZIO <= dateRif && x.DTA_FINE > dateRif);

                var recQual = db.INCARLAV.Where(x => x.ID_PERSONA == pratica.ID_PERSONA).OrderByDescending(w => w.DTA_FINE).ThenByDescending(x => x.TMS_TIMESTAMP).FirstOrDefault();

                if (recQual != null && codUnitaOrg != recQual.UNITAORG.COD_UNITAORG)
                {
                    codUnitaOrg = recQual.UNITAORG.COD_UNITAORG;
                    if (lettera.BodyText.Contains("__UNITA_ORG__"))
                        System.Diagnostics.Debug.WriteLine("Da controllare: " + pratica.MATRICOLA);
                }

                if (inquadramenti == null)
                {
                    var list = ctx.Database.SqlQuery<Inquadramento>("SELECT [cod_divisione],[des_divisione],[cod_direzione],[des_direzione],[cod_struttura],[des_struttura],[cod_sezione],[des_sezione] FROM [PERSDIP].[user_perseo].[TSezioneOpa]  where cod_sezione='" + codUnitaOrg + "' and data_fine_validita = '99991231'");
                    if (list != null)
                        inq = list.FirstOrDefault();
                }
                else
                {
                    inq = inquadramenti.FirstOrDefault(x => x.cod_sezione == codUnitaOrg);
                }
                if (isPromozione)
                {
                    myRai.Data.CurriculumVitae.DFiguraPro figProf = null;
                    if (figureProf != null)
                        figProf = figureProf.FirstOrDefault(x => x.CodiceFiguraPro == qualSTD);
                    else
                        figProf = ctx.DFiguraPro.FirstOrDefault(x => x.CodiceFiguraPro == qualSTD);
                    if (figProf != null)
                        figuraprof = figProf.DescriFiguraPro.ToLower();

                    if (figuraprof.Contains("f1"))
                        figuraprof = figuraprof.Replace("f1", "");
                }

                direzioni = perseoDB.Database.SqlQuery<DServizio>("SELECT * FROM [PERSDIP].[user_perseo].[DServizio] WHERE data_scadenza = '99991231'").ToList();
                if (direzioni == null)
                {
                    throw new Exception("Direzioni non trovate");
                }
                direzioni.ForEach(w =>
                {
                    w.Codice = w.Codice.Trim();
                });

                var dir = direzioni.Where(w => w.Codice == pratica.XR_PRV_DIREZIONE.CODICE).FirstOrDefault();

                if (dir == null && pratica.XR_PRV_DIREZIONE.CODICE.Trim().Length > 2)
                {
                    // se finisce qui è probabile che sia una direzione fittizia esempio 20_B CANONE SEDI
                    // che non è presente su DServizio
                    var direzione = db.XR_PRV_DIREZIONE.Where(w => w.ID_DIREZIONE == pratica.ID_DIREZIONE).FirstOrDefault();
                    if (direzione != null)
                    {
                        dTx = CommonHelper.ToTitleCase(direzione.NOME);
                    }
                }
                else if (dir != null)
                {
                    dTx = CommonHelper.ToTitleCase(String.IsNullOrEmpty(dir.DescrLunga) ? dir.Descrizione : dir.DescrLunga);
                }

                if (dir == null && String.IsNullOrEmpty(dTx))
                {
                    throw new Exception("Direzione non trovata");
                }

                // se la direzione è figlia di CTO o CFO deve scrivere
                // non solo il nome della direzione, ma anche il padre es:
                // ICT - CTO - INFRASTRUTTURE TECNOLOGICHE
                string cod_direzione = pratica.XR_PRV_DIREZIONE.CODICE.Trim();

                var _temp = ctx.Database.SqlQuery<string>("SELECT des_direzione FROM [PERSDIP].[user_perseo].[TSezioneOpa] where cod_servizio = '" + cod_direzione + "' and data_fine_validita = '99991231' and len(cod_sezione) = 2").FirstOrDefault();
                if (_temp != null &&
                    _temp.Any() &&
                    (_temp.ToUpper().Contains("CTO -") || _temp.ToUpper().Contains("CFO -") || _temp.ToUpper().Contains("INFRASTRUTTURE IMMOBILIARI E SEDI LOCALI")))
                {
                    dTx = dTx + " - " + _temp.Trim();
                }
            }

            string denom = "";
            if (pratica.SINTESI1.COD_SESSO == "M")
                denom = "Sig.";
            else
                denom = "Sig.ra";

            bool addToday = true;
            //if (pratica.XR_PRV_DIREZIONE.XR_PRV_AREA.NOME.Contains("EDITORIALE")
            //    || pratica.XR_PRV_DIREZIONE.XR_PRV_AREA.NOME.Contains("PRODUZIONE")
            //    || db.XR_PRV_DIREZIONE.Any(x=>x.CODICE==pratica.XR_PRV_DIREZIONE.CODICE && (x.XR_PRV_AREA.NOME.Contains("EDITORIALE")||x.XR_PRV_AREA.NOME.Contains("PRODUZIONE"))))
            {
                //lettera.HeaderText += pratica.MATRICOLA;
                //addToday = true;
            }

            //lettera.HeaderText = "RUO/GSR/ST/26114/" + pratica.SINTESI1.COD_MATLIBROMAT;

            // Francesco 01/12/2022 modifica commentata i quanto questo protocollo serviva per i pdf generati con la data di agosto
            //if (String.IsNullOrEmpty(lettera.HeaderText))
            //{
            //    lettera.HeaderText = "RUO/GSR/ST/26550/" + pratica.SINTESI1.COD_MATLIBROMAT;
            //}

            pdf.AggiungiTableProtHeader(lettera.HeaderText, denom, lettera.DipName, lettera.DipSurname, pratica.SINTESI1.DES_CITTASEDE, addToday, "", lettera.Data);

            lettera.BodyText = lettera.BodyText.Replace("__IMPORTO_NUM__", effProv.DIFF_RAL.ToString("N"));
            var parteIntera = (long)Decimal.Truncate(effProv.DIFF_RAL);
            var parteDecimale = (int)(Math.Round(effProv.DIFF_RAL - parteIntera, 2) * 100);
            string result = string.Format("{0}/{1:00}", parteIntera.ConvertNumberToReadableString(), parteDecimale);
            lettera.BodyText = lettera.BodyText.Replace("__IMPORTO_LETT__", result);
            if (pratica.DECORRENZA.HasValue)
                lettera.BodyText = lettera.BodyText.Replace("__DECORRENZA__", pratica.DECORRENZA.Value.ToString("d MMMM yyyy"));

            if (inq != null)
            {
                if (!String.IsNullOrWhiteSpace(inq.des_struttura))
                {
                    lettera.BodyText = lettera.BodyText.Replace("__TR_AREA__", " - area ");
                    lettera.BodyText = lettera.BodyText.Replace("__DI_AREA__", " di ");
                    lettera.BodyText = lettera.BodyText.Replace("__DELL_AREA__", " dell'area ");
                    lettera.BodyText = lettera.BodyText.Replace("__AREA__", "\"" + inq.des_sezione + "\"");
                    lettera.BodyText = lettera.BodyText.Replace("__UNITA_ORG__", "\"" + inq.des_struttura + "\"");
                    lettera.BodyText = lettera.BodyText.Replace("__DIREZIONE__", dTx);
                }
                else
                {
                    lettera.BodyText = lettera.BodyText.Replace("__TR_AREA__", "");
                    lettera.BodyText = lettera.BodyText.Replace("__DI_AREA__", "");
                    lettera.BodyText = lettera.BodyText.Replace("__DELL_AREA__", "");
                    lettera.BodyText = lettera.BodyText.Replace("__AREA__", "\"" + inq.des_sezione + "\"");
                    lettera.BodyText = lettera.BodyText.Replace("__UNITA_ORG__", "\"" + inq.des_sezione + "\"");
                    lettera.BodyText = lettera.BodyText.Replace("__DIREZIONE__", dTx);
                }
            }
            else
            {
                lettera.BodyText = lettera.BodyText.Replace("__TR_AREA__", "");
                lettera.BodyText = lettera.BodyText.Replace("__DI_AREA__", "");
                lettera.BodyText = lettera.BodyText.Replace("__DELL_AREA__", "");
                lettera.BodyText = lettera.BodyText.Replace("__AREA__", "");
                lettera.BodyText = lettera.BodyText.Replace("__UNITA_ORG__", "");
                lettera.BodyText = lettera.BodyText.Replace("__DIREZIONE__", "");
            }

            if (isPromozione)
            {
                if (lettera.Livelli.TryGetValue(effProv.LIV_PREVISTO, out string decodLiv))
                    lettera.BodyText = lettera.BodyText.Replace("__LIVELLO__", decodLiv);

                lettera.BodyText = lettera.BodyText.Replace("__FIG_PROF__", figuraprof.Trim());
            }

            lettera.BodyText = lettera.BodyText.Replace("__MANSIONE__", descrizioneQualifica);

            pdf.WriteLine(lettera.BodyText);

            pdf.WriteLine(" ");
            pdf.WriteLine(lettera.FooterText);
            pdf.WriteLine(" ");

            pdf.AggiungiFirma(png, lettera.SignText);
        }

        public static Lettera GetLetteraModel(IncentiviEntities db, XR_PRV_DIPENDENTI pratica, bool loadSignImage, List<XR_PRV_TEMPLATE> templates = null)
        {
            if (db == null)
                db = new IncentiviEntities();
            Lettera lettera = new Lettera();
            if (templates == null)
                templates = db.XR_PRV_TEMPLATE.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now).ToList();

            lettera.IdPratica = pratica.ID_DIPENDENTE;
            lettera.DipName = pratica.SINTESI1.DES_NOMEPERS.TitleCase();
            lettera.DipSurname = pratica.SINTESI1.DES_COGNOMEPERS.TitleCase();

            string qual = pratica.SINTESI1.COD_QUALIFICA;

            DateTime dateRif = new DateTime(2019, 12, 18);
            var recQual = db.ASSQUAL.FirstOrDefault(x => x.ID_PERSONA == pratica.ID_PERSONA && x.DTA_INIZIO <= dateRif && x.DTA_FINE > dateRif);
            if (recQual != null && qual != recQual.COD_QUALIFICA)
            {
                qual = recQual.COD_QUALIFICA;
                System.Diagnostics.Debug.WriteLine("Da controllare qualifica " + pratica.MATRICOLA + " - " + recQual.COD_QUALIFICA + " - " + pratica.SINTESI1.COD_QUALIFICA);
            }

            string codDir = pratica.XR_PRV_DIREZIONE.CODICE;
            //int prov = pratica.CUSTOM_PROV.GetValueOrDefault() ? pratica.XR_PRV_PROV_EFFETTIVO.BASE_PROV.Value : pratica.ID_PROV_EFFETTIVO.Value;
            var prov = PoliticheRetributiveHelper.GetDipProvEffettivo(pratica);

            //if (prov == (int)ProvvedimentiEnum.AumentoLivello || prov == (int)ProvvedimentiEnum.AumentoLivelloNoAssorbimento)
            if (prov.XR_PRV_PROV.SIGLA == PoliticheRetributiveHelper.SIGLA_PROMOZIONI)
            {
                if (!String.IsNullOrWhiteSpace(pratica.CAT_RICHIESTA))
                    qual = pratica.CAT_RICHIESTA;
                else
                    qual = prov.CAT_PREVISTA;
            }

            XR_PRV_DIPENDENTI_DOC doc = db.XR_PRV_DIPENDENTI_DOC.FirstOrDefault(x => x.ID_DIPENDENTE == pratica.ID_DIPENDENTE);
            //lettera.HeaderText = pratica.XR_PRV_DIREZIONE.PROTOCOLLO;
            lettera.HeaderText = "";

            if (pratica.ID_TEMPLATE.GetValueOrDefault() > 0)
            {
                var template = templates.FirstOrDefault(x => x.ID_TEMPLATE == pratica.ID_TEMPLATE.Value);
                lettera.BodyText = template.TEMPLATE_TEXT;
            }
            lettera.FooterText = GetTemplateText(templates, 0, "FOOTER", qual, codDir);

            if (doc != null)
            {
                if (doc.IND_HEADER) lettera.HeaderText = doc.HEADER_TEXT;
                if (doc.IND_BODY) lettera.BodyText = doc.BODY_TEXT;
                if (doc.IND_FOOTER) lettera.FooterText = doc.FOOTER_TEXT;
            }

            bool forzaFirma = false;
            bool firmaBalzola = false;
            XR_PRV_TEMPLATE signTemplate = null;

            var parametroFirmaPDF = CommonHelper.GetParametri<string>(EnumParametriSistema.PoliticheRetributive_CodiceFirmaPDF);
            bool firmaPresa = false;
            if (parametroFirmaPDF != null && parametroFirmaPDF.Any())
            {
                string codiceFirmaPDF = parametroFirmaPDF[0];
                if (!string.IsNullOrWhiteSpace(codiceFirmaPDF))
                {
                    var tmp = templates.Where(x => x.ID_PROV != null && x.NOME == codiceFirmaPDF).FirstOrDefault();
                    if (tmp != null)
                    {
                        signTemplate = tmp;
                        firmaPresa = true;
                        forzaFirma = true;
                    }
                }
            }
            if (!firmaPresa)
            {

                var parametroFirmaBalzola = CommonHelper.GetParametri<string>(EnumParametriSistema.PoliticheRetributive_FirmaBalzolaSeProdTV);
                firmaBalzola = bool.Parse(parametroFirmaBalzola[0]);

                // se è abilitato il controllo su firma balzola per la produzione
                if (firmaBalzola && pratica.XR_PRV_DIREZIONE.XR_PRV_AREA.NOME.Trim().ToUpper() == "AREA PRODUZIONE")
                {
                    var tmp = templates.Where(x => x.ID_PROV != null && x.NOME == "SIGN SB").FirstOrDefault();
                    signTemplate = tmp;

                    if (signTemplate == null)
                    {
                        signTemplate = GetTemplate(templates, prov.ID_PROV, "SIGN", qual, codDir);
                    }
                }
                else
                {
                    var parametroFirma = CommonHelper.GetParametri<string>(EnumParametriSistema.PoliticheRetributive_ForzaFirmaVenturaNelPDF);
                    if (parametroFirma != null && parametroFirma.Any())
                    {
                        if (!String.IsNullOrEmpty(parametroFirma[0]))
                        {
                            forzaFirma = bool.Parse(parametroFirma[0]);
                            if (forzaFirma)
                            //&& pratica.ID_TEMPLATE.GetValueOrDefault() != 2093317557)
                            {
                                //var tmp = templates.Where(x => x.ID_PROV != null && x.ID_PROV == prov.ID_PROV);
                                //signTemplate = tmp.FirstOrDefault(x => x.NOME == "SIGN FV");

                                var tmp = templates.Where(x => x.ID_PROV != null && x.NOME == "SIGN FV").FirstOrDefault();
                                signTemplate = tmp;

                                if (signTemplate == null)
                                {
                                    signTemplate = GetTemplate(templates, prov.ID_PROV, "SIGN", qual, codDir);
                                }
                            }
                            else
                            {
                                signTemplate = GetTemplate(templates, prov.ID_PROV, "SIGN", qual, codDir);
                            }

                            //int idprov = 0;
                            //if (parametroFirma[0].Contains("*"))
                            //{
                            //    // se contiene * vorrà dire che la firma riguarda tutte le tipologie
                            //    // di provvedimento
                            //    var tmp = templates.Where(x => x.ID_PROV != null && x.ID_PROV == prov.ID_PROV);
                            //    signTemplate = tmp.FirstOrDefault(x => x.NOME == "SIGN FV");
                            //}
                            //else if (parametroFirma[0].Contains(","))
                            //{
                            //    // se contiene , allora è una lista di tipi provvedimento
                            //}
                            //else
                            //{
                            //    // è un solo provvedimento
                            //    idprov = int.Parse(parametroFirma[0]);
                            //    var tmp = templates.Where(x => x.ID_PROV != null && x.ID_PROV == idprov);
                            //    signTemplate = tmp.FirstOrDefault(x => x.NOME == "SIGN FV");
                            //}
                        }
                    }
                    else
                    {
                        signTemplate = GetTemplate(templates, prov.ID_PROV, "SIGN", qual, codDir);
                    }
                }
            }

            if (signTemplate != null)
            {
                lettera.SignText = signTemplate.TEMPLATE_TEXT;
                if (loadSignImage)
                {
                    lettera.SignImage = GetImageDoc(signTemplate.TEMPLATE);
                    lettera.SignImageByte = signTemplate.TEMPLATE;
                }
            }

            // FRANCESCO TEMPORANEO VA RIVISTO ALTRIMENTI SE QUESTO PARAMETRO E' TRUE SI PERDONO LE ALTRE COSE 
            // COME DATA E PROTOCOLLO
            if (doc != null &&
                doc.DATA.HasValue && !forzaFirma)
            {
                lettera.Data = doc.DATA.GetValueOrDefault();
            }
            else
            {
                string dataStampa = "";
                dataStampa = CommonHelper.GetParametro<string>(EnumParametriSistema.PoliticheRetributive_DataStampataNelPDF);
                if (String.IsNullOrEmpty(dataStampa) || dataStampa == "TODAY")
                {
                    lettera.Data = DateTime.Today;
                }
                else
                {
                    DateTime dtTemp;
                    if (!DateTime.TryParseExact(dataStampa, "dd/MM/yyyy", null, DateTimeStyles.None, out dtTemp))
                    {
                        dtTemp = DateTime.Today;
                    }
                    lettera.Data = dtTemp;
                }
            }

            if (String.IsNullOrEmpty(lettera.HeaderText) || forzaFirma)
            {
                var parametro = CommonHelper.GetParametri<string>(EnumParametriSistema.PoliticheRetributive_ProtocolloStampatoNelPDF);
                if (parametro != null && parametro.Any())
                {
                    if (!String.IsNullOrEmpty(parametro[0]))
                    {
                        if (qual != null && qual.ToUpper() == "UNDEFINED")
                        {
                            // va caricata la qualifica giusta
                            var variazione = db.XR_PRV_DIPENDENTI_VARIAZIONI.Where(w => w.ID_DIPENDENTE == pratica.ID_DIPENDENTE &&
                            w.ID_PROV == pratica.ID_PROV_EFFETTIVO).FirstOrDefault();

                            if (variazione != null)
                            {
                                pratica.CAT_RICHIESTA = variazione.CAT_PREVISTA;
                                qual = variazione.CAT_PREVISTA;
                            }
                        }

                        string catIncluse = parametro[1];
                        if (!String.IsNullOrEmpty(catIncluse) &&
                            (catIncluse.Contains("*") ||
                            catIncluse.Contains(qual)))
                        {
                            string txProtocollo = parametro[0];

                            if (txProtocollo.Contains("MATRICOLA"))
                            {
                                txProtocollo = txProtocollo.Replace("MATRICOLA", pratica.MATRICOLA);
                            }
                            lettera.HeaderText = txProtocollo;
                        }
                    }
                }
            }

            return lettera;
        }

        public static string GetTemplateText(List<XR_PRV_TEMPLATE> templates, int idProv, string contentType, string qual, string codDir)
        {
            string templateText = "";
            XR_PRV_TEMPLATE template = GetTemplate(templates, idProv, contentType, qual, codDir);

            if (template != null)
                templateText = template.TEMPLATE_TEXT;

            return templateText;
        }

        public static XR_PRV_TEMPLATE GetTemplate(List<XR_PRV_TEMPLATE> templates, int idProv, string contentType, string qual, string codDir)
        {
            XR_PRV_TEMPLATE template = null;

            IQueryable<XR_PRV_TEMPLATE> tmp = templates.AsQueryable();
            switch (contentType)
            {
                case "HEADER":
                    tmp = tmp.Where(x => x.IND_HEADER);
                    break;
                case "FOOTER":
                    tmp = tmp.Where(x => x.IND_FOOTER);
                    break;
                case "BODY":
                    tmp = tmp.Where(x => x.IND_BODY);
                    break;
                case "SIGN":
                    tmp = tmp.Where(x => x.IND_SIGN);
                    break;
                default:
                    tmp = tmp.Where(x => x.NOME == contentType);
                    break;
            }

            if (idProv > 0 && contentType != "SIGN")
                tmp = tmp.Where(x => x.ID_PROV != null && x.ID_PROV == idProv);

            string[] fSuper = CezanneHelper.GetFSuperCat();
            string f1 = "Q11,Q13,Q15";

            string qualBalzola = "Q31,Q32,Q36,Q38,Q40,Q45,Q46,Q47,Q48,Q49,Q50,Q54,Q61,S31,S32,S33,S3A,S3E,S3F,T11,T12,T13,T14,T1A,T1E,T1F,T20,T21,T22,T23,T26,T51,T52,T53,T54,T55,T61,T62,T63,T6A,T6E,T6F,V01,V02,V03,V04,V05,V0E,V10,V11,V12,V13,V1A,V1E,V1F,V40,V41,V42,V43,V44,V71,V72,V73,V74,V75,X11,X12,X13,X14,X41,X42,X43,X44,X45,X50,X51,X52,X53,X54,X5E,X5F,X61,X62,X63,X64,X6A,X6E,X6F,Y01,Y02,Y03,Y04,Y05,Z02,Z04,Z05,Z06,Z07,Z08,Z09,Z0A,Z0B,Z10,Z11,Z12,Z13,Z15,Z16,Z17,Z18,Z20,Z21,Z22,Z23,Z25,Z26,Z27,Z28,Z29,Z30,Z31,Z32,Z33,Z34,Z36,Z37,Z38,Z3A,Z59,Z5A,Z5B,Z5C,Z5D,Z64,Z65,Z66,Z84,Z85,Z86,Z90,Z96,Z97,Z98";
            string dirBalzola = "41,71,73,79,82";

            if (contentType == "SIGN")
            {
                //if (codDir == "09")
                //{
                //    //template = tmp.FirstOrDefault(x => x.NOME == "SIGN FV");

                //}
                //else 
                if (fSuper.Contains(qual))
                {
                    if (idProv == 1 || idProv == 4)
                        template = tmp.FirstOrDefault(x => x.NOME == "SIGN FV");
                    else
                        template = tmp.FirstOrDefault(x => x.NOME == "SIGN FV");
                }
                else if ((f1.Contains(qual) && (idProv == 1 || idProv == 4)))
                {
                    //template = tmp.FirstOrDefault(x => x.NOME == "SIGN FV");
                    template = tmp.FirstOrDefault(x => x.NOME == "SIGN FDL");
                }
                else if (qualBalzola.Contains(qual) && dirBalzola.Contains(codDir)) //((idProv == 1 || idProv == 4) && qualBalzola.Contains(qual) && dirBalzola.Contains(codDir))
                {
                    template = tmp.FirstOrDefault(x => x.NOME == "SIGN SB");
                }
                else
                {
                    template = tmp.FirstOrDefault(x => x.NOME == "SIGN FDL");
                }
            }
            else
            {
                foreach (var item in tmp)
                {
                    if (
                        (item.CAT_INCLUSE == null || item.CAT_INCLUSE.Split(',').Any(x => qual.StartsWith(x))) && (item.CAT_ESCLUSE == null || !item.CAT_ESCLUSE.Split(',').Any(x => qual.StartsWith(x)))
                        && (item.DIR_INCLUSE == null || item.DIR_INCLUSE.Split(',').Contains(codDir)) && (item.DIR_ESCLUSE == null || !item.DIR_ESCLUSE.Split(',').Contains(codDir))
                        )
                    {
                        template = item;
                        break;
                    }
                }
            }

            return template;
        }

        public static List<SelectListItem> GetPassaggiDisponibiliPerCategoriaPartenzaSelectList(string matricola, string catDaSelez)
        {
            List<SelectListItem> LS = new List<SelectListItem>();
            var db = new IncentiviEntities();
            var catPartenza = db.SINTESI1.Where(x => x.COD_MATLIBROMAT == matricola).Select(x => x.COD_QUALIFICA).FirstOrDefault();
            if (catPartenza == null)
                return LS;

            var items = GetPassaggiDisponibiliPerCategoriaPartenza(catPartenza);
            foreach (var item in items)
            {
                LS.Add(new SelectListItem()
                {
                    Value = item.CatArrivo,
                    Text = item.DescCatArrivo,
                    Selected = item.CatArrivo == catDaSelez
                });
            }
            return LS;
        }
        public static List<CategorieDisponibiliPassaggi> GetPassaggiDisponibiliPerCategoriaPartenza(string catPartenza)//M-POLRETR tutte le cat dal livello in su
        {
            var db = new IncentiviEntities();
            List<XR_PRV_PASSAGGI> LP = new List<XR_PRV_PASSAGGI>();

            LP = AddPassaggi(catPartenza, LP);

            var res = LP.GroupBy(x => new { x.CAT_ATTUALE, x.CAT_ARRIVO, x.LIV_ARRIVO }).Select(z => z).ToList()
                       .OrderBy(x => x.Key.LIV_ARRIVO).ToList();


            List<CategorieDisponibiliPassaggi> LD = new List<CategorieDisponibiliPassaggi>();

            foreach (var row in res)
            {
                if (LD.Select(x => x.CatArrivo).ToArray().Contains(row.Key.CAT_ARRIVO)) continue;
                if (row.Key.LIV_ARRIVO != null && row.Key.LIV_ARRIVO.Trim().ToUpper() == "DIRIGENTE") continue;

                LD.Add(new CategorieDisponibiliPassaggi()
                {
                    CatPartenza = row.Key.CAT_ATTUALE,
                    CatArrivo = row.Key.CAT_ARRIVO,
                    CatPartenzaForzata = row.Key.CAT_ATTUALE == catPartenza ? null : row.Key.CAT_ATTUALE,
                    LivArrivo = row.Key.LIV_ARRIVO,
                    OrdineLivello = GetOrdineLivello(row.Key.LIV_ARRIVO),
                    DescCatArrivo = GetDesQualifica(row.Key.CAT_ARRIVO)
                });
            }
            foreach (var p in LD.OrderByDescending(x => x.OrdineLivello))
            {
                Debug.Print(p.CatPartenza + " -> " + p.CatArrivo + "    " + p.LivArrivo + " " + p.CatPartenzaForzata);
            }
            return LD;
        }
        public static int GetOrdineLivello(string liv)
        {
            if (liv == "0A.0") return 5;
            if (liv == "DIRIGENTE") return 3;
            else return Convert.ToInt32(liv.Replace(".", ""));

        }
        public static List<XR_PRV_PASSAGGI> AddPassaggi(string catPartenza, List<XR_PRV_PASSAGGI> LP)
        {
            var db = new IncentiviEntities();
            try
            {
                foreach (var pass in db.XR_PRV_PASSAGGI.Where(x => x.CAT_ATTUALE == catPartenza && x.CAT_ARRIVO != null && x.CAT_ARRIVO != "").ToList())
                {
                    if (pass.CAT_ARRIVO != catPartenza)
                    {
                        LP.Add(pass);
                        LP = AddPassaggi(pass.CAT_ARRIVO, LP);
                    }

                }
            }
            catch (Exception ex)
            {


            }
            return LP;
        }
        public static string GetImageDoc(byte[] template)
        {
            string result = null;
            if (template != null)
                result = Convert.ToBase64String(template);
            return result;
        }
        #endregion

        public static List<System.Web.UI.WebControls.ListItem> GetStatoLetteraList()
        {
            List<System.Web.UI.WebControls.ListItem> list = new List<System.Web.UI.WebControls.ListItem>();
            list.Add(new System.Web.UI.WebControls.ListItem() { Value = "", Text = "-", Selected = true });

            IncentiviEntities db = new IncentiviEntities();
            list.AddRange(db.XR_PRV_STATI_LETTERA.ToList().Select(x => new System.Web.UI.WebControls.ListItem() { Value = x.ID_STATO_LETTERA.ToString(), Text = x.NOME }));

            return list;
        }
        public static string GetDesQualifica(string cod)
        {
            var db = new IncentiviEntities();
            string desc = db.QUALIFICA.Where(x => x.COD_QUALIFICA == cod).Select(x => x.DES_QUALIFICA).FirstOrDefault();
            if (desc == null)
                desc = cod;
            else
                desc = desc.Trim();
            return desc;
        }
        public static XR_PRV_DIPENDENTI_SIMULAZIONI GetSimulazionePending(string matricola, string catArrivo)//M-POLRETR check dalla view
        {
            var db = new IncentiviEntities();
            var sim = db.XR_PRV_DIPENDENTI_SIMULAZIONI.Where(x => x.MATRICOLA == matricola && x.CAT_ARRIVO == catArrivo).FirstOrDefault();
            return sim;
        }
        public static XR_PRV_DIPENDENTI_VARIAZIONI GetDipProvFromSimulazioni(XR_PRV_DIPENDENTI dip, int? idProv, //M-POLRETR riga per riga dalla view
            string catArrivo, int gruppo = 0, int ipotesi = 0)
        {
            var db = new IncentiviEntities();
            var sim = db.XR_PRV_DIPENDENTI_SIMULAZIONI.Where(x => x.MATRICOLA == dip.MATRICOLA && x.CAT_ARRIVO == catArrivo).FirstOrDefault();
            XR_PRV_DIPENDENTI_VARIAZIONI result = null;
            if (sim != null)
            {
                if (String.IsNullOrWhiteSpace(sim.JSON_SIMULAZIONE)) return null;

                PrevisioneConIndennita prev = Newtonsoft.Json.JsonConvert.DeserializeObject<PrevisioneConIndennita>(sim.JSON_SIMULAZIONE);
                if (prev != null)
                {

                    /* decimal passAss = variazione.Diff_ral_passaggio_assorbimento.GetValueOrDefault();
                         decimal merito = variazione.Diff_ral_merito.GetValueOrDefault();
                            decimal passaggio = variazione.Diff_ral_passaggio.GetValueOrDefault();*/

                    decimal passAss = 0;
                    if (!string.IsNullOrWhiteSpace(prev.Previsione.IMPO_SUPERMINIMO) &&
                            decimal.TryParse(prev.Previsione.IMPO_SUPERMINIMO, out decimal d2))
                    {
                        passAss = d2 / 100;
                    }
                    decimal merito = 0;
                    if (!string.IsNullOrWhiteSpace(prev.Previsione.RETRIB_MENS_PER_12) &&
                            decimal.TryParse(prev.Previsione.RETRIB_MENS_PER_12, out decimal d1))
                    {
                        merito = d1 / 100;
                    }

                    decimal passaggio = 0;
                    decimal ral_simulazione = Convert.ToDecimal(prev.Previsione.TOT_RETRIB_ANNUA) / 100;
                    decimal diff = ral_simulazione - (decimal)dip.RAL_ATTUALE;
                    passaggio = diff;

                    //if (!string.IsNullOrWhiteSpace(prev.Previsione.IMPO_DIF_CAT_SUPER) &&
                    //        decimal.TryParse(prev.Previsione.IMPO_DIF_CAT_SUPER, out decimal d3))
                    //{
                    //    passaggio = d3 / 100;
                    //}


                    result = new XR_PRV_DIPENDENTI_VARIAZIONI();


                    switch (idProv)
                    {
                        case 1:
                            result.DIFF_RAL = passaggio;
                            break;
                        case 4:
                            result.DIFF_RAL = passAss;
                            break;
                        case 2:
                            result.DIFF_RAL = merito;
                            break;
                    }

                    decimal aliq = HRDWData.GetProvvAliq(db, (int)idProv, catArrivo);
                    result.COSTO_ANNUO = result.DIFF_RAL + (result.DIFF_RAL * aliq / 100);

                    if (dip.DECORRENZA.HasValue)
                    {
                        // se la data di decorrenza è riferita ad un anno successivo al corrente
                        // allora il costo di periodo sarà pari a zero
                        DateTime oggi = DateTime.Now;
                        int annoCorrente = oggi.Year;
                        int annoSelezionato = dip.DECORRENZA.Value.Year;

                        if (annoSelezionato > annoCorrente)
                        {
                            result.COSTO_PERIODO = 0;
                            if (CommonHelper.GetParametro<bool>(EnumParametriSistema.PolRetrCostoAnnuo))
                                result.COSTO_ANNUO = result.COSTO_ANNUO / 14 * ((13 - dip.DECORRENZA.Value.Month) + Convert.ToDecimal((dip.DECORRENZA.Value.Month < 7 ? 2 : 1)));
                        }
                        else
                        {
                            result.COSTO_PERIODO = result.COSTO_ANNUO / 14 * ((13 - dip.DECORRENZA.Value.Month) + Convert.ToDecimal((dip.DECORRENZA.Value.Month < 7 ? 2.5 : 1.5)));
                        }

                        if (dip.CUSTOM_PROV.GetValueOrDefault())
                        {
                            var varManuale = PoliticheRetributiveHelper.GetDipBaseProv(dip, (int)idProv);
                            if (varManuale != null)
                            {
                                if (annoSelezionato > annoCorrente)
                                {
                                    varManuale.COSTO_PERIODO = 0;
                                    if (CommonHelper.GetParametro<bool>(EnumParametriSistema.PolRetrCostoAnnuo))
                                        varManuale.COSTO_ANNUO = result.COSTO_ANNUO / 14 * ((13 - dip.DECORRENZA.Value.Month) + Convert.ToDecimal((dip.DECORRENZA.Value.Month < 7 ? 2 : 1)));
                                }
                                else
                                {
                                    varManuale.COSTO_PERIODO = result.COSTO_ANNUO / 14 * ((13 - dip.DECORRENZA.Value.Month) + Convert.ToDecimal((dip.DECORRENZA.Value.Month < 7 ? 2.5 : 1.5)));
                                }
                                varManuale.COSTO_PERIODO = Decimal.Round(varManuale.COSTO_PERIODO, 2);
                            }
                        }
                    }
                    // }
                }
            }
            return result;
        }

        public static PrevisioneConIndennita GetDatiPrevisionaliFromJSON(string json)
        {
            if (String.IsNullOrWhiteSpace(json)) return null;

            PrevisioneConIndennita prev = Newtonsoft.Json.JsonConvert.DeserializeObject<PrevisioneConIndennita>(json);
            return prev;
        }
        public static PrevisioneConIndennita GetDatiPrevisionaliFromMatricolaCat(string matricola, string CatPartenza, string CatArrivo, bool AttendiSeMancante)
        {
            var db = new IncentiviEntities();
            var prev = db.XR_PRV_DIPENDENTI_SIMULAZIONI.Where(x => x.MATRICOLA == matricola && x.CAT_ARRIVO == CatArrivo && x.CAT_PARTENZA == CatPartenza).FirstOrDefault();
            if (prev == null)
            {
                PrevisioneConIndennita Previsione = AccodaRichiestaSimulazione(matricola, CatPartenza, CatArrivo, AttendiSeMancante);
                return Previsione;
            }
            else
            {
                return GetDatiPrevisionaliFromJSON(prev.JSON_SIMULAZIONE);
            }

        }

        private static PrevisioneConIndennita AccodaRichiestaSimulazione(string matricola, string catPartenza, string catArrivo, bool AttendiSeMancante)
        {
            var db = new IncentiviEntities();

            XR_PRV_DIPENDENTI_SIMULAZIONI Simulazione = new XR_PRV_DIPENDENTI_SIMULAZIONI()
            {
                CAT_ARRIVO = catArrivo,
                CAT_PARTENZA = catPartenza,
                MATRICOLA = matricola,
                MATR_RICHIESTA = CommonHelper.GetCurrentUserMatricola(),
                DTA_RICHIESTA = DateTime.Now
            };
            db.XR_PRV_DIPENDENTI_SIMULAZIONI.Add(Simulazione);
            db.SaveChanges();

            if (!AttendiSeMancante)
                return null;
            else
                return AttendiSimulazione(Simulazione.ID_SIMULAZIONE);
        }
        private static PrevisioneConIndennita AttendiSimulazione(int ID)
        {
            var db = new IncentiviEntities();
            DateTime Dstart = DateTime.Now;
            int TimeoutSec = CommonHelper.GetParametro<int>(EnumParametriSistema.SecondiAttesaSimulazionePolRetr);
            while (true)
            {
                var Simulazione = db.XR_PRV_DIPENDENTI_SIMULAZIONI.Where(x => x.ID_SIMULAZIONE == ID).FirstOrDefault();
                if (Simulazione.DTA_ESECUZIONE != null)
                {
                    return GetDatiPrevisionaliFromJSON(Simulazione.JSON_SIMULAZIONE);
                }
                System.Threading.Thread.Sleep(3000);
                if ((DateTime.Now - Dstart).TotalSeconds > TimeoutSec)
                {
                    Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                    {
                        provenienza = "AttendiSimulazione",
                        error_message = "Timeout attesa simulazione ID " + ID
                    });
                    return null;
                }
            }
        }
    }

    public class CategorieDisponibiliPassaggi
    {
        public string CatPartenza { get; set; }
        public string CatArrivo { get; set; }
        public string LivArrivo { get; set; }
        public string CatPartenzaForzata { get; set; }
        public int OrdineLivello { get; set; }
        public string DescCatArrivo { get; set; }
    }
    public class PdfPrinter
    {
        const int BORDER_NONE = 0;
        const int BORDER_ALL = Rectangle.LEFT_BORDER | Rectangle.TOP_BORDER | Rectangle.RIGHT_BORDER | Rectangle.BOTTOM_BORDER;

        const int fontSize = 12;
        const int fontTitleSize = 12;

        int rightAlign = PdfPCell.ALIGN_RIGHT;

        public static Image GetImage(string imagePath, bool setAbsolute = true, float width = 50f, float height = 50f)
        {
            iTextSharp.text.Image png = null;
            string pattern = @"data:image/(gif|png|jpeg|jpg);base64,";
            string imgString = Regex.Replace(imagePath, pattern, string.Empty);
            png = Image.GetInstance(Convert.FromBase64String(imgString));
            if (setAbsolute)
                png.ScaleAbsolute(width, height);
            return png;
        }

        BaseFont bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
        Font myFont;
        Font myFontBold;
        Font myFontUnderline;

        private Document _document;
        private PdfWriter _writer;
        private MemoryStream _ms;
        private string _imagePath;
        private string _imageInt;
        private string _title;

        private string _footerSoc1;
        private string _footerSoc2;

        private int _maxYPage = 100;
        private int _startX = 45;

        private int _startY = 800;

        private PdfReader reader;

        public PdfPrinter(string imagePath, string imageInt, string title, string footerSoc1, string footerSoc2, int startY = 800)
        {
            _imagePath = imagePath;
            _imageInt = imageInt;
            _title = title;

            _footerSoc1 = footerSoc1;
            _footerSoc2 = footerSoc2;

            myFont = new Font(bf, fontSize);
            myFontBold = new Font(bf, fontSize, Font.BOLD);
            myFontUnderline = new Font(bf, fontSize, Font.UNDERLINE);

            _startY = startY;
        }

        public bool Apri(byte[] baseTemplate)
        {
            bool isOpened = false;
            try
            {
                reader = new PdfReader(baseTemplate);

                _ms = new MemoryStream();
                //_document = new Document(PageSize.A4, 0f, 0f, 0f, 0f);
                _document = new Document(reader.GetPageSizeWithRotation(1));
                _writer = PdfWriter.GetInstance(_document, _ms);
                _writer.PageEvent = new ITextEvents(_imagePath, _imageInt, 0, _footerSoc1, _footerSoc2, reader, _startY);
                _document.Open();
                isOpened = true;
            }
            catch (Exception)
            {

            }

            return isOpened;
        }

        public void NewPage()
        {
            _document.NewPage();

        }

        public bool Chiudi(out MemoryStream ms)
        {
            ms = new MemoryStream();
            bool isClosed = false;

            try
            {
                _document.Close();
                _writer.Close();
                byte[] byteInfo = _ms.ToArray();
                ms.Write(byteInfo, 0, byteInfo.Length);
                ms.Position = 0;
                isClosed = true;

            }
            catch (Exception)
            {

            }

            return isClosed;
        }

        public bool AggiungiTableProtHeader(string protocollo, string denom, string nome, string cognome, string sede, bool addToday = false, string addDay = "", DateTime? dataCreazione = null)
        {
            bool isAdded = false;

            PdfContentByte cb = _writer.DirectContent;

            int currentY = 725;
            int lStartX = 34;

            if (currentY < _maxYPage)
            {
                _document.NewPage();
                currentY = ((ITextEvents)_writer.PageEvent).CurrentY;
            }

            float tableWidth = _document.PageSize.Width - lStartX * 2;

            PdfPTable tableDetail = new PdfPTable(3);
            tableDetail.DefaultCell.BorderWidth = 1;
            tableDetail.TotalWidth = _document.PageSize.Width - lStartX * 2;
            tableDetail.LockedWidth = true;
            int[] tableDetailWidth = new int[] { 200, 70, 245 };
            tableDetail.SetWidths(tableDetailWidth);

            Phrase phLeftCol = new Phrase(14, protocollo, myFont);

            Phrase phRightCol = new Phrase();
            phRightCol.Add(new Chunk(Chunk.NEWLINE));
            phRightCol.Add(new Chunk(Chunk.NEWLINE));
            phRightCol.Add(new Chunk("RISERVATA PERSONALE", myFontUnderline));
            phRightCol.Add(new Chunk(Chunk.NEWLINE));
            phRightCol.Add(new Chunk(denom + " " + nome + " ", myFont));
            phRightCol.Add(new Chunk(cognome, myFontBold));
            phRightCol.Add(new Chunk(Chunk.NEWLINE));
            phRightCol.Add(new Chunk("RAI-Radiotelevisione Italiana", myFont));

            tableDetail.AddCell(WriteCell(phLeftCol, 1, myFont));
            tableDetail.AddCell(WriteCell(" ", 1, myFont));
            tableDetail.AddCell(WriteCell(phRightCol, 1, myFont));

            tableDetail.AddCell(WriteCell(" ", 3, myFont));

            DateTime dataStampata;

            if (dataCreazione.HasValue)
            {
                dataStampata = dataCreazione.GetValueOrDefault();
            }
            else
            {
                // reperimento della data da stampare nel PDF
                var dataStampa = CommonHelper.GetParametro<string>(EnumParametriSistema.PoliticheRetributive_DataStampataNelPDF);
                if (String.IsNullOrEmpty(dataStampa) || dataStampa == "TODAY")
                {
                    dataStampata = DateTime.Today;
                }
                else
                {
                    if (!DateTime.TryParseExact(dataStampa, "dd/MM/yyyy", null, DateTimeStyles.None, out dataStampata))
                        throw new Exception("Errore in conversione data ");
                }
            }

            //dataStampata = new DateTime(2022, 8, 4);
            tableDetail.AddCell(WriteCell("Roma, " + dataStampata.ToString("dd/MM/yyyy"), 1, myFont));
            //tableDetail.AddCell(WriteCell("Roma," + (addToday ? " 18/12/2019" : !String.IsNullOrWhiteSpace(addDay) ? addDay : ""), 1, myFont));
            tableDetail.AddCell(WriteCell(" ", 1, myFont));
            tableDetail.AddCell(WriteCell(sede, 1, myFontUnderline));

            tableDetail.WriteSelectedRows(0, (tableDetail.Rows.Count + 1), lStartX, currentY, cb);

            ((ITextEvents)_writer.PageEvent).CurrentY -= (int)tableDetail.CalculateHeights();

            WriteLine("\n");
            WriteLine("\n");
            WriteLine("\n");
            WriteLine("\n");
            WriteLine("\n");
            WriteLine("\n");
            WriteLine("\n");
            WriteLine("\n");

            return isAdded;
        }

        internal static PdfPCell WriteCell(string text, int colspan, iTextSharp.text.Font f, int align = Element.ALIGN_LEFT, int borderType = BORDER_NONE)
        {
            Phrase phrase = new Phrase(text, f);

            PdfPCell cell = new PdfPCell(phrase);
            //cell.FixedHeight = 16f;
            cell.Border = borderType;
            cell.HorizontalAlignment = align;
            cell.VerticalAlignment = PdfPCell.ALIGN_TOP;
            cell.Colspan = colspan;
            cell.SetLeading(16, 0);
            return cell;
        }

        internal static PdfPCell WriteCell(Phrase phrase, int colspan, iTextSharp.text.Font f, int align = Element.ALIGN_LEFT, int borderType = BORDER_NONE)
        {
            PdfPCell cell = new PdfPCell(phrase);
            //cell.FixedHeight = 16f;
            cell.Border = borderType;
            cell.HorizontalAlignment = align;
            cell.VerticalAlignment = PdfPCell.ALIGN_TOP;
            cell.Colspan = colspan;
            cell.SetLeading(16, 0);
            return cell;
        }

        public void WriteLine(string p, bool isTitle = false)
        {
            int currentY = 800;
            int lStartX = 34;

            PdfContentByte cb = _writer.DirectContent;

            currentY = ((ITextEvents)_writer.PageEvent).CurrentY;

            if (currentY < _maxYPage)
            {
                _document.NewPage();
                currentY = ((ITextEvents)_writer.PageEvent).CurrentY;
            }

            PdfPTable tableDetail = new PdfPTable(1);
            tableDetail.DefaultCell.BorderWidth = 1;
            tableDetail.TotalWidth = _document.PageSize.Width - lStartX * 2;
            tableDetail.LockedWidth = true;
            int[] tableDetailWidth = new int[] { (int)tableDetail.TotalWidth };
            tableDetail.SetWidths(tableDetailWidth);

            tableDetail.AddCell(WriteCell(p, 1, myFont, Element.ALIGN_JUSTIFIED));

            tableDetail.WriteSelectedRows(0, (tableDetail.Rows.Count + 1), lStartX, currentY, cb);

            ((ITextEvents)_writer.PageEvent).CurrentY -= (int)tableDetail.CalculateHeights();
        }

        public void AggiungiFirma(iTextSharp.text.Image png, string signText)
        {
            int currentY = 800;
            int lStartX = _startX;

            PdfContentByte cb = _writer.DirectContent;

            currentY = ((ITextEvents)_writer.PageEvent).CurrentY;

            if (currentY < _maxYPage)
            {
                currentY = _maxYPage + 10;
            }

            Font firmaFont = new FontManager("", new BaseColor(0, 0, 153)).Normal;

            const int fontIntestazione = 12;
            BaseColor colorFirma = new BaseColor(System.Drawing.Color.Blue);
            Font myFontFirma = FontFactory.GetFont("Times-Roman", fontIntestazione, Font.NORMAL, colorFirma);

            PdfPTable table = new PdfPTable(3);
            table.DefaultCell.BorderWidth = 1;
            table.TotalWidth = _document.PageSize.Width - lStartX * 2;
            table.LockedWidth = true;
            int[] widths = new int[] { 250, 220, 30 };
            table.SetWidths(widths);

            table.AddCell(WriteCell(" ", 3, myFont));

            table.AddCell(WriteCell("", 1, myFont));
            table.AddCell(WriteCell(new Phrase(signText, myFontFirma), 1, firmaFont, Element.ALIGN_CENTER));
            table.AddCell(WriteCell("", 1, myFont));

            int tableHeight = (int)table.CalculateHeights();
            table.WriteSelectedRows(0, (table.Rows.Count + 1), lStartX, currentY, cb);
            ((ITextEvents)_writer.PageEvent).CurrentY -= (int)table.CalculateHeights();

            if (png != null)
            {
                table = new PdfPTable(2);
                table.DefaultCell.BorderWidth = 1;
                table.TotalWidth = _document.PageSize.Width - lStartX * 2;
                table.LockedWidth = true;
                widths = new int[] { 490, 30 };
                table.SetWidths(widths);

                table.AddCell(WriteCell(" ", 2, myFont));
                table.AddCell(WriteCell(" ", 2, myFont));

                PdfPCell cellImage = new PdfPCell(png);
                cellImage.Border = BORDER_NONE;
                cellImage.HorizontalAlignment = rightAlign;
                table.AddCell(cellImage);

                table.AddCell(WriteCell("", 1, myFont));
                table.AddCell(WriteCell("", 2, myFont));

                table.WriteSelectedRows(0, (table.Rows.Count + 1), lStartX, currentY, cb);
                ((ITextEvents)_writer.PageEvent).CurrentY -= (int)table.CalculateHeights();
            }
        }

        internal void AggiungiHeaderDirezione(string key, string v, int record)
        {
            int currentY = 800;
            int lStartX = _startX;

            WriteLine("\n");
            WriteLine("\n");
            WriteLine("\n");
            WriteLine("\n");

            PdfContentByte cb = _writer.DirectContent;

            currentY = ((ITextEvents)_writer.PageEvent).CurrentY;

            if (currentY < _maxYPage)
            {
                _document.NewPage();
                currentY = ((ITextEvents)_writer.PageEvent).CurrentY;
            }

            float tableWidth = _document.PageSize.Width - lStartX * 2;
            int mainCell = (int)tableWidth - 20;

            PdfPTable tableDetail = new PdfPTable(3);
            tableDetail.DefaultCell.BorderWidth = 1;
            tableDetail.TotalWidth = _document.PageSize.Width - lStartX * 2;
            tableDetail.LockedWidth = true;
            int[] tableDetailWidth = new int[] { 10, mainCell, 10 };
            tableDetail.SetWidths(tableDetailWidth);

            tableDetail.AddCell(WriteCell(" ", 1, myFont));
            tableDetail.AddCell(WriteCell(key + " - " + v + ":" + record, 1, myFont, Element.ALIGN_CENTER));
            tableDetail.AddCell(WriteCell(" ", 1, myFont));

            tableDetail.WriteSelectedRows(0, (tableDetail.Rows.Count + 1), lStartX, currentY, cb);

            ((ITextEvents)_writer.PageEvent).CurrentY -= (int)tableDetail.CalculateHeights();
        }

        internal void AggiungiTableRiepilogo(XR_PRV_DIPENDENTI pratica)
        {
            int currentY = 700;
            int lStartX = _startX;

            PdfContentByte cb = _writer.DirectContent;

            currentY = ((ITextEvents)_writer.PageEvent).CurrentY;

            if (currentY < _maxYPage)
            {
                _document.NewPage();
                AggiungiTableRiepilogoHeader();
                currentY = ((ITextEvents)_writer.PageEvent).CurrentY;

            }

            PdfPTable tableDetail = new PdfPTable(5);
            tableDetail.DefaultCell.BorderWidth = 1;

            tableDetail.LockedWidth = true;
            int[] tableDetailWidth = null;
            tableDetailWidth = new int[] { 60, 180, 40, 70, 70 };
            tableDetail.TotalWidth = 520; // _document.PageSize.Width - lStartX * 2;
            tableDetail.SetWidths(tableDetailWidth);

            string provvedimento = "";

            var prov = PoliticheRetributiveHelper.GetDipProvEffettivo(pratica);
            provvedimento = prov.XR_PRV_PROV.SIGLA;

            tableDetail.AddCell(WriteCell(pratica.MATRICOLA, 1, myFont, borderType: BORDER_ALL));
            tableDetail.AddCell(WriteCell(pratica.SINTESI1.Nominativo(), 1, myFont, borderType: BORDER_ALL));
            tableDetail.AddCell(WriteCell(provvedimento, 1, myFont, borderType: BORDER_ALL));
            tableDetail.AddCell(WriteCell(String.Format("{0:N2} €", prov.DIFF_RAL), 1, myFont, rightAlign, borderType: BORDER_ALL));
            tableDetail.AddCell(WriteCell(pratica.DECORRENZA.Value.ToString("MM/yyyy"), 1, myFont, borderType: BORDER_ALL));

            tableDetail.WriteSelectedRows(0, (tableDetail.Rows.Count + 1), lStartX, currentY, cb);

            ((ITextEvents)_writer.PageEvent).CurrentY -= (int)tableDetail.CalculateHeights();
        }

        internal void AggiungiTableRiepilogoHeader()
        {
            int currentY = 700;
            int lStartX = _startX;

            PdfContentByte cb = _writer.DirectContent;

            currentY = ((ITextEvents)_writer.PageEvent).CurrentY;

            if (currentY < _maxYPage)
            {
                _document.NewPage();
                currentY = ((ITextEvents)_writer.PageEvent).CurrentY;
            }

            PdfPTable tableDetail = new PdfPTable(5);
            tableDetail.DefaultCell.BorderWidth = 1;

            tableDetail.LockedWidth = true;
            int[] tableDetailWidth = null;
            tableDetailWidth = new int[] { 60, 180, 40, 70, 70 };
            tableDetail.TotalWidth = 520; // _document.PageSize.Width - lStartX * 2;
            tableDetail.SetWidths(tableDetailWidth);

            tableDetail.AddCell(WriteCell("Matricola", 1, myFontBold, borderType: BORDER_ALL));
            tableDetail.AddCell(WriteCell("Nominativo", 1, myFontBold, borderType: BORDER_ALL));
            tableDetail.AddCell(WriteCell("Provv.", 1, myFontBold, borderType: BORDER_ALL));
            tableDetail.AddCell(WriteCell("Importo", 1, myFontBold, borderType: BORDER_ALL));
            tableDetail.AddCell(WriteCell("Decorrenza", 1, myFontBold, borderType: BORDER_ALL));

            tableDetail.WriteSelectedRows(0, (tableDetail.Rows.Count + 1), lStartX, currentY, cb);

            ((ITextEvents)_writer.PageEvent).CurrentY -= (int)tableDetail.CalculateHeights();
        }
    }

    class ITextEvents : PdfPageEventHelper
    {
        string _imgPath;
        int _pageStart;
        string _imgInfo;

        PdfReader _reader;

        iTextSharp.text.Image _pngInfo = null;
        iTextSharp.text.Image _pngImage = null;

        string _footerSoc1;
        string _footerSoc2;

        public ITextEvents(string imgPath = "", string imgInfo = "", int pageStart = 0, string footerSoc1 = "", string footerSoc2 = "", PdfReader reader = null, int startY = 800)
        {
            this._imgPath = imgPath;
            this._pageStart = pageStart;
            this._imgInfo = imgInfo;

            if (!String.IsNullOrWhiteSpace(_imgPath))
            {
                _pngImage = PdfPrinter.GetImage(_imgPath);
            }

            if (!String.IsNullOrWhiteSpace(_imgInfo))
            {
                _pngInfo = PdfPrinter.GetImage(_imgInfo);
                _pngInfo.ScalePercent(30f);
            }

            this._footerSoc1 = footerSoc1;
            this._footerSoc2 = footerSoc2;

            _reader = reader;

            _startY = startY;
        }

        #region Fields
        private string _header;
        #endregion

        #region Properties
        public string Header
        {
            get { return _header; }
            set { _header = value; }
        }

        public int CurrentY
        {
            get
            {
                return this.currentY;
            }
            set
            {
                this.currentY = value;
            }
        }
        #endregion

        // This is the contentbyte object of the writer
        PdfContentByte cb;

        // we will put the final number of pages in a template
        PdfTemplate headerTemplate, footerTemplate;

        // This keeps track of the creation time
        DateTime PrintTime = DateTime.Now;

        private int _startY = 800;

        int currentY = 800;
        const int lStartX = 45;
        const int fontSize = 12;
        BaseFont bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1250, BaseFont.NOT_EMBEDDED);

        public override void OnStartPage(PdfWriter writer, Document document)
        {

            int intestazioneHeight = WriteIntestazione(cb, document);
            headerTemplate = cb.CreateTemplate(500, intestazioneHeight);
            cb.AddTemplate(headerTemplate, document.LeftMargin, document.PageSize.GetTop(document.TopMargin));
            writer.DirectContent.AddTemplate(writer.GetImportedPage(_reader, 1), 0, 0);
        }

        private int WriteIntestazione(PdfContentByte cb, Document document)
        {
            int _currentY = _startY;
            const int lStartX = 45;
            const int fontSize = 12;
            this.CurrentY = _currentY;
            return 0;
            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED);
            iTextSharp.text.Font myFont = new iTextSharp.text.Font(bf, fontSize, iTextSharp.text.Font.NORMAL, new BaseColor(95, 95, 95));

            // disegno del logo
            bool drawImage = false;
            if (!String.IsNullOrWhiteSpace(_imgPath))
                drawImage = true;

            PdfPTable table = new PdfPTable(2);
            table.DefaultCell.BorderWidth = 1;
            table.TotalWidth = document.PageSize.Width - 50;
            table.LockedWidth = true;
            int[] widths = new int[] { 50, 350 };
            table.SetWidths(widths);

            PdfPCell cell = null;
            if (drawImage)
            {
                PdfPCell cellImage = new PdfPCell(_pngImage);
                cellImage.Border = PdfPCell.NO_BORDER;
                cellImage.Rowspan = 3;
                cellImage.Colspan = 2;
                table.AddCell(cellImage);

                cell = new PdfPCell(new Phrase(" ", new FontManager("", BaseColor.BLACK).Normal));
                cell.Border = PdfPCell.NO_BORDER;
                cell.Colspan = 2;
                table.AddCell(cell);
            }

            cell = new PdfPCell(new Phrase(" ", new FontManager("", BaseColor.BLACK).Normal));
            cell.Border = PdfPCell.NO_BORDER;
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase(" ", new FontManager("", BaseColor.BLACK).Normal));
            cell.Border = PdfPCell.NO_BORDER;
            cell.Colspan = 2;
            table.AddCell(cell);

            table.WriteSelectedRows(0, table.Rows.Count + 1, lStartX, _currentY, cb);

            _currentY = _currentY - (int)table.CalculateHeights();

            this.CurrentY = _currentY;

            return (int)table.CalculateHeights();
        }

        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            try
            {
                // Aggiunta metadata al documento
                document.AddAuthor("digiGapp");

                document.AddCreator("digiGapp con l'ausilio di iTextSharp");

                document.AddKeywords("PDF report ordine");

                document.AddSubject("Prospetto");

                document.AddTitle("");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            cb = writer.DirectContent;
            footerTemplate = cb.CreateTemplate(50, 50);
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            base.OnEndPage(writer, document);
            //writer.DirectContent.AddTemplate(writer.GetImportedPage(_reader, 1), 0, 0);
        }

        private void AggiungiInfoSocieta(PdfWriter writer, Document document)
        {
            int currentY = 80;
            int lStartX = 45;

            PdfContentByte cb = writer.DirectContent;

            PdfPTable table = new PdfPTable(1);
            table.DefaultCell.BorderWidth = 1;
            table.TotalWidth = document.PageSize.Width - lStartX * 2;
            table.LockedWidth = true;
            int[] widths = new int[] { 500 };
            table.SetWidths(widths);

            PdfPCell cellImage = new PdfPCell(_pngInfo);
            cellImage.Border = PdfPCell.NO_BORDER;
            //cellImage.Rowspan = 3;
            cellImage.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
            table.AddCell(cellImage);

            table.WriteSelectedRows(0, (table.Rows.Count + 1), lStartX, currentY, cb);
            ((ITextEvents)writer.PageEvent).CurrentY -= (int)table.CalculateHeights();
        }

        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            base.OnCloseDocument(writer, document);

            footerTemplate.BeginText();
            footerTemplate.SetFontAndSize(bf, fontSize);
            footerTemplate.SetTextMatrix(0, 0);
            footerTemplate.ShowText("" + (writer.PageNumber));
            footerTemplate.EndText();
        }
    }
}