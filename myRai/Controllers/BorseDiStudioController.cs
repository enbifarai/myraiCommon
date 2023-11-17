using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web.Mvc;
using System.Globalization;
using myRaiData;
using ComunicaCics;
using myRai.it.rai.servizi.sendmail;
using MyRaiServiceInterface.MyRaiServiceReference1;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using iTextSharp.text.pdf;
using iTextSharp.text;
using myRaiHelper;
using myRaiCommonModel;

namespace myRai.Controllers
{
    public class BorseDiStudioController : BaseCommonController
    {
        public ActionResult Index()
        {
            try
            {
                string matricola = CommonHelper.GetCurrentUserMatricola();
                string pmatricola = CommonHelper.GetCurrentUserPMatricola();

                Index_BorseDiStudio ViewModel = new Index_BorseDiStudio();
                try { ViewModel.Storico.RichiesteBorseDiStudio = getRichiesteBorseDiStudio(matricola, pmatricola); }
                catch (Exception ex) { }
                ViewModel.Riepilogo.PuoEffettureNuovaRichiesta = getBandiApertiPerUtente(matricola).Any();
                ViewModel.Riepilogo.NumeroRichieste = ViewModel.Storico.RichiesteBorseDiStudio == null ? (int?)null : ViewModel.Storico.RichiesteBorseDiStudio.Count();

                return View(ViewModel);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public ActionResult getAllegato(int annoRif, string codFiscale)
        {
            try
            {
                var matricola = CommonHelper.GetCurrentUserMatricola();
                var allegato = getBorsaDiStudio_Allegato(matricola, annoRif.ToString(), codFiscale, 1);
                if (allegato == null)
                {
                    ViewBag.Iframe = true;
                    return View("404");
                }

                Response.AddHeader("Content-Disposition", "inline; filename=" + "Richiesta_" + annoRif + "_" + matricola + "_" + codFiscale + ".pdf");
                return File(allegato, "application/pdf");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public ActionResult getFronteSpizio(int annoRif, string codFiscale, string nominativo, bool sendmail = false)
        {
            var matricola = CommonHelper.GetCurrentUserMatricola();
            var allegato = getBorsaDiStudio_FronteSpizioPDF(matricola, annoRif, codFiscale);
            if (allegato == null)
            {
                ViewBag.Iframe = true;
                return View("404");
            }

            Response.AddHeader("Content-Disposition", "inline; filename=" + "Frontespizio_" + annoRif + "_" + matricola + "_" + codFiscale + ".pdf");
            InviaEmailBorsaDiStudio(annoRif, codFiscale, nominativo, sendmail);

            return File(allegato, "application/pdf");
        }

        private void InviaEmailBorsaDiStudio(int annoRif, string codFiscale, string nominativo, bool sendmail = true)
        {
            try
            {
                var matricola = CommonHelper.GetCurrentUserMatricola();
                var allegato = getBorsaDiStudio_FronteSpizioPDF(matricola, annoRif, codFiscale);

                if (sendmail == true)
                {
                    var AnnoAccademico = annoRif + "/" + (annoRif + 1);
                    string emailFrom;
                    using (var db = new digiGappEntities())
                    {
                        emailFrom = db.MyRai_ParametriSistema.FirstOrDefault(n => n.Chiave == "AttivitàSociali_EmailAndress").Valore1;
                    }
                    var emailTo = new string[] { "p" + matricola + "@rai.it" };

                    var emailTitle = "RICHIESTA BORSA DI STUDIO ANNO SCOLASTICO " + AnnoAccademico;
                    var emailBody = @"
                        <table>
                            <tr>
                                <td valign='top' colspan='2'>
                                    <b>RICHIESTA BORSE DI STUDIO PER FAMIGLIARI - Anno Scolastico {0}</b>
                                </td>
                            </tr>
                            <tr>
                                <td valign='top' colspan='2'>
                                    <br />
                                    <br />
                                    In allegato alla presente mail trova la richiesta in formato pdf da inviare con il documento attestante la frequenza o l'iscrizione.<br />
                                </td>
                            </tr>
                            <tr>
                                <td valign='top' colspan='2'>
                                    <br />
                                    Attenzione:<br />
                                    La presente e-mail è stata inviata da un sistema automatico.<br />
                                    Eventuali risposte alla presente non potranno essere gestite.<br />
                                </td>
                            </tr>
                        </table>";
                    emailBody = string.Format(emailBody, AnnoAccademico);

                    var emailAttach = new Dictionary<string, byte[]>();
                    var nomeAllegato = "Richiesta_BStuFam_" + matricola + "_" + nominativo.Replace(" ", "_") + ".pdf";
                    emailAttach.Add(nomeAllegato, allegato);

                    inviaEmail(emailFrom, emailTo, emailBody, emailTitle, emailAttach);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private byte[] addFiligrana(byte[] sourceFile, string stringToWriteToPdf, bool bTextSuImg = false)
        {
            PdfReader reader = new PdfReader(sourceFile);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                PdfStamper pdfStamper = new PdfStamper(reader, memoryStream);
                int i = 0;
                for (i = 1; i <= reader.NumberOfPages; i++)
                {
                    Rectangle pageSize = reader.GetPageSizeWithRotation(i);
                    PdfContentByte pdfPageContents;
                    if (bTextSuImg)
                    {
                        PdfGState trasparenza = new PdfGState();
                        trasparenza.FillOpacity = 0.3F;
                        pdfPageContents = pdfStamper.GetOverContent(i);
                        pdfPageContents.SetGState(trasparenza);
                    }
                    else
                        pdfPageContents = pdfStamper.GetUnderContent(i);
                    BaseFont baseFont__1 = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, System.Text.Encoding.ASCII.EncodingName, false);
                    pdfPageContents.SetFontAndSize(baseFont__1, 40);
                    pdfPageContents.SetRGBColorFill(160, 160, 160);
                    pdfPageContents.BeginText();
                    float textAngle = System.Convert.ToSingle(GetHypotenuseAngleInDegreesFrom(pageSize.Height, pageSize.Width));
                    pdfPageContents.ShowTextAligned(PdfContentByte.ALIGN_CENTER, stringToWriteToPdf, pageSize.Width / (float)2, pageSize.Height / (float)2, textAngle);
                    pdfPageContents.EndText();
                }
                pdfStamper.FormFlattening = true;
                pdfStamper.Close();
                return memoryStream.ToArray();
            }
        }

        private static double GetHypotenuseAngleInDegreesFrom(double opposite, double adjacent)
        {
            double radians = Math.Atan2(opposite, adjacent);
            double angle = radians * (180 / Math.PI);
            return angle;
        }

        public ActionResult removeRichiesta(DateTime dtRic, string codFiscale, string status)
        {
            try
            {
                var matricola = CommonHelper.GetCurrentUserMatricola();
                CancellaRichiestaBorsaDiStudio(matricola, dtRic, codFiscale, status);
                return new HttpStatusCodeResult(200);
            }
            catch (Exception ex)
            {
                return Http404();
            }
        }

        [HttpPost]
        public ActionResult insertRichiesta(string AnnoScolasticoRichiesta, string IstitutoRichiesta, string CodFiscaleDestinatario, string NumeroTelefono, System.Web.HttpPostedFileBase fileUpload)
        {
            byte[] fileBytes = null;
            var fileExtension = String.Empty;
            byte[] doc = null;

            if (fileUpload != null)
            {
                if (fileUpload.ContentType == "application/pdf")
                {
                    try
                    {
                        using (BinaryReader br = new BinaryReader(fileUpload.InputStream))
                        {
                            fileBytes = br.ReadBytes(fileUpload.ContentLength);
                            fileExtension = fileUpload.FileName.Substring(fileUpload.FileName.IndexOf('.') + 1).ToLowerInvariant();
                        }
                        doc = addFiligrana(fileBytes.ToArray(), String.Concat("Anno Accademico ", String.Concat("Anno Accademico ", AnnoScolasticoRichiesta)), true);
                    }
                    catch (Exception ex)
                    {
                        return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest, "<b>Spiacenti. " + ex.Message + "</b>");
                    }

                }
                else
                {
                    return new HttpStatusCodeResult(System.Net.HttpStatusCode.NotAcceptable, "<b>Il documento deve essere in formato pdf</b>");
                }
            }

            try
            {
                var matricola = CommonHelper.GetCurrentUserMatricola();
                var pmatricola = CommonHelper.GetCurrentUserPMatricola();
                CodFiscaleDestinatario = CodFiscaleDestinatario.ToUpper().Trim();

                var bando = getBandiApertiPerUtente(matricola).FirstOrDefault(n => n.annorif_BDSLav == AnnoScolasticoRichiesta);
                if (bando == null)
                { throw new InvalidOperationException("Non è possibile inserire richieste nell'anno desiderato."); }

                var familiare = getFamiliariCICS(matricola, pmatricola, int.Parse(bando.annorif_BDSLav), bando.etaMin_BDSLav.GetValueOrDefault(0), bando.etaMax_BDSLav.GetValueOrDefault(99)).FirstOrDefault(n => n.CodFiscale == CodFiscaleDestinatario);
                if (familiare == null)
                { throw new InvalidOperationException("Non ci sono familiari compatibili con i bandi attualmente aperti."); }

                var istituto = getIstituti(AnnoScolasticoRichiesta, getEta(familiare.DataNascita), matricola, pmatricola, CodFiscaleDestinatario)
                    .FirstOrDefault(n => n.Descrittiva_Bsf_Corsi == IstitutoRichiesta);
                if (istituto == null)
                { throw new InvalidOperationException("Non è possibile inserire, istituto errato oppure è già presente una richiesta destinata a questo corso scolastico."); }

                string nominativoDip;
                string sedeArcalDip;
                string categoriaArcalDip;
                getDipArcalCICS(matricola, pmatricola, out nominativoDip, out sedeArcalDip, out categoriaArcalDip);

                if (String.IsNullOrWhiteSpace(NumeroTelefono))
                {
                    using (var db = new PERSEOEntities())
                    {
                        var tel = db.VHRDWLIV1_TELEFONO_AZIENDALE.FirstOrDefault(n => n.matricola_dp == matricola);
                        if (tel == null || string.IsNullOrWhiteSpace(tel.passante) || string.IsNullOrWhiteSpace(tel.prefisso) || string.IsNullOrWhiteSpace(tel.telefono))
                        { throw new InvalidOperationException("Errore: numero di telefono aziendale non valido"); }

                        var _passante = tel.passante.Substring(0, tel.passante.Contains('-') ? tel.passante.IndexOf("-") : tel.passante.Length).Replace("/", "").Trim();

                        NumeroTelefono = _passante + " " + tel.telefono.Trim() + " (" + tel.prefisso.Trim() + ")";
                    }
                }
                var ordinamento = string.Concat("B", istituto.Anno_Bsf_Corsi.ToString(), familiare.Progressivo, istituto.Codice_Bsf_Corsi, istituto.ImportoOrigine_Bsf_Corsi);

                //se la cifra è inferiore a 5 cifre inserisco degli 0 iniziali per arrivare a 13 cifre
                if (ordinamento.Length < 13)
                {
                    for (; ordinamento.Length < 13; )
                    {
                        ordinamento = ordinamento.Insert(8, "0");
                    }
                }

                using (var db = new HRASDBEntities())
                {
                    var newRic = new T_BDSDom()
                    {
                        //file 
                        allegato1_BDSDom = doc != null ? doc : null,
                        TipoAllegato1_BDSDom = String.IsNullOrWhiteSpace(fileExtension) ? fileExtension : null,
                        //Dati Richiedente
                        matricola_BDSDom = matricola,
                        nominativoRich_BDSDom = nominativoDip,
                        tipoDip_BDSDom = categoriaArcalDip,
                        sede_BDSDom = sedeArcalDip,
                        telefono_BDSDom = NumeroTelefono,
                        //Dati Bando
                        impoSpetAssegnato_BDSDom = istituto.ImportoOrigine_Bsf_Corsi * familiare.PercentualeACarico / 100,  //importoSpetAss * percSpetAss / 100
                        impoSpettante_BDSDom = istituto.ImportoOrigine_Bsf_Corsi, //importoSpettante
                        //Dati Familiare                        
                        cfDest_BDSDom = familiare.CodFiscale,
                        nominativoDest_BDSDom = familiare.Nominativo,
                        nascitaDest_BDSDom = familiare.DataNascita.ToString("yyyyMMdd"),
                        progr_BDSDom = familiare.Progressivo,
                        perc_BDSDom = familiare.PercentualeACarico,
                        percRiassegnata_BDSDom = familiare.PercentualeACarico,
                        //Dati Istituto
                        annorif_BDSDom = istituto.Anno_Bsf_Corsi.ToString(),
                        istitutoCod_BDSDom = istituto.Codice_Bsf_Corsi,
                        istitutoDescr_BDSDom = istituto.Descrittiva_Bsf_Corsi,
                        Raggruppamento_BDSDom = istituto.Raggruppamento_Bsf_Corsi,
                        ClasseFreq_BDSDom = int.Parse(istituto.Param3_Bsf_Corsi),
                        //altri dati
                        ordinamento_BDSDom = ordinamento,
                        //Dati statici
                        TipoDest_BDSDom = "F",
                        dataApprova_BDSDom = "",
                        dataRaccolta_BDSDom = "",
                        dataRichiesta_BDSDom = DateTime.Now.ToString("yyyyMMdd"),
                        dataStato_BDSDom = DateTime.Now.ToString("yyyyMMdd"),
                        stato_BDSDom = "NUOVA RIC",
                        //stato_BDSDom = "HPSA".Contains(istituto.Codice_Bsf_Corsi) ? "NUOVA RIC" : "ATTESA CERT",
                        impoRettifAssegnato_BDSDom = 0,
                    };
                    db.T_BDSDom.Add(newRic);
                    db.SaveChanges();
                }
                InviaEmailBorsaDiStudio(int.Parse(AnnoScolasticoRichiesta), CodFiscaleDestinatario, nominativoDip);

                return Content("Richiesta di borsa di studio inserita correttamente.");
            }
            catch (InvalidOperationException ex)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest, "<b>Spiacenti. " + ex.Message + "</b>");
            }
        }

        public ActionResult createRichiesta()
        {
            try
            {
                var matricola = CommonHelper.GetCurrentUserMatricola();
                var pmatricola = CommonHelper.GetCurrentUserPMatricola();

                var periodiAperti = getBandiApertiPerUtente(matricola);
                if (periodiAperti.Any() == false)
                { throw new InvalidOperationException("Non ci sono bandi aperti a te dedicati."); }

                periodiAperti.RemoveAll(n =>
                {
                    var familiari = getFamiliariCICS(matricola, pmatricola, int.Parse(n.annorif_BDSLav), n.etaMin_BDSLav.GetValueOrDefault(0), n.etaMax_BDSLav.GetValueOrDefault(99));
                    if (familiari.Any() == false) { return true; }

                    familiari.RemoveAll(f =>
                    {
                        var istituti = getIstituti(n.annorif_BDSLav, getEta(f.DataNascita), matricola, pmatricola, f.CodFiscale);
                        return !istituti.Any();
                    });
                    return !familiari.Any();
                });

                if (periodiAperti.Any() == false)
                { throw new InvalidOperationException("Non si possiede i requisiti idonei per effetture la richiesta."); }

                var ViewModel = new NuovaRichiesta_BorseDiStudio();
                ViewModel.AnniScolasticiSelezionabili = periodiAperti.Select(n => n.annorif_BDSLav).ToList();

                string telefono;
                using (var db = new PERSEOEntities())
                {
                    var tel = db.VHRDWLIV1_TELEFONO_AZIENDALE.FirstOrDefault(n => n.matricola_dp == matricola);

                    var _passante = tel.passante.Substring(0, tel.passante.Contains('-') ? tel.passante.IndexOf("-") : tel.passante.Length).Replace("/", "").Trim();

                    telefono = _passante + " " + tel.telefono.Trim() + " (" + tel.prefisso.Trim() + ")";
                }

                ViewModel.NumeroTelefono = telefono;

                return View("~/Views/BorseDiStudio/subpartial/InsertRichiesta.cshtml", ViewModel);
            }
            catch (InvalidOperationException ex)
            {
                return new HttpStatusCodeResult(403, "<b>Spiacenti. " + ex.Message + "</b>");
            }
        }

        public ActionResult getFamiliariSelezionabili(string annoRiferimento)
        {
            try
            {
                var matricola = CommonHelper.GetCurrentUserMatricola();
                var pmatricola = CommonHelper.GetCurrentUserPMatricola();

                var periodo = getBandiApertiPerUtente(matricola).FirstOrDefault(n => n.annorif_BDSLav == annoRiferimento);
                if (periodo == null)
                { throw new InvalidOperationException("Non è possibile inserire richieste nell'anno desiderato."); }

                var familiari = getFamiliariCICS(matricola, pmatricola, int.Parse(annoRiferimento), periodo.etaMin_BDSLav ?? 0, periodo.etaMax_BDSLav ?? 99);
                if (familiari.Any() == false)
                { throw new InvalidOperationException("Non ci sono familiari a carico compatibili."); }

                return Json(familiari);
            }
            catch (InvalidOperationException ex)
            {
                return Content(ex.Message);
            }
            catch (Exception ex)
            {
                return Content("Errore operazione non riuscita");
            }
        }

        public ActionResult getIstitutiSelezionabili(string annoRiferimento, string codFiscaleDestinatario)
        {
            try
            {
                var matricola = CommonHelper.GetCurrentUserMatricola();
                var pmatricola = CommonHelper.GetCurrentUserPMatricola();

                var periodo = getBandiApertiPerUtente(matricola).FirstOrDefault(n => n.annorif_BDSLav == annoRiferimento);
                if (periodo == null)
                { throw new InvalidOperationException("Non è possibile inserire richieste nell'anno desiderato."); }

                var familiari = getFamiliariCICS(matricola, pmatricola, int.Parse(annoRiferimento), periodo.etaMin_BDSLav ?? 0, periodo.etaMax_BDSLav ?? 99);
                var familiare = familiari.FirstOrDefault(n => n.CodFiscale == codFiscaleDestinatario);
                if (familiare == null)
                { throw new InvalidOperationException("Non è possibile inserire richieste per il familiare desiderato."); }

                var istituti = getIstituti(annoRiferimento, getEta(familiare.DataNascita), matricola, pmatricola, codFiscaleDestinatario);
                var ricPregresse = getRichiesteBorseDiStudio(matricola, pmatricola)
                        .Where(n => n.CodFiscale == codFiscaleDestinatario).ToList();

                if (ricPregresse.Any(n => n.AnnoScolastico.ToString() == annoRiferimento))
                {   //Esiste già una richiesta per lo stesso anno e destinatario
                    throw new InvalidOperationException("C'è già una richiesta di borsa di studio per l'anno e codice fiscale selezionato.");
                }
                if (!istituti.Any())
                { throw new InvalidOperationException("Non ci sono istituti scolastici compatibili."); }

                return Json(new { istituti = istituti.Select(s => s.Descrittiva_Bsf_Corsi).ToList(), cod = istituti.Select(s => s.Codice_Bsf_Corsi).ToList() });
            }

            catch (InvalidOperationException ex)
            {
                //Response.StatusCode = 403;
                return new HttpStatusCodeResult(403, ex.Message);
            }
            catch (Exception ex)
            {
                //Response.StatusCode = 400;
                return new HttpStatusCodeResult(400, "Errore operazione non riuscita");
            }
        }

        private byte[] getBorsaDiStudio_Allegato(string matricolaRichiedente, string annoRiferimento, string codiceFiscaleDestinatario, int numAllegato)
        {
            try
            {
                byte[] allegato = null;
                using (var db = new HRASDBEntities())
                {
                    var bs = db.T_BDSDom.FirstOrDefault(n =>
                        n.matricola_BDSDom == matricolaRichiedente &&
                        n.annorif_BDSDom == annoRiferimento &&
                        n.cfDest_BDSDom == codiceFiscaleDestinatario);

                    if (bs != null)
                    {
                        if (numAllegato == 1 && bs.allegato1_BDSDom != null)
                        {
                            allegato = bs.allegato1_BDSDom;
                        }
                        //if (numAllegato == 2 && bs.allegato2_BDSDom != null)
                        //{
                        //    allegato = bs.allegato2_BDSDom;
                        //}
                    }
                }
                return allegato;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private byte[] getBorsaDiStudio_FronteSpizioPDF(string matricolaRichiedente, int annoRiferimento, string codiceFiscaleDestinatario)
        {
            try
            {
                var bs = new T_BDSDom();
                using (var db = new HRASDBEntities())
                {
                    string sAnnoRif = annoRiferimento.ToString();
                    bs = db.T_BDSDom.FirstOrDefault(n =>
                        n.matricola_BDSDom == matricolaRichiedente &&
                        n.annorif_BDSDom == sAnnoRif &&
                        n.cfDest_BDSDom == codiceFiscaleDestinatario);
                }
                if (bs == null) { return null; }

                var OperArcalDestR1 = "";
                var OperArcalDestR2 = "";
                var OperArcalDestR3 = "";
                var OperArcalDestR4 = "";
                using (var db = new digiGappEntities())
                {
                    OperArcalDestR1 = db.MyRai_ParametriSistema.FirstOrDefault(n => n.Chiave == "BorseDiStudio_OperatoreArcalDestR1").Valore1;
                    OperArcalDestR2 = db.MyRai_ParametriSistema.FirstOrDefault(n => n.Chiave == "BorseDiStudio_OperatoreArcalDestR2").Valore1;
                    OperArcalDestR3 = db.MyRai_ParametriSistema.FirstOrDefault(n => n.Chiave == "BorseDiStudio_OperatoreArcalDestR3").Valore1;
                    OperArcalDestR4 = db.MyRai_ParametriSistema.FirstOrDefault(n => n.Chiave == "BorseDiStudio_OperatoreArcalDestR4").Valore1;
                }

                var AnnoAccademico = annoRiferimento.ToString() + "/" + (annoRiferimento + 1).ToString();
                var nato = int.Parse(bs.cfDest_BDSDom.Substring(9, 1)) > 3 ? "nata" : "nato";


                var frontespizio = "A tal fine allega il documento originale attestante la frequenza o l'iscrizione.";
                var cod_ist = bs.istitutoCod_BDSDom;
                if (cod_ist == "H" || cod_ist == "P" || cod_ist == "A" || cod_ist == "S")
                {
                    frontespizio = "Per la seguente scuola " + bs.istitutoDescr_BDSDom + " NON è necessario inviare il frontespizio e gli allegati.";
                }

                var pdfOutput = new MemoryStream();
                var documento = new Document(PageSize.A4, 25.0F, 25.0F, 80.0F, 100.0F);
                var Scrittore = PdfWriter.GetInstance(documento, pdfOutput);
                documento.Open();

                var Chr_COURIER10 = FontFactory.GetFont(FontFactory.COURIER, 10, Font.NORMAL, BaseColor.BLACK);
                var titleFont = FontFactory.GetFont("ARIAL NARROW", 12, Font.BOLD);
                var subTitleFont = FontFactory.GetFont("ARIAL NARROW", 11, Font.NORMAL);
                var boldTableFont = FontFactory.GetFont("ARIAL NARROW", 11, Font.BOLD);
                var bodyFont = FontFactory.GetFont("ARIAL NARROW", 11, Font.NORMAL);

                PdfPCell cellaVuota = new PdfPCell(new Phrase(""));
                cellaVuota.Border = 0;

                iTextSharp.text.pdf.PdfPCell tdVuota;
                tdVuota = new iTextSharp.text.pdf.PdfPCell(new Phrase("", bodyFont));

                // ***********************************************
                // TABELLA INTESTAZIONE      
                // ***********************************************
                string strModulo = "";

                var intestazioneTabel = new PdfPTable(1);
                intestazioneTabel.WidthPercentage = 100;
                intestazioneTabel.HorizontalAlignment = 1;
                intestazioneTabel.SpacingBefore = 0;
                intestazioneTabel.SpacingAfter = 30;
                intestazioneTabel.DefaultCell.Border = 0;

                PdfPCell tdIntest;
                strModulo = "RICHIESTA BORSA DI STUDIO ANNO SCOLASTICO " + AnnoAccademico;
                tdIntest = new PdfPCell(new Phrase(strModulo, titleFont));
                tdIntest.HorizontalAlignment = 1; // Element.ALIGN_JUSTIFIED
                tdIntest.Border = 0;
                intestazioneTabel.AddCell(tdIntest);
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                
                documento.Add(intestazioneTabel);

                var datiTable = new PdfPTable(1);
                datiTable.WidthPercentage = 100;
                datiTable.HorizontalAlignment = 1;
                datiTable.SpacingBefore = 5;
                datiTable.SpacingAfter = 5;
                datiTable.DefaultCell.Padding = 1;
                datiTable.DefaultCell.Border = 0;
                datiTable.DefaultCell.FixedHeight = 50;

                PdfPCell tdLblDesc1;
                PdfPCell tdLblDesc2;
                PdfPCell tdLblDesc3;
                PdfPCell tdLblDesc4;

                tdLblDesc1 = new PdfPCell(new Phrase("Il/La sottoscritta: " + bs.nominativoRich_BDSDom.Trim() + " matricola: " + matricolaRichiedente + ", della sede " + bs.sede_BDSDom + ",", bodyFont));
                tdLblDesc1.Border = 0;
                tdLblDesc1.HorizontalAlignment = Element.ALIGN_LEFT;

                datiTable.AddCell(tdLblDesc1);

                tdLblDesc2 = new PdfPCell(new Phrase("Richiede la borsa di studio per l'anno scolastico " + AnnoAccademico + " istituto " + bs.istitutoDescr_BDSDom, bodyFont));
                tdLblDesc2.Border = 0;
                tdLblDesc2.HorizontalAlignment = Element.ALIGN_LEFT;

                datiTable.AddCell(tdLblDesc2);

                tdLblDesc3 = new PdfPCell(new Phrase("per il familiare " + bs.nominativoDest_BDSDom + " codice fiscale " + bs.cfDest_BDSDom + " " + nato + " il " + DateTime.ParseExact(bs.nascitaDest_BDSDom, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("dd/MM/yyyy"), bodyFont));
                tdLblDesc3.Border = 0;
                tdLblDesc3.HorizontalAlignment = Element.ALIGN_LEFT;

                datiTable.AddCell(tdLblDesc3);

                tdLblDesc4 = new PdfPCell(new Phrase(" ", boldTableFont));
                tdLblDesc4.Border = 0;
                tdLblDesc4.HorizontalAlignment = Element.ALIGN_LEFT;
                datiTable.AddCell(tdLblDesc4);

                tdLblDesc4 = new PdfPCell(new Phrase(frontespizio, boldTableFont));
                tdLblDesc4.Border = 0;
                tdLblDesc4.HorizontalAlignment = Element.ALIGN_LEFT;
                datiTable.AddCell(tdLblDesc4);

                tdLblDesc4 = new PdfPCell(new Phrase(" ", bodyFont));
                tdLblDesc4.Border = 0;
                tdLblDesc4.HorizontalAlignment = Element.ALIGN_LEFT;
                datiTable.AddCell(tdLblDesc4);

                tdLblDesc4 = new PdfPCell(new Phrase("Al fine di facilitare eventuali comunicazioni inserire un recapito telefonico: .......................................", bodyFont));
                tdLblDesc4.Border = 0;
                tdLblDesc4.HorizontalAlignment = Element.ALIGN_LEFT;
                datiTable.AddCell(tdLblDesc4);

                tdLblDesc4 = new PdfPCell(new Phrase(" ", boldTableFont));
                tdLblDesc4.Border = 0;
                tdLblDesc4.HorizontalAlignment = Element.ALIGN_LEFT;

                datiTable.AddCell(tdLblDesc4);

                documento.Add(datiTable);

                var nsTabel = new PdfPTable(2);

                nsTabel.WidthPercentage = 100;
                nsTabel.DefaultCell.Border = 0;
                nsTabel.HorizontalAlignment = 1;
                nsTabel.SpacingBefore = 0;
                nsTabel.SpacingAfter = 5;

                iTextSharp.text.pdf.PdfPCell tdNS1;
                tdNS1 = new iTextSharp.text.pdf.PdfPCell(new Phrase("Data richiesta elettronica: " + DateTime.ParseExact(bs.dataRichiesta_BDSDom, "yyyyMMdd", CultureInfo.InvariantCulture).ToShortDateString(), bodyFont));
                tdNS1.Border = 0;
                tdNS1.Colspan = 2;

                nsTabel.AddCell(tdNS1);

                iTextSharp.text.pdf.PdfPCell tdNS2;
                tdNS2 = new iTextSharp.text.pdf.PdfPCell(new Phrase("Documento stampato in data " + DateTime.Now.ToString("dd/MM/yyyy"), bodyFont));
                tdNS2.Border = 0;
                nsTabel.AddCell(tdNS2);

                iTextSharp.text.pdf.PdfPCell tdNS3;
                tdNS3 = new iTextSharp.text.pdf.PdfPCell(new Phrase("firma", bodyFont));
                tdNS3.HorizontalAlignment = Element.ALIGN_CENTER;
                tdNS3.Border = 0;
                nsTabel.AddCell(tdNS3);

                iTextSharp.text.pdf.PdfPCell tdCellaVuota;
                tdCellaVuota = new iTextSharp.text.pdf.PdfPCell(new Phrase(" ", bodyFont));
                tdCellaVuota.Border = 0;
                nsTabel.AddCell(tdCellaVuota);

                iTextSharp.text.pdf.PdfPCell tdNS4;
                tdNS4 = new iTextSharp.text.pdf.PdfPCell(new Phrase(bs.nominativoRich_BDSDom, bodyFont));
                tdNS4.HorizontalAlignment = Element.ALIGN_CENTER;
                tdNS4.Border = 0;
                nsTabel.AddCell(tdNS4);

                iTextSharp.text.pdf.PdfPCell tdCellaVuota2;
                tdCellaVuota2 = new iTextSharp.text.pdf.PdfPCell(new Phrase(" ", bodyFont));
                tdCellaVuota2.Border = 0;
                tdCellaVuota2.Colspan = 2;
                nsTabel.AddCell(tdCellaVuota2);

                iTextSharp.text.pdf.PdfPCell tdCellaVuota3;
                tdCellaVuota3 = new iTextSharp.text.pdf.PdfPCell(new Phrase(" ", bodyFont));
                tdCellaVuota3.Border = 0;
                tdCellaVuota3.Colspan = 2;
                nsTabel.AddCell(tdCellaVuota3);

                iTextSharp.text.pdf.PdfPCell tdCellaDestinatario1;
                tdCellaDestinatario1 = new iTextSharp.text.pdf.PdfPCell(new Phrase("Da inoltrare a:", bodyFont));
                tdCellaDestinatario1.Border = 0;
                tdCellaDestinatario1.Colspan = 2;
                nsTabel.AddCell(tdCellaDestinatario1);

                iTextSharp.text.pdf.PdfPCell tdCellaDestinatario2;
                tdCellaDestinatario2 = new iTextSharp.text.pdf.PdfPCell(new Phrase(OperArcalDestR1, bodyFont));
                tdCellaDestinatario2.Border = 0;
                tdCellaDestinatario2.Colspan = 2;
                nsTabel.AddCell(tdCellaDestinatario2);

                iTextSharp.text.pdf.PdfPCell tdCellaDestinatario3;
                tdCellaDestinatario3 = new iTextSharp.text.pdf.PdfPCell(new Phrase(OperArcalDestR2, bodyFont));
                tdCellaDestinatario3.Border = 0;
                tdCellaDestinatario3.Colspan = 2;
                nsTabel.AddCell(tdCellaDestinatario3);

                iTextSharp.text.pdf.PdfPCell tdCellaDestinatario4;
                tdCellaDestinatario4 = new iTextSharp.text.pdf.PdfPCell(new Phrase(OperArcalDestR3, bodyFont));
                tdCellaDestinatario4.Border = 0;
                tdCellaDestinatario4.Colspan = 2;
                nsTabel.AddCell(tdCellaDestinatario4);

                iTextSharp.text.pdf.PdfPCell tdCellaDestinatario5;
                tdCellaDestinatario5 = new iTextSharp.text.pdf.PdfPCell(new Phrase(OperArcalDestR4, bodyFont));
                tdCellaDestinatario5.Border = 0;
                tdCellaDestinatario5.Colspan = 2;
                nsTabel.AddCell(tdCellaDestinatario5);

                documento.Add(nsTabel);
                documento.Close();
                pdfOutput.Close();

                return pdfOutput.ToArray();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CancellaRichiestaBorsaDiStudio(string matricolaRichiedente, DateTime dataRichiesta, string codiceFiscaleDestinatario, string statusCodeRichiesta)
        {
            try
            {
                var dataric = dataRichiesta.ToString("yyyyMMdd");

                using (var db = new HRASDBEntities())
                {
                    var ric = db.T_BDSDom.FirstOrDefault(n =>
                        n.matricola_BDSDom == matricolaRichiedente &&
                            //n.annorif_BDSDom == annoRiferimento &&
                        n.dataRichiesta_BDSDom == dataric &&
                        n.cfDest_BDSDom == codiceFiscaleDestinatario &&
                        (n.stato_BDSDom == "NUOVA RIC" || n.stato_BDSDom == "ATTESA CERT"));

                    if (ric == null)
                    { throw new Exception("Elemento non trovato"); }
                    db.T_BDSDom.Remove(ric);
                    db.SaveChanges();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Recupera e unisce i dati provenienti da GAPP, CICS E SQL inerenti alle richieste di borsa di studio di un dipendente Rai.
        /// </summary>
        /// <param name="targetMatricola">Matricola dipendente Rai.</param>
        /// <param name="targetPMatricola">Utenza Rai associata alla matricola del dipendente.</param>
        /// <returns>Ottiene lo storico delle richieste di borse di studio eseguite da un dipendente Rai.</returns>
        public List<Richiesta_BorseDiStudio> getRichiesteBorseDiStudio(string targetMatricola, string targetPMatricola)
        {
            try
            {
                var bsGAPP = getRichiesteBorseDiStudioGAPP(targetMatricola);

                var bsCICS = getRichiesteBorseDiStudioCICS(targetMatricola, targetPMatricola);

                var bsSQL = getRichiesteBorseDiStudioSQL(targetMatricola);

                var richiesteBorseDiStudio = mergeRichiesteBorseDiStudio(ref bsGAPP, ref bsCICS, ref bsSQL);

                foreach (var ric in richiesteBorseDiStudio)
                {
                    var istAssociato = getIstituti(ric.AnnoScolastico.ToString()).FirstOrDefault(n => n.Anno_Bsf_Corsi == ric.AnnoScolastico.ToString() && n.Codice_Bsf_Corsi == ric.CodeIstituto);
                    if (istAssociato == null) { continue; }

                    ric.RankIstituto = istAssociato.Ord_Bsf_Corsi.Value;
                    //ric.DescrIstituto = string.IsNullOrWhiteSpace(ric.DescrIstituto) ? istAssociato.Descrittiva_Bsf_Corsi : ric.DescrIstituto;
                    ric.DescrStatus = ric.DescrStatus.Length > 1 ? ric.DescrStatus : getStatusByCode_RichiestaBorsaDiStudio(ric.DescrStatus);
                }

                richiesteBorseDiStudio = richiesteBorseDiStudio
                    .OrderByDescending(n => n.AnnoScolastico)
                    .ThenBy(n => n.Nominativo)
                    .ToList();

                return richiesteBorseDiStudio;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<Richiesta_BorseDiStudio> getRichiesteBorseDiStudioGAPP(string targetMatricola)
        {
            try
            {
                using (var db = new PERSEOEntities())
                {
                    var EntitiesGAPP = db.VHRDWLIV1_ECCEZ_BORSESTUDIO
                        .Where(n => n.matricola == targetMatricola && n.stato_eccezione == "P")
                        .ToList();

                    var richiesteGAPP = new List<Richiesta_BorseDiStudio>();

                    foreach (var ric in EntitiesGAPP)
                    {   //esplode il dato "riepilogato per quantità" nello stesso numero di entità                  
                        for (int i = 0; i < (ric.quantita_numero ?? 1); i++)
                        {
                            int? _annorif = getAnnoRifByCode_RichiestaBorsaDiStudio(ric.cod_eccez, ric.data_documento);

                            var richiestaGAPP = new Richiesta_BorseDiStudio()
                            {
                                AnnoScolastico = _annorif,
                                CodeIstituto = ric.cod_eccez.PadRight(4).Substring(3, 1).ToUpper(),
                                DescrStatus = ric.stato_eccezione,
                                CodeDocumento = null,
                                DataRichiesta = ric.data_documento,
                                DataContabile = string.IsNullOrWhiteSpace(ric.mese_contabile) ? (DateTime?)null : DateTime.ParseExact(ric.mese_contabile, "yyyyMM", CultureInfo.InvariantCulture),
                                DataApprovata = ric.data_immissione,
                                DataDiStampa = ric.data_immissione,
                                Importo = ric.importo,
                                Nominativo = null,
                            };
                            richiesteGAPP.Add(richiestaGAPP);
                        }
                    }
                    return richiesteGAPP;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<Richiesta_BorseDiStudio> getRichiesteBorseDiStudioCICS(string targetMatricola, string targetPMatricola)
        {
            try
            {
                var service = new WSDigigapp();
                var response = service.getRichiesteBorseDiStudio_CicsEntities(targetMatricola, targetPMatricola);

                if (response == null || response.Esito == false)
                { throw new Exception("Impossibile recuperare i dati da Cics: " + response.Errore); }

                if (response.UlterioriPagine != 0)
                { throw new Exception("Cics richiede la non implementata paginazione del dato"); }

                var richiesteCICS = new List<Richiesta_BorseDiStudio>();
                foreach (var item in response.Items)
                {
                    var _codeccez = item.Substring(7, 3);
                    var _datadoc = DateTime.ParseExact(item.Substring(11, 8), "yyyyMMdd", CultureInfo.InvariantCulture);
                    int? _annorif = getAnnoRifByCode_RichiestaBorsaDiStudio(_codeccez, _datadoc);

                    var richiesta = new Richiesta_BorseDiStudio()
                    {
                        AnnoScolastico = _annorif,
                        CodeIstituto = item.Substring(10, 1).ToUpper(),
                        DescrStatus = item.Substring(27, 1),
                        CodeDocumento = item.Substring(21, 6).Trim().ToUpper(),
                        DataRichiesta = _datadoc,
                        DataApprovata = string.IsNullOrWhiteSpace(item.Substring(28, 8)) || item.Substring(28, 8).Distinct().Count()==1 ? (DateTime?)null : DateTime.ParseExact(item.Substring(28, 8), "yyyyMMdd", CultureInfo.InvariantCulture),
                        DataContabile = string.IsNullOrWhiteSpace(item.Substring(84, 6)) || item.Substring(84,6).Distinct().Count() == 1 ? (DateTime?)null : DateTime.ParseExact(item.Substring(84, 6), "yyyyMM", CultureInfo.InvariantCulture),
                        DataDiStampa = string.IsNullOrWhiteSpace(item.Substring(44, 8)) || item.Substring(44, 8).Distinct().Count() == 1 ? (DateTime?)null : DateTime.ParseExact(item.Substring(44, 8), "yyyyMMdd", CultureInfo.InvariantCulture),
                        Importo = int.Parse(item.Substring(90, 7)) / 100,
                        Nominativo = null,
                    };
                    richiesteCICS.Add(richiesta);
                }
                return richiesteCICS;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<Richiesta_BorseDiStudio> getRichiesteBorseDiStudioSQL(string targetMatricola)
        {
            try
            {
                using (var db = new HRASDBEntities())
                {
                    var EntitiesSQL = db.T_BDSDom
                        .Where(n => n.matricola_BDSDom == targetMatricola)
                        .Where(n => n.stato_BDSDom != "SCADUTA")  //chiedere a Leonardo
                        .ToList();

                    var richiesteSQL = EntitiesSQL.Select(n => new Richiesta_BorseDiStudio()
                        {
                            AnnoScolastico = int.Parse(n.annorif_BDSDom),
                            DescrStatus = string.IsNullOrWhiteSpace(n.stato_BDSDom) ? "" : n.stato_BDSDom.Trim().ToUpper(),
                            CodeDocumento = string.IsNullOrWhiteSpace(n.NumDocGapp_BDSDom) ? null : n.NumDocGapp_BDSDom.Trim().ToUpper(),
                            CodeIstituto = n.istitutoCod_BDSDom == null ? null : n.istitutoCod_BDSDom.ToUpper(),
                            RankClasseFreq = n.ClasseFreq_BDSDom.GetValueOrDefault(0),
                            DataRichiesta = string.IsNullOrWhiteSpace(n.dataRichiesta_BDSDom) ? (DateTime?)null : DateTime.ParseExact(n.dataRichiesta_BDSDom, "yyyyMMdd", CultureInfo.InvariantCulture),
                            DataApprovata = string.IsNullOrWhiteSpace(n.dataApprova_BDSDom) ? (DateTime?)null : DateTime.ParseExact(n.dataApprova_BDSDom, "yyyyMMdd", CultureInfo.InvariantCulture),
                            DataContabile = string.IsNullOrWhiteSpace(n.dataRaccolta_BDSDom) ? (DateTime?)null : DateTime.ParseExact(n.dataRaccolta_BDSDom, "yyyyMMdd", CultureInfo.InvariantCulture),
                            DataDiStampa = null,
                            Importo = (n.impoSpetAssegnato_BDSDom + n.impoRettifAssegnato_BDSDom) / 100,
                            Nominativo = string.IsNullOrWhiteSpace(n.nominativoDest_BDSDom) ? null : n.nominativoDest_BDSDom.Trim(),
                            CodFiscale = string.IsNullOrWhiteSpace(n.cfDest_BDSDom) ? null : n.cfDest_BDSDom.Trim().ToUpper(),
                            HaAllegatoTipo1 = n.allegato1_BDSDom == null ? false : true,
                            HaAllegatoTipo2 = n.allegato2_BDSDom == null ? false : true,
                            DescrIstituto = n.istitutoDescr_BDSDom
                        }).ToList();

                    return richiesteSQL;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private List<Richiesta_BorseDiStudio> mergeRichiesteBorseDiStudio(ref List<Richiesta_BorseDiStudio> RicGAPP, ref List<Richiesta_BorseDiStudio> ricCICS, ref List<Richiesta_BorseDiStudio> ricSQL)
        {
            try
            {
                var richieste = new List<Richiesta_BorseDiStudio>(RicGAPP);

                //Merge GAPP->CICS
                foreach (var ric in ricCICS)
                {
                    var ToUpdate = richieste.FirstOrDefault(n =>
                        n.CodeIstituto == ric.CodeIstituto &&
                        n.AnnoScolastico == ric.AnnoScolastico &&
                        n.DataRichiesta == ric.DataRichiesta &&
                        n.CodeDocumento == null);

                    if (ToUpdate == null)
                    {
                        richieste.Add(ric);
                    }
                    else
                    {
                        ToUpdate.CodeDocumento = ric.CodeDocumento;
                        ToUpdate.DescrStatus = ric.DescrStatus;
                        ToUpdate.Importo = ric.Importo;
                    }
                }

                //Merge GAPP+CICS->SQL
                foreach (var ric in ricSQL)
                {
                    var ToUpdate = richieste.FirstOrDefault(n =>
                        n.CodeIstituto == ric.CodeIstituto &&
                        n.AnnoScolastico == ric.AnnoScolastico &&
                        n.DataApprovata == ric.DataApprovata &&
                        (n.CodeDocumento == ric.CodeDocumento ||
                        (n.CodeDocumento == null || n.Importo == ric.Importo)) &&
                        (n.Nominativo == null || n.Nominativo == ric.Nominativo));

                    if (ToUpdate == null)
                    {
                        richieste.Add(ric);
                    }
                    else
                    {
                        ToUpdate.Nominativo = ric.Nominativo;
                        ToUpdate.CodFiscale = ric.CodFiscale;
                        ToUpdate.DescrIstituto = ric.DescrIstituto;
                        ToUpdate.RankClasseFreq = ric.RankClasseFreq;
                        ToUpdate.HaAllegatoTipo1 = ric.HaAllegatoTipo1;
                        ToUpdate.HaAllegatoTipo2 = ric.HaAllegatoTipo2;
                    }
                }
                return richieste;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private T_Bsf_Corsi getIstitutoByCode_RichiestaBorsaDiStudio(int targetYear, string targetCodeIstituto)
        {
            try
            {
                using (var db = new HRASDBEntities())
                {
                    var year = targetYear.ToString();

                    var istituto = db.T_Bsf_Corsi.FirstOrDefault(n =>
                            n.Anno_Bsf_Corsi == year &&
                            n.Codice_Bsf_Corsi == targetCodeIstituto);

                    return istituto;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private string getStatusByCode_RichiestaBorsaDiStudio(string StatoGAPP)
        {
            try
            {
                string status = "";
                switch ((StatoGAPP ?? "").Trim().ToUpper())
                {
                    case "":
                        status = null;
                        break;
                    case " ":
                        status = "VALIDATA";
                        break;
                    case "D":
                        status = "DA VALIDARE";
                        break;
                    case "P":
                        status = "PAGATA";
                        break;
                    case "R":
                        status = "RIFIUTATA";
                        break;
                    case "S":
                        status = "STAMPA RIEPILOGO";
                        break;
                    case "C":
                        status = "CONVALIDATA";
                        break;
                    default:
                        status = "[" + StatoGAPP.Trim().ToUpper() + "]";
                        break;
                }
                return status;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private int? getAnnoRifByCode_RichiestaBorsaDiStudio(string cod_eccezione, DateTime? data_documento)
        {
            try
            {
                int? _annorif = null;
                if (cod_eccezione.StartsWith("BAA"))
                {
                    _annorif = data_documento.HasValue ? data_documento.Value.Year - 1 : (int?)null;
                }
                else if (int.Parse(cod_eccezione.Substring(1, 2)) < 80)
                {
                    _annorif = int.Parse("20" + cod_eccezione.Substring(1, 2));
                }
                else
                {
                    _annorif = int.Parse("19" + cod_eccezione.Substring(1, 2));
                }
                return _annorif;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private int getEta(DateTime dataRiferimento)
        {
            var now = DateTime.Now;
            var eta = DateTime.Now.Year - dataRiferimento.Year;

            if (now.Month < dataRiferimento.Month || (now.Month == dataRiferimento.Month && now.Day < dataRiferimento.Day)) { eta--; }

            return eta;
        }

        /// <summary> 
        /// Costruisce e invia una email a più destinatari con lo stesso mittente utilizzando il servizio web di RAI 
        /// </summary>
        public void inviaEmail(string mittente, string[] destinatari, string corpoMessaggio, string oggettoMessaggio = "", Dictionary<string, byte[]> NomeEContenutoAllegati = null, string[] destinatariCC = null, string[] destinatariCCN = null)
        {
            try
            {
                var email = new Email();
                email.From = mittente;
                email.toList = destinatari;
                email.ccList = destinatariCC;
                email.bccList = destinatariCCN;
                email.Subject = oggettoMessaggio;
                email.Body = corpoMessaggio;
                email.Priority = 2;
                email.ContentType = "text/html";
                email.CharSet = "Windows-1252";
                email.ContentTransferEncoding = "8bit";

                if (NomeEContenutoAllegati != null && NomeEContenutoAllegati.Count > 0)
                {
                    email.AttachementsList = NomeEContenutoAllegati
                        .Select(n => new Attachement() { AttachementName = n.Key, AttachementValue = n.Value })
                        .ToArray();
                }
                email.SendWhen = DateTime.Now.AddSeconds(2);

                var serviceLogin = new MyRai_ParametriSistema();
                using (var db = new digiGappEntities())
                {
                    serviceLogin = db.MyRai_ParametriSistema.FirstOrDefault(n => n.Chiave == "AccountUtenteServizio");
                }
                var Invia = new MailSender();
                Invia.Credentials = new System.Net.NetworkCredential(serviceLogin.Valore1, serviceLogin.Valore2, "RAI");
                Invia.Send(email);
                Invia.Dispose();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private List<T_BDSLav> getBandiApertiPerUtente(string matricola, DateTime? TestDate = null)
        {
            var stringDate = TestDate.HasValue ? TestDate.Value.ToString("yyyyMMdd") : DateTime.Now.ToString("yyyyMMdd");

            var srv = new MyRaiService1Client();
            var catDip = srv.GetRecuperaUtente(matricola, DateTime.Now.ToString("ddMMyyyy")).data.categoria.Substring(0, 1);
            catDip = catDip == "A" ? "U" : "D";

            using (var db = new HRASDBEntities())
            {
                return db.T_BDSLav.Where(n =>
                             n.stato_BDSLav == "Aperta" &&
                            (n.TipoDipendente_BDSLav == "X" || n.TipoDipendente_BDSLav == catDip) &&
                             n.dataOpen_BDSLav.CompareTo(stringDate) <= 0 &&
                             n.dataClose_BDSLav.CompareTo(stringDate) >= 0)
                    .ToList();
            }
        }

        /// <summary>
        /// Ottiene i corsi scolastici per il quale è possibile richiedere borsa di studio.
        /// </summary>
        /// <param name="annoRic">Filtra i corsi in base all'anno scolastico relativo alla richiesta.</param>
        /// <param name="etaRic">Filtra i corsi in base alla compatibilità con l'età del richiedente.</param>
        /// <param name="matrRic">Matricola del dipendente Rai richiedente.</param>
        /// <param name="pmatrRic">PMatricola del dipendete Rai richiedente.</param>
        /// <param name="codFiscaleRic">Filtra i corsi in base alle richieste già effettuate dallo stesso destinatario della richiesta.</param>
        /// <returns>Ottiene i corsi scolastici per il quale è possibile richiedere borsa di studio.</returns>
        private List<T_Bsf_Corsi> getIstituti(string annoRic, int etaRic = -1, string matrRic = null, string pmatrRic = null, string codFiscaleRic = null)
        {
            using (var db = new HRASDBEntities())
            {
                var istituti = db.T_Bsf_Corsi.Where(n => n.Anno_Bsf_Corsi == annoRic).OrderBy(n => n.Ord_Bsf_Corsi).ToList();

                if (etaRic >= 0)
                {
                    istituti = istituti.Where(n => int.Parse(n.Param1_Bsf_Corsi) <= etaRic && int.Parse(n.Param2_Bsf_Corsi) >= etaRic).ToList();
                }

                //Aggiunge i corsi delle scuole superiori che non sono presenti in tabella T_Bsf_Corsi e la rispettiva classe
                var corsi = new List<T_Bsf_Corsi>();
                foreach (var ist in istituti)
                {
                    switch (ist.Raggruppamento_Bsf_Corsi)
                    {
                        case "E":
                            ist.Param3_Bsf_Corsi = "1";
                            break;
                        case "M":
                            switch (ist.Codice_Bsf_Corsi)
                            {
                                case "P":
                                    ist.Param3_Bsf_Corsi = "1";
                                    break;
                                case "A":
                                    ist.Param3_Bsf_Corsi = "2";
                                    break;
                                case "S":
                                    ist.Param3_Bsf_Corsi = "3";
                                    break;
                                default:
                                    ist.Param3_Bsf_Corsi = null;
                                    break;
                            }
                            break;
                        case "S":
                            for (int i = 1; i <= 5; i++)
                            {
                                var newist = (T_Bsf_Corsi)db.Entry(ist).CurrentValues.ToObject();
                                newist.Descrittiva_Bsf_Corsi += " - " + i + " sup.";
                                newist.Param3_Bsf_Corsi = i.ToString();
                                corsi.Add(newist);
                            }
                            continue;
                        case "A":
                            ist.Param3_Bsf_Corsi = "0";
                            break;
                        default:
                            ist.Param3_Bsf_Corsi = null;
                            break;
                    }
                    corsi.Add(ist);
                }

                if (matrRic != null && pmatrRic != null && codFiscaleRic != null)
                {
                    var ricPregresse = getRichiesteBorseDiStudio(matrRic, pmatrRic)
                        .Where(n => n.CodFiscale == codFiscaleRic).ToList();

                    if (ricPregresse.Any(n => n.AnnoScolastico.ToString() == annoRic))
                    {   //Esiste già una richiesta per lo stesso anno e destinatario
                        return new List<T_Bsf_Corsi>();
                    }

                    var ultimoIstitutoRic = ricPregresse.Any() ? ricPregresse
                        .Max(n => n.RankIstituto) : -1;

                    var ultimaClasseRic = ricPregresse.Any() ? ricPregresse
                        .Where(n => n.RankIstituto == ultimoIstitutoRic)
                        .Max(n => n.RankClasseFreq) : -1;

                    corsi = corsi
                        .Where(n => n.Ord_Bsf_Corsi > ultimoIstitutoRic || (n.Ord_Bsf_Corsi == ultimoIstitutoRic && int.Parse(n.Param3_Bsf_Corsi) > ultimaClasseRic))
                        .ToList();
                }

                //Filtra i corsi per il quale è già stata fatta richiesta di borsa di studio
                corsi = corsi
                    .OrderBy(n => n.Ord_Bsf_Corsi)
                    .ThenBy(n => n.Param3_Bsf_Corsi).ToList();
                return corsi;
            }
        }

        private void getDipArcalCICS(string targetMatricola, string targetPMatricola, out string nominativoDip, out string SedeArcalDip, out string categoriaArcalDip)
        {
            try
            {
                var cics = new ComunicaVersoCics();
                string chiamata = string.Format("P956,ARC,HRAESS{0}   {1},RIMP", targetPMatricola, targetMatricola);
                var risposta = cics.ComunicaVersoCics(chiamata).ToString();

                if (string.IsNullOrWhiteSpace(risposta) || risposta.Length < 10 || risposta.StartsWith("ACK00") || risposta.StartsWith("ACK0") == false)
                { throw new InvalidOperationException("Risposta CICS non valida"); }

                nominativoDip = risposta.Substring(37, 30).ToUpper();
                SedeArcalDip = risposta.Substring(87, 2).Trim() + " - " + risposta.Substring(93, 8).Trim() + " (" + risposta.Substring(89, 4).Replace("ROMA", "RM").Trim() + ")";
                categoriaArcalDip = risposta.Substring(106, 1);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private List<Familiare_BorseDiStudio> getFamiliariCICS(string targetMatricola, string targetPMatricola, int annorif, int etaMin, int etaMax)
        {
            try
            {
                var cics = new ComunicaVersoCics();
                string chiamata = string.Format("P956,ARC,HRAESS{0}   {1},RIMP", targetPMatricola, targetMatricola);
                var risposta = cics.ComunicaVersoCics(chiamata).ToString();

                if (string.IsNullOrWhiteSpace(risposta) || risposta.Length < 10 || risposta.StartsWith("ACK00") || risposta.StartsWith("ACK0") == false)
                { throw new InvalidOperationException("Risposta CICS non valida"); }

                var lenghtHeader = 140;
                var lenghtFamiliare = 68;
                var lenghtFamiliareAcarico = 29;
                var numFamiliari = int.Parse(risposta.Substring(137, 3));
                var familiariRaw = risposta.Substring(lenghtHeader, (lenghtFamiliare * numFamiliari));
                var aCaricoRaw = risposta.Substring(lenghtHeader + familiariRaw.Length);

                if (familiariRaw.Length % lenghtFamiliare != 0 && aCaricoRaw.Length % lenghtFamiliareAcarico != 0)
                { throw new InvalidOperationException("Risposta CICS di lunghezza non regolare"); }

                var familiariCics = new List<string>();
                for (int i = 0; i < familiariRaw.Length; i += lenghtFamiliare)
                {
                    var familiareCics = familiariRaw.Substring(i, lenghtFamiliare);
                    familiariCics.Add(familiareCics);
                }

                var familiariACaricoCics = new List<string>();
                for (int i = 0; i < aCaricoRaw.Length; i += lenghtFamiliareAcarico)
                {
                    var familiareACaricoCics = aCaricoRaw.Substring(i, lenghtFamiliareAcarico);
                    familiariACaricoCics.Add(familiareACaricoCics);
                }

                var familiari = new List<Familiare_BorseDiStudio>();
                foreach (var fam in familiariCics)
                {
                    try
                    {
                        var dataDiNascita = DateTime.ParseExact(fam.Substring(44, 10), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        var eta = getEta(dataDiNascita);

                        if (eta < etaMin || eta > etaMax)
                        { continue; }

                        var nominativo = fam.Substring(3, 30).Trim();
                        var progressivo = fam.Substring(0, 2);
                        var famAcarico = familiariACaricoCics.FirstOrDefault(n => n.Substring(0, 2) == progressivo && n.Substring(18, 8) == dataDiNascita.ToString("yyyyMMdd"));

                        if (famAcarico == null)
                        {
                            var casoNoAimp = getCasoNoAimp(targetMatricola, annorif.ToString(), nominativo, progressivo, dataDiNascita);
                            if (casoNoAimp == null) { continue; }

                            famAcarico = casoNoAimp.ProgrAnar_BDSCaricoNoAIMP + casoNoAimp.CodFisc_BDSCaricoNoAIMP + new string(' ', 8) + casoNoAimp.PercCarico_BDSCaricoNoAIMP;
                        }

                        var familiare = new Familiare_BorseDiStudio()
                        {
                            Nominativo = nominativo.ToUpper(),
                            Progressivo = int.Parse(famAcarico.Substring(0, 2)),
                            CodFiscale = famAcarico.Substring(2, 16).Trim().ToUpper(),
                            DataNascita = dataDiNascita,
                            PercentualeACarico = int.Parse(famAcarico.Substring(26, 3)),
                        };
                        familiari.Add(familiare);
                    }
                    catch (Exception)
                    { throw; }
                }

                return familiari;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private T_BDSCaricoNoAIMP getCasoNoAimp(string matricola, string anno, string nome, string progressivo, DateTime dataDiNascita)
        {
            using (var db = new HRASDBEntities())
            {
                string datanasc = dataDiNascita.ToString("dd/MM/yyyy");

                return db.T_BDSCaricoNoAIMP.FirstOrDefault(n => n.anno_BDSCaricoNoAIMP == anno &&
                        n.matricola_BDSCaricoNoAIMP == matricola &&
                        n.NomeAnar_BDSCaricoNoAIMP == nome &&
                        n.ProgrAnar_BDSCaricoNoAIMP == progressivo &&
                        n.DataNasc_BDSCaricoNoAIMP == datanasc);
            }
        }
    }
}



