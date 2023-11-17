using iTextSharp.text;
using iTextSharp.text.pdf;
using myRai.Business;
using myRai.Data.CurriculumVitae;
using myRai.DataAccess;
using myRaiCommonModel.RaiAcademy;
using myRaiDataTalentia;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;


namespace myRaiCommonManager
{
    public class RaiAcademyManager
    {
        public static List<V_CVCorsiRai> GetCorsiFatti(string matricola)
        {
            return myRaiCommonModel.DataControllers.RaiAcademy.RaiAcademyDataController.GetVCorsiFatti(matricola);
        }
        
        public static AcademyPrivacy GetPrivacyData(string matricola)
        {
            AcademyPrivacy result = new AcademyPrivacy();

            var db = new TalentiaEntities();
            string[] paramPrivacy = CommonManager.GetParametri<string>(EnumParametriSistema.PrivacyAcademyModulo);

            if (paramPrivacy == null
                || String.IsNullOrWhiteSpace(paramPrivacy[0])
                || (!String.IsNullOrWhiteSpace(paramPrivacy[1]) && !paramPrivacy[1].Contains(matricola)))
            {
                result.Active = false;
            }
            else
            {
                result.Active = true;

                var dbR = new myRaiData.digiGappEntities();
                string sezione = CommonManager.GetSezioneContabile(matricola, true);

                string chiavePar = "TestoPrivacyAcademy";
                switch (sezione.ToUpper())
                {
                    case "B2":
                        chiavePar = "TestoPrivacyAcademyB2";
                        break;
                    case "N2":
                        chiavePar = "TestoPrivacyAcademyN2";
                        break;
                    case "C2":
                        chiavePar = "TestoPrivacyAcademyC2";
                        break;
                    default:
                        chiavePar = "TestoPrivacyAcademy";
                        break;
                }
                var pars = dbR.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == chiavePar);
                result.Text = pars != null ? pars.Valore1 : CommonManager.GetParametro<string>(EnumParametriSistema.TestoPrivacyAcademy);

                string codModulo = paramPrivacy[0];
                var privacyRecord = db.XR_MOD_DIPENDENTI.FirstOrDefault(x => x.COD_MODULO == codModulo && x.MATRICOLA == matricola);
                result.IsAccepted = privacyRecord != null && privacyRecord.DATA_COMPILAZIONE.HasValue;
                result.AcceptanceDate = privacyRecord != null ? privacyRecord.DATA_COMPILAZIONE : null;
            }

            return result;
        }

        public static bool PrivacyAcceptance(string matricola, out string errore)
        {
            bool result = false;
            errore = "";

            var db = new TalentiaEntities();
            string paramPrivacy = CommonManager.GetParametro<string>(EnumParametriSistema.PrivacyAcademyModulo);

            if (!String.IsNullOrWhiteSpace(paramPrivacy))
            {
                SINTESI1 sint = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == matricola);
                var mod = db.XR_MOD_DIPENDENTI.FirstOrDefault(x => x.COD_MODULO == paramPrivacy && x.MATRICOLA == matricola);
                if (mod != null)
                {
                    errore = "Privacy accetta il " + mod.DATA_COMPILAZIONE.Value.ToString("dd/MM/yyyy");
                }
                else
                {
                    mod = new XR_MOD_DIPENDENTI()
                    {
                        XR_MOD_DIPENDENTI1 = db.XR_MOD_DIPENDENTI.GeneraPrimaryKey(9),
                        ID_PERSONA = sint!=null?sint.ID_PERSONA:0,
                        MATRICOLA = matricola, //sint.COD_MATLIBROMAT,
                        COD_MODULO = paramPrivacy,
                        SCELTA = "true",
                        DATA_LETTURA = DateTime.Now,
                        DATA_COMPILAZIONE = DateTime.Now,
                        IND_STATO = "0",
                        COD_USER = CommonManager.GetCurrentUserPMatricola(),
                        COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress,
                        TMS_TIMESTAMP = DateTime.Now
                    };
                    db.XR_MOD_DIPENDENTI.Add(mod);

                    result = DBHelper.Save(db, "AccettazionePrivacy", CommonManager.GetCurrentUserMatricola());
                    if (!result)
                        errore = "Errore durante il salvataggio";
                }
            }
            else
            {
                errore = "Impossibile trovare il modulo di riferimento";
            }

