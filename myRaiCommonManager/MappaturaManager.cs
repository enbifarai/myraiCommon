using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using myRai.DataAccess;
using myRaiCommonModel;
using myRaiData;
using myRai.Data.CurriculumVitae;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace myRaiCommonManager
{
    public class MappaturaManager
    {
        public static List<MPGGruppo> GetElencoGruppi(string codiceGruppo = "", bool loadAvailableDips = false)
        {
            var db = new cv_ModelEntities();
            List<MPGGruppo> result = new List<MPGGruppo>();
            List<MPGDipendente> dips = new List<MPGDipendente>();

            var elencoPren = db.TSVPrenElencoDip.ToList();
            var elencoSedeCont = db.DSedeContabile.ToList();
            var elencoInterv = db.Database.SqlQuery<TSVIntervScheda>("select * from TSVIntervScheda").ToList();
            var elencoCategoria = db.Database.SqlQuery<DCategoria>("select * from DCategoria").ToList();
            var elencoServizi = db.Database.SqlQuery<DServizio>("select * from DServizio").ToList();
            var elencoPrenotazioni = db.TSVPrenPrenota.ToList();
            var elencoSlot = db.TSVPrenSlot.ToList();

            if (loadAvailableDips)
            {
                string today = DateTime.Today.ToString("yyyyMMdd");

                var elenco = db.TDipendenti.Where(x => x.Flag_ultimo_record == "$"
                                && (x.Data_cessazione == null || string.Compare(today, x.Data_cessazione) > 0)
                                && (x.Categoria.StartsWith("M") || x.Categoria.StartsWith("A7"))).ToList();

                if (elenco != null)
                {
                    var tmpElenco = elenco.Select(x => new MPGDipendente()
                    {
                        Dip = x,
                        PrenDip = elencoPren.FirstOrDefault(y => y.Matricola == x.Matricola),
                        Categoria = elencoCategoria.FirstOrDefault(y => y.Codice == x.Categoria),
                        SedeContabile = elencoSedeCont.FirstOrDefault(y => y.posizione == x.Sede),
                        Intervista = elencoInterv.FirstOrDefault(y => y.Matricola == x.Matricola),
                        Prenotazione = elencoPrenotazioni.FirstOrDefault(y => y.Matricola == x.Matricola),
                        Servizio = elencoServizi.FirstOrDefault(y => y.Codice.Trim() == x.Servizio.Substring(0, 2))
                    });

                    dips.AddRange(tmpElenco.Where(x => x.Intervista == null && x.Prenotazione == null));
                }
            }

            var query = db.TSVPrenStanza.Where(x => x.CodGruppoValutati != null && x.CodGruppoValutati != "");
            if (!String.IsNullOrWhiteSpace(codiceGruppo))
                query = query.Where(x => x.CodGruppoValutati == codiceGruppo);

            foreach (var group in query.GroupBy(x => x.CodGruppoValutati))
            {
                MPGGruppo newGruppo = new MPGGruppo();
                newGruppo.Codice = group.Key;
                newGruppo.CodSedeCont = group.First().CodSedeCont;
                newGruppo.Stanze = new List<MPGStanza>();
                newGruppo.AvailableDip = new List<MPGDipendente>();

                var elencoPrenDip = elencoPren.Where(x => x.CodGruppoValutati == group.Key).ToList();
                var matricole = elencoPrenDip.Select(x => x.Matricola);

                var elencoDipGruppo = db.TDipendenti.Where(x => matricole.Contains(x.Matricola) && x.Flag_ultimo_record == "$").ToList();


                var interv = elencoInterv.Where(x => matricole.Contains(x.Matricola));
                newGruppo.PrenDip = elencoDipGruppo.Select(x => new MPGDipendente()
                {
                    Dip = x,
                    PrenDip = elencoPrenDip.FirstOrDefault(z => z.Matricola == x.Matricola),
                    Intervista = interv.FirstOrDefault(z => z.Matricola == x.Matricola),
                    Prenotazione = elencoPrenotazioni.FirstOrDefault(y => y.Matricola == x.Matricola),
                    SedeContabile = elencoSedeCont.FirstOrDefault(z => z.posizione == x.Sede),
                    Servizio = elencoServizi.FirstOrDefault(y => y.Codice.Trim() == x.Servizio.Substring(0, 2))
                }).ToList();


                foreach (var stanza in group)
                {
                    MPGStanza newStanza = new MPGStanza()
                    {
                        PrenStanza = stanza,
                        PrenElencoDips = new List<MPGDipendente>()
                    };
                    newStanza.PrenElencoDips.AddRange(newGruppo.PrenDip);
                    newStanza.PrenSlots = new List<TSVPrenSlot>();
                    newStanza.PrenSlots.AddRange(elencoSlot.Where(x => x.Id_Stanza == stanza.Id_Stanza));

                    newGruppo.Stanze.Add(newStanza);
                }

                if (loadAvailableDips)
                    newGruppo.AvailableDip.AddRange(dips);

                result.Add(newGruppo);
            }



            return result;
        }



        public static bool DisassociaDipendente(string matricola, string codicegruppo, out string result)
        {
            bool isOK = false;
            result = "";

            cv_ModelEntities db = new cv_ModelEntities();
            var prenDip = db.TSVPrenElencoDip.FirstOrDefault(x => x.Matricola == matricola);
            db.TSVPrenElencoDip.Remove(prenDip);

            isOK = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), updatePerc: false, provenienza: "Mappatura Giornalisti");
            if (!isOK)
            {
                result = "Errore durante il salvataggio dei dati";
            }

            return isOK;
        }
        public static bool DisassociaDipendenti(string elencoMatr, string codicegruppo, out string result)
        {
            bool isOK = false;
            result = "";

            cv_ModelEntities db = new cv_ModelEntities();
            var matricole = elencoMatr.Split(',').Where(x => !String.IsNullOrWhiteSpace(x));

            foreach (var matricola in matricole)
            {
                var prenDip = db.TSVPrenElencoDip.FirstOrDefault(x => x.Matricola == matricola);
                if (prenDip != null)
                    db.TSVPrenElencoDip.Remove(prenDip);
            }

            isOK = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), updatePerc: false, provenienza: "Mappatura Giornalisti");
            if (!isOK)
            {
                result = "Errore durante il salvataggio dei dati";
            }

            return isOK;
        }

        public static bool AssociaDipendente(string matricola, string codicegruppo, out string result)
        {
            bool isOK = false;
            result = "";


            bool isNew = false;
            cv_ModelEntities db = new cv_ModelEntities();
            var prenDip = db.TSVPrenElencoDip.FirstOrDefault(x => x.Matricola == matricola);
            if (prenDip == null)
            {
                isNew = true;
                prenDip = new TSVPrenElencoDip()
                {
                    Matricola = matricola,
                    TipoAgg = "I",
                    StatoPrenota = "N",
                    Utente = CommonHelper.GetCurrentUserMatricola()
                };
            }
            else
            {
                prenDip.TipoAgg = "A";
            }
            prenDip.CodGruppoValutati = codicegruppo;
            prenDip.DataOraAgg = DateTime.Now;

            if (isNew)
                db.TSVPrenElencoDip.Add(prenDip);

            isOK = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), updatePerc: false, provenienza: "Mappatura Giornalisti");
            if (!isOK)
            {
                result = "Errore durante il salvataggio dei dati";
            }

            return isOK;
        }
        public static bool AssociaDipendenti(string elencoMatr, string codicegruppo, out string result)
        {
            bool isOK = false;
            result = "";

            bool isNew = false;
            cv_ModelEntities db = new cv_ModelEntities();
            var matricole = elencoMatr.Split(',').Where(x => !String.IsNullOrWhiteSpace(x));

            foreach (var matricola in matricole)
            {
                var prenDip = db.TSVPrenElencoDip.FirstOrDefault(x => x.Matricola == matricola);
                if (prenDip == null)
                {
                    isNew = true;
                    prenDip = new TSVPrenElencoDip()
                    {
                        Matricola = matricola,
                        TipoAgg = "I",
                        StatoPrenota = "N",
                        Utente = CommonHelper.GetCurrentUserMatricola()
                    };
                }
                else
                {
                    prenDip.TipoAgg = "A";
                }
                prenDip.CodGruppoValutati = codicegruppo;
                prenDip.DataOraAgg = DateTime.Now;

                if (isNew)
                    db.TSVPrenElencoDip.Add(prenDip);
            }

            isOK = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), updatePerc: false, provenienza: "Mappatura Giornalisti");
            if (!isOK)
            {
                result = "Errore durante il salvataggio dei dati";
            }

            return isOK;
        }

        public static List<MPGStanza> GetStanzeNotAssigned()
        {
            cv_ModelEntities db = new cv_ModelEntities();
            List<MPGStanza> result = new List<MPGStanza>();

            var tmp = db.TSVPrenStanza.Where(x => x.CodGruppoValutati == null || x.CodGruppoValutati == "");

            if (tmp != null)
                result.AddRange(tmp.Select(x => new MPGStanza()
                {
                    PrenStanza = x
                }));

            return result;
        }

        public static bool EsportaAppuntamenti(string CodiceSede, string DataFrom, string DataTo, string NumeroStanza, string formato, out byte[] output, out string errorMsg)
        {
            bool result = false;
            output = null;
            errorMsg = null;

            List<MyExcelSheet> LM = new List<MyExcelSheet>();
            string criterio = "";

            using (var ent = new myRai.Data.CurriculumVitae.cv_ModelEntities())
            {
                var query = from c in ent.TSVPrenPrenota
                            join d in ent.TSVPrenSlot on c.Id_Slot equals d.Id_Slot
                            join st in ent.TSVPrenStanza on d.Id_Stanza equals st.Id_Stanza
                            orderby d.CodSedeCont, d.OrarioInizioDispo
                            select new { slot = d, prenotaz = c, stanza = st };

                if (!string.IsNullOrEmpty(CodiceSede))
                {
                    query = query.Where(x => x.slot.CodSedeCont == CodiceSede);
                    criterio += "Sede: " + ent.DSedeContabile.Where(x => x.codice == CodiceSede).Select(x => x.descri_breve).FirstOrDefault() + " - ";
                }
                if (!string.IsNullOrEmpty(DataFrom))
                {
                    DateTime D;
                    DateTime.TryParseExact(DataFrom, "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out D);
                    query = query.Where(x => x.slot.OrarioInizioDispo >= D);
                    criterio += "A partire dal: " + D.ToString("dd MMM yyyy") + " - ";
                }
                if (!string.IsNullOrEmpty(DataTo))
                {
                    DateTime D;
                    DateTime.TryParseExact(DataTo, "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out D);
                    DateTime D1 = D.AddDays(1);
                    query = query.Where(x => x.slot.OrarioInizioDispo <= D1);
                    criterio += "Fino al: " + D1.ToString("dd MMM yyyy");
                }
                if (!string.IsNullOrEmpty(NumeroStanza))
                {
                    int numStanza = Convert.ToInt32(NumeroStanza);
                    query = query.Where(x => x.stanza.Id_Stanza == numStanza);

                    var stanza = ent.TSVPrenStanza.FirstOrDefault(x => x.Id_Stanza == numStanza);

                    if (stanza == null)
                        criterio += " Stanza " + NumeroStanza;
                    else
                        criterio += " Stanza " + stanza.DesStanza;
                }

                if (criterio.EndsWith("- ")) criterio = criterio.Substring(0, criterio.Length - 2);

                List<string> matr = query.Select(x => x.prenotaz.Matricola).Distinct().ToList();

                var dipendenti = (from c in ent.TDipendenti where matr.Contains(c.Matricola) select new { mat = c.Matricola, nom = c.Nominativo }).ToList();

                string[] codicisedi = query.Select(x => x.slot.CodSedeCont).Distinct().ToArray();
                foreach (string codicesede in codicisedi)
                {
                    MyExcelSheet me = new MyExcelSheet();
                    me.Nome = ent.DSedeContabile.Where(x => x.codice == codicesede).Select(x => x.descri_breve).FirstOrDefault();
                    foreach (var q in query.Where(x => x.slot.CodSedeCont == codicesede).OrderBy(x => x.slot.OrarioInizioDispo))
                    {
                        me.Lrows.Add(new CellRow()
                        {
                            Lvalori = new List<string>() {
                                                    ((DateTime)q.slot.DataDispo).ToString("dd/MM/yyyy"),
                                                    ((DateTime)q.slot.OrarioInizioDispo).ToString("HH:mm") + "/" + ((DateTime)q.slot.OrarioFineDispo).ToString("HH:mm"),
                                                    q.stanza.DesStanza,
                                                    q.prenotaz.Matricola,
                                                    dipendenti.Where (x=>x.mat == q.prenotaz.Matricola).Select (x=>x.nom).FirstOrDefault(),
                                                    q.stanza.Link
                                                     }
                        });
                    }
                    LM.Add(me);
                }
            }

            if (LM.Count() == 0)
            {
                errorMsg = "Non ci sono prenotazioni";
            }
            else if (formato.ToLower() == "excel")
            {
                result = true;
                output = CreaExcel(LM, criterio);
            }
            else if (formato.ToLower() == "pdf")
            {
                result = true;
                output = CreaPdf(LM, criterio);
            }

            return result;
        }

        public static byte[] CreaPdf(List<MyExcelSheet> LM, string criterio)
        {
            Document document = new Document(PageSize.A4, 30f, 30f, 30f, 30f);
            Font F = new Font(Font.FontFamily.HELVETICA, 8f, 0);

            using (MemoryStream M = new MemoryStream())
            {
                PdfWriter writer = PdfWriter.GetInstance(document, M);
                writer.PageEvent = new MyPdfPageEventHelpPageNo();

                PdfPTable table = new PdfPTable(5);

                table.TotalWidth = 500f;
                table.HorizontalAlignment = Element.ALIGN_CENTER;
                table.LockedWidth = true;


                float[] widths = new float[] { 4f, 4f, 4f, 4f, 10f };
                table.SetWidths(widths);

                table.SpacingBefore = 16f;
                table.SpacingAfter = 30f;

                document.Open();
                document.Add(new Paragraph("Elenco prenotazioni ", new Font(Font.FontFamily.HELVETICA, 18)) { Alignment = Element.ALIGN_CENTER });
                document.Add(new Paragraph(" (Aggiornato al " + DateTime.Now.ToString("dd/MM/yyyy HH.mm") + ")", new Font(Font.FontFamily.HELVETICA, 8)) { Alignment = Element.ALIGN_CENTER });

                if (!string.IsNullOrEmpty(criterio))
                {
                    document.Add(new Paragraph("\r\n" + criterio, new Font(Font.FontFamily.HELVETICA, 10)) { Alignment = Element.ALIGN_CENTER });
                }
                foreach (MyExcelSheet sheet in LM)
                {
                    bool anyLink = sheet.Lrows.Any(x => x.Lvalori.Count() > 5);

                    table.AddCell(new PdfPCell(new Phrase(sheet.Nome)) { Colspan = 5, BackgroundColor = new BaseColor(230, 230, 230) });
                    table.AddCell(new PdfPCell(new Phrase("Data")) { BackgroundColor = new BaseColor(240, 240, 240) });
                    table.AddCell(new PdfPCell(new Phrase("Orario")) { BackgroundColor = new BaseColor(240, 240, 240) });
                    table.AddCell(new PdfPCell(new Phrase("Stanza")) { BackgroundColor = new BaseColor(240, 240, 240) });
                    table.AddCell(new PdfPCell(new Phrase("Matricola")) { BackgroundColor = new BaseColor(240, 240, 240) });
                    table.AddCell(new PdfPCell(new Phrase("Nominativo")) { BackgroundColor = new BaseColor(240, 240, 240) });
                    for (int i = 0; i < sheet.Lrows.Count; i++)
                    {
                        for (int stringaNumber = 0; stringaNumber < sheet.Lrows[i].Lvalori.Count; stringaNumber++)
                        {
                            if (anyLink && stringaNumber == 5)
                                continue;
                            table.AddCell(new Phrase(sheet.Lrows[i].Lvalori[stringaNumber], F));
                        }
                    }
                }
                document.Add(table);
                document.Close();
                return M.ToArray();
            }
        }

        private static byte[] CreaExcel(List<MyExcelSheet> LM, string criterio)
        {
            if (LM.Count == 0) LM.Add(new MyExcelSheet() { Nome = "Vuoto" });

            using (MemoryStream M2 = new MemoryStream())
            {
                using (XLWorkbook workbook = new XLWorkbook())
                {
                    foreach (MyExcelSheet sheet in LM)
                    {
                        var worksheet = workbook.Worksheets.Add(sheet.Nome);
                        {
                            worksheet.ColumnWidth = 20;

                            bool addLink = sheet.Lrows.Any(x => x.Lvalori.Count() > 5);

                            for (int i = 0; i < sheet.Lrows.Count; i++)
                            {
                                if (i == 0)
                                {
                                    worksheet.Cell(i + 1, 1).Value = criterio;

                                    worksheet.Cell(i + 2, 1).Value = "Data";
                                    worksheet.Cell(i + 2, 1).Style.Font.Bold = true;
                                    worksheet.Cell(i + 2, 2).Value = "Orario";
                                    worksheet.Cell(i + 2, 2).Style.Font.Bold = true;
                                    worksheet.Cell(i + 2, 3).Value = "Stanza";
                                    worksheet.Cell(i + 2, 3).Style.Font.Bold = true;
                                    worksheet.Cell(i + 2, 4).Value = "Matricola";
                                    worksheet.Cell(i + 2, 4).Style.Font.Bold = true;
                                    worksheet.Cell(i + 2, 5).Value = "Nominativo";
                                    worksheet.Cell(i + 2, 5).Style.Font.Bold = true;
                                    if (addLink)
                                    {
                                        worksheet.Cell(i + 2, 6).Value = "Link";
                                        worksheet.Cell(i + 2, 6).Style.Font.Bold = true;
                                    }
                                }
                                for (int stringaNumber = 0; stringaNumber < sheet.Lrows[i].Lvalori.Count; stringaNumber++)
                                {
                                    worksheet.Cell(i + 3, stringaNumber + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                    worksheet.Cell(i + 3, stringaNumber + 1).SetValue<string>(sheet.Lrows[i].Lvalori[stringaNumber]);
                                }
                            }
                        }
                    }
                    workbook.SaveAs(M2);
                    M2.Position = 0;
                    return M2.ToArray();
                }
            }
        }
    }

    public class MyPdfPageEventHelpPageNo : iTextSharp.text.pdf.PdfPageEventHelper
    {
        public override void OnEndPage(PdfWriter writer, Document document)
        {
            PdfContentByte cb = writer.DirectContent;
            cb.SaveState();
            string text = "Elenco prenotazioni - Pag. " + writer.PageNumber;

            Font FontPiePag = FontFactory.GetFont("Tahoma", 6, Font.ITALIC, BaseColor.BLACK);

            ColumnText ct = new ColumnText(cb);
            Phrase myText = new Phrase(text, FontPiePag);
            ct.SetSimpleColumn(myText, 10, 10, document.PageSize.Width, 20, 1, Element.ALIGN_CENTER);
            ct.Go();

            cb.RestoreState();
        }
    }
}