            return result;
        }

        public static bool GetAttestato(int id, out MemoryStream ms, out string fileName, out string errorMsg)
        {
            //using (var test = new TalentiaEntities())
            //{
            //    var rec = test.CZNMMDOC.Where(w => w.NME_FILENAME == "ACA_Attestato_Completamento.pdf").FirstOrDefault();
            //    if (rec==null)
            //    {
            //        var file = File.ReadAllBytes(@"C:\Users\Nik\Desktop\ACA_Attestato_Completamento.pdf");
            //        rec = new CZNMMDOC()
            //        {
            //            ID_CZNMMDOC = test.CZNMMDOC.GeneraPrimaryKey(9),
            //            ID_DATASOURCE = null,
            //            ID_LANG = null,
            //            ID_DOCSCAT = null,
            //            NME_FILENAME= "ACA_Attestato_Completamento.pdf",
            //            DES_CZNMMDOC= "RAI_ATTESTATO_COMPLETAMENTO_CORSO",
            //            COD_EXTENSION = ".pdf",
            //            OBJ_OBJECT = file,
            //            NBR_FILESIZE = file.Length,
            //            IND_PRIVATE = "N",
            //            NME_ATTACHNAME = "ACA_Attestato_Completamento.pdf",
            //            COD_USER="ADMIN",
            //            COD_TERMID = "BATCHSESSION",
            //            TMS_TIMESTAMP = DateTime.Now,
            //            NOT_CZNMMDOC= "Attestato completamento per portale Rai Academy",
            //            NBR_SOURCETYPE = 0,
            //            ID_DATASET = null,
            //            IND_FACTORY = "N",
            //            NBR_CLEAROPTION = 0,
            //            NBR_TARGETTYPE = 0,
            //            NME_BOREPORTNAME = null,
            //            COD_INSERTUSER = null,
            //            IND_STANDALONE = "N"
            //        };
            //        test.CZNMMDOC.Add(rec);
            //        test.SaveChanges();
            //    }
            //}

            bool result = false;
            MemoryStream operMs = new MemoryStream();
            ms = new MemoryStream();
            errorMsg = "";
            fileName = "";

            int idPersona = CommonManager.GetCurrentIdPersona();

            var db = new TalentiaEntities();
            CORSO corso = db.CORSO.Find(id);
            SINTESI1 sint = db.SINTESI1.Find(idPersona);
            GENNOTES args = db.GENNOTES.FirstOrDefault(x =>x.ID_ENTITY==id && x.COD_GRPNOTE == "A003" && x.COD_TPNOTE == "ARGO");
            if (args==null)
            {
                errorMsg = "Non è presente il certificato per il corso '" + corso.COD_CORSO + "'";
                return false;
            }

            CURRFORM iscr = corso.EDIZIONE.SelectMany(x=>x.CURRFORM).FirstOrDefault(y=>y.ID_PERSONA==idPersona && y.IND_PARTICIPANT=="Y" && y.IND_STATOEVENTO=="E");
            if (iscr==null)
            {
                errorMsg = "Il corso non è stato completato";
                return false;
            }



            CZNMMDOC template = db.CZNMMDOC.Where(w => w.NME_FILENAME == "ACA_Attestato_Completamento.pdf").FirstOrDefault();
            if (template==null)
            {
                errorMsg = "Template non trovato";
                return false;
            }

            fileName = "Attestato " + corso.COD_CORSO.Replace('.', '-') + ".pdf";
            
            //byte[] template = File.ReadAllBytes(@"C:\Users\Nik\Desktop\Modello Attestato blank.pdf");
            PdfReader reader = new PdfReader(template.OBJ_OBJECT);
            Document doc = new Document(reader.GetPageSizeWithRotation(1));
            PdfWriter writer = PdfWriter.GetInstance(doc, operMs);

            doc.Open();

            writer.DirectContent.AddTemplate(writer.GetImportedPage(reader, 1), 0, 0);

            PdfContentByte cb = writer.DirectContent;

            BaseColor color = new BaseColor(System.Drawing.Color.Black);
            Font myFontIntestazione = FontFactory.GetFont("Arial", 28, Font.BOLD, color);
            Font myFontTitoloCorso = FontFactory.GetFont("Arial", 24, Font.BOLD, color);
            Font myFont = FontFactory.GetFont("Arial", 20, Font.NORMAL, color);
            Font myFontData = FontFactory.GetFont("Arial", 18, Font.NORMAL, color);
            Font myFontLF = FontFactory.GetFont("Arial", 10, Font.NORMAL, color);

            Font myFontArg = FontFactory.GetFont("Arial", 18, Font.NORMAL, color);
            Font myFontArgBoldItalic = FontFactory.GetFont("Arial", 18, Font.BOLDITALIC, color);
            Font myFontArgItalic = FontFactory.GetFont("Arial", 18, Font.ITALIC, color);


            #region Matricola
            //info matricola
            PdfPTable tableInfo = new PdfPTable(1);
            tableInfo.TotalWidth = doc.PageSize.Width;
            tableInfo.SetWidths(new float[] { doc.PageSize.Width });

            Phrase phInfo = new Phrase(20);
            phInfo.Add(new Chunk((sint.DES_COGNOMEPERS+ " " + sint.DES_NOMEPERS).ToUpper(), myFontIntestazione));
            phInfo.Add(new Phrase("\n\n", myFontLF));
            phInfo.Add(new Chunk("Matr. "+sint.COD_MATLIBROMAT, myFont));

            PdfPCell cell = new PdfPCell(phInfo);
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.Border = 0;
            tableInfo.AddCell(cell);
            tableInfo.WriteSelectedRows(0, tableInfo.Rows.Count + 1, 0, 431.5f, cb);
            #endregion

            #region Corso
            //Corso
            PdfPTable tableCorso = new PdfPTable(1);
            tableCorso.TotalWidth = doc.PageSize.Width-200;
            tableCorso.SetWidths(new float[] { doc.PageSize.Width-200 });

            Phrase phCorso = new Phrase("«" + corso.COD_CORSO + "»", myFontTitoloCorso);

            PdfPCell cellCorso = new PdfPCell(phCorso);
            cellCorso.HorizontalAlignment = Element.ALIGN_CENTER;
            cellCorso.VerticalAlignment = Element.ALIGN_MIDDLE;
            cellCorso.Border = 0;
            tableCorso.AddCell(cellCorso);
            tableCorso.WriteSelectedRows(0, tableCorso.Rows.Count + 1, 100, 322, cb);
            #endregion

            //Argomenti
            Font zapfdingbats = null;
            Chunk bullet = null;

            string arg = args.NOT_NOTE;
            if (!String.IsNullOrWhiteSpace(args.NOT_NOTE))
            {
                if (arg.Contains('$'))
                {
                    zapfdingbats = FontFactory.GetFont(FontFactory.ZAPFDINGBATS, 8);
                    bullet = new Chunk((char)108, zapfdingbats);

                    arg = arg.Replace("/$", "")
                             .Replace("$/", "");
                }
                string[] lines = arg.Split('\r');

                PdfPTable tableArg = new PdfPTable(2);
                tableArg.TotalWidth = doc.PageSize.Width - 360;
                tableArg.SetWidths(new float[] { 10, doc.PageSize.Width - 350 });

                PdfPCell headerArg = new PdfPCell(new Phrase("Argomenti trattati:", myFontArg));
                headerArg.HorizontalAlignment = Element.ALIGN_CENTER;
                headerArg.VerticalAlignment = Element.ALIGN_MIDDLE;
                headerArg.Border = 0;
                headerArg.Colspan = 2;
                tableArg.AddCell(headerArg);

                foreach (var line in lines)
                {
                    string tmpLine = line.Trim();

                    bool hasBullet = false;
                    if (tmpLine.StartsWith("$"))
                    {
                        tmpLine = tmpLine.Replace("$", "");
                        //phArg.Add(bullet);
                        PdfPCell cellBullet = new PdfPCell(new Phrase(bullet));
                        cellBullet.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                        cellBullet.Border = 0;
                        tableArg.AddCell(cellBullet);
                        hasBullet = true;
                    }

                    tmpLine = Regex.Replace(tmpLine, @"\*([^\*]*)\*", "|*$1*|");

                    Phrase phArg = new Phrase();
                    foreach (var token in tmpLine.Split('|'))
                    {
                        if (token.StartsWith("*"))
                            phArg.Add(new Chunk(token.Replace("*", ""), myFontArgBoldItalic));
                        else
                            phArg.Add(new Chunk(token, myFontArgItalic));
                    }

                    PdfPCell cellArg = new PdfPCell(phArg);
                    //cellArg.HorizontalAlignment = Element.ALIGN_CENTER;
                    cellArg.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
                    cellArg.Border = 0;
                    if (!hasBullet)
                        cellArg.Colspan = 2;
                    tableArg.AddCell(cellArg);
                }
                tableArg.WriteSelectedRows(0, tableArg.Rows.Count + 1, 180, 249, cb);
            }

            #region Data
            //Data
            PdfPTable tableData = new PdfPTable(1);
            tableData.TotalWidth = doc.PageSize.Width;
            tableData.SetWidths(new float[] { doc.PageSize.Width });

            Phrase phData = new Phrase();
            if (iscr!=null && iscr.DTA_COMPLETEDON.HasValue)
                phData.Add(new Chunk("Data: "+iscr.DTA_COMPLETEDON.Value.ToString("dd/MM/yyyy"), myFontData));

            PdfPCell cellData = new PdfPCell(phData);
            cellData.HorizontalAlignment = Element.ALIGN_CENTER;
            cellData.VerticalAlignment = Element.ALIGN_CENTER;
            cellData.Border = 0;
            tableData.AddCell(cellData);
            tableData.WriteSelectedRows(0, tableData.Rows.Count + 1, 0, 85, cb);
            #endregion

            doc.Close();

            byte[] byteInfo = operMs.ToArray();
            ms.Write(byteInfo, 0, byteInfo.Length);
            ms.Position = 0;

            result = true;

            return result;
        }
    }
}