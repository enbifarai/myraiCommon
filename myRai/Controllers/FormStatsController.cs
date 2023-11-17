using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using myRaiData;
using ClosedXML.Excel;
using System.IO;
using myRaiCommonModel;
using myRaiHelper;
using myRai.Data.CurriculumVitae;

namespace myRai.Controllers
{
    public class RisposteDateHelper
    {
        public static IEnumerable<MyRai_FormRisposteDate> GetElencoRisposte(IEnumerable<MyRai_FormRisposteDate> elencoRisposte, bool filtroMatricola, List<string> elencoMatricole)
        {
            if (filtroMatricola && elencoMatricole != null)
                return elencoRisposte.Where(x => elencoMatricole.Contains(x.matricola));
            else
                return elencoRisposte;
        }
    }

    public class FormStatsController : BaseCommonController
    {
        private List<Tuple<string, List<string>>> GetElencoHotelMatricole(string idHotel, MyRai_FormPrimario formprimario)
        {
            MyRai_FormDomande domandaAlbergo = formprimario.MyRai_FormSecondario.First(x => x.attivo && x.progressivo == 1)
                            .MyRai_FormDomande.First(y => y.attiva && y.progressivo == 1);

            List<Tuple<string, List<string>>> elencoHotelMatricole = new List<Tuple<string, List<string>>>();
            if (idHotel != null)
            {
                elencoHotelMatricole.Add(Tuple.Create(idHotel,
                                                domandaAlbergo.MyRai_FormRisposteDate.Where(x => x.risposta_text == idHotel)
                                                                                     .Select(y => y.matricola).ToList()));
            }
            else
            {
                elencoHotelMatricole.AddRange(domandaAlbergo.MyRai_FormRisposteDate.GroupBy(x => x.risposta_text)
                                                                                   .Select(y => Tuple.Create(y.Key, y.Select(z => z.matricola).ToList())).ToList());
            }
            return elencoHotelMatricole;
        }

        private FormStatisticsModel GetFormStatisticModel(MyRai_FormPrimario formPrimario, string[] colors, bool filtroMatricola, List<string> elencoMatricole)
        {
            List<DatoDomanda> elencoDatoDomanda = new List<DatoDomanda>();

            foreach (MyRai_FormSecondario sec in formPrimario.MyRai_FormSecondario
                .Where(x => x.attivo)
                .OrderBy(x => x.progressivo))
            {
                foreach (var dom in sec.MyRai_FormDomande.Where(x => x.attiva
                    //&& x.id_domanda_parent==null
                    )
                    .OrderBy(x => x.progressivo))
                {
                        if (dom.id_tipologia == 5 || dom.id_tipologia == 6 || dom.id_tipologia == 7) continue;

                    DatoDomanda d = new DatoDomanda();
                    d.domanda = dom;
                    d.PieItems = new List<PieItem>();
                    if (dom.id_tipologia == 1)
                    {
                        int counter = 0;
                        d.PieItems.Add(new PieItem()
                        {
                            color = colors[counter % colors.Length],
                            label = "SI",
                            data = new List<List<int>>() { new List<int>() { 1,
                                    RisposteDateHelper.GetElencoRisposte(dom.MyRai_FormRisposteDate.Where(x=>x.risposta_text=="SI"), filtroMatricola, elencoMatricole).Count()} }
                        });
                        counter++;

                        d.PieItems.Add(new PieItem()
                        {
                            color = colors[counter % colors.Length],
                            label = "NO",
                            data = new List<List<int>>() { new List<int>() { 1,
                                RisposteDateHelper.GetElencoRisposte(dom.MyRai_FormRisposteDate.Where(x=>x.risposta_text=="NO"), filtroMatricola, elencoMatricole).Count() } }
                        });
                    }
                    else
                    {
                        int counter = 0;
                        foreach (var rispostaPossibile in d.domanda.MyRai_FormRispostePossibili)
                        {
                            d.PieItems.Add(new PieItem()
                            {
                                color = colors[counter % colors.Length],
                                label = rispostaPossibile.item_risposta,
                                data = new List<List<int>>() { new List<int>() { 1, RisposteDateHelper.GetElencoRisposte(rispostaPossibile.MyRai_FormRisposteDate, filtroMatricola, elencoMatricole).Count() } }
                            });
                            counter++;
                        }
                        var risposteFreeText = RisposteDateHelper.GetElencoRisposte(d.domanda.MyRai_FormRisposteDate.Where(z => z.id_risposta == null), filtroMatricola, elencoMatricole).ToList();
                        string text = "<div style='text-align:left'>" +
                                String.Join("", risposteFreeText.Select(x => "<li>" + x.risposta_text + "</li>").ToArray())
                                + "</div>";
                        //string text = "<div style='text-align:left'><li>aaaaaaaa</li><li>bbbb</li></div>";
                        text = text.Replace('"', '\'');
                        d.PieItems.Add(new PieItem()
                        {
                            color = colors[counter % colors.Length],
                            label = "<span class='cursor' data-container='body' data-html='true' data-toggle=\"tooltip\" title=\"\" data-original-title=\"" + text + "\">Altro</span>",
                            data = new List<List<int>>() { new List<int>() { 1, risposteFreeText.Count() } }
                        });
                    }

                    d.JsonPieItems = new JavaScriptSerializer().Serialize(d.PieItems);

                    elencoDatoDomanda.Add(d);
                }
            }

            FormStatisticsModel statModel = new FormStatisticsModel();
            statModel.formprimario = formPrimario;

            foreach (var datoDomanda in elencoDatoDomanda.Where(x => x.domanda.id_domanda_parent == null))
            {
                statModel.items.Add(datoDomanda);
                statModel.items.AddRange(elencoDatoDomanda.Where(x => x.domanda.id_domanda_parent == datoDomanda.domanda.id));
            }

            return statModel;
        }

        private void FillWorkbook(MyRai_FormPrimario questionario, MyExcelWorkbook workbook, bool filtroMatricola, List<string> elencoMatricole)
        {

            foreach (MyRai_FormSecondario sec in questionario.MyRai_FormSecondario
                                                    .Where(x => x.attivo)
                                                    .OrderBy(x => x.progressivo))
            {
                foreach (MyRai_FormDomande dom in sec.MyRai_FormDomande
                                                    .Where(d => d.attiva && d.id_domanda_parent == null)
                                                    .OrderBy(d => d.progressivo))
                {
                    if (questionario.MyRai_FormTipologiaForm.tipologia == "Hotel")
                    {
                        if (sec.progressivo == 1 && dom.progressivo == 1 && dom.max_scelte == 999999) continue;
                    }

                    workbook.Add_Domanda_Titolo(dom);

                    if (dom.MyRai_FormDomande2 == null)
                    {
                        switch (dom.id_tipologia)
                        {
                            case (int)EnumTipologiaDomanda.MasterPerMatrixRating:
                                foreach (MyRai_FormDomande domSlave in dom.MyRai_FormDomande1)
                                {
                                    workbook.Add_Domanda_Titolo(domSlave);
                                    workbook.Add_Risposte_CB_RB_Tend(domSlave, filtroMatricola, elencoMatricole);
                                }
                                workbook.ro++;
                                break;

                            case (int)EnumTipologiaDomanda.ShortText:
                            case (int)EnumTipologiaDomanda.LongText:
                                workbook.Add_Risposte_Text(dom, filtroMatricola, elencoMatricole);
                                workbook.ro++;
                                break;

                            case (int)EnumTipologiaDomanda.Si_No:
                                workbook.Add_Risposte_SINO(dom, filtroMatricola, elencoMatricole);
                                workbook.ro++;
                                break;

                            case (int)EnumTipologiaDomanda.Risposta_Multipla_CheckBox:
                            case (int)EnumTipologiaDomanda.Risposta_singola_Lista_RadioButton:
                            case (int)EnumTipologiaDomanda.Risposta_Singola_Lista_Tendina:
                                workbook.Add_Risposte_CB_RB_Tend(dom, filtroMatricola, elencoMatricole);
                                workbook.ro++;
                                break;

                            case (int)EnumTipologiaDomanda.Precompilato:
                                workbook.Add_Risposte_Text(dom, filtroMatricola, elencoMatricole);
                                workbook.ro++;
                                break;
                        }
                    }
                }
            }
        }

        public ActionResult Index(FormStatsModel model)
        {
            var db = new digiGappEntities();
            string matr = CommonHelper.GetCurrentUserMatricola();

            model.forms = db.MyRai_FormPrimario.Where(x => x.attivo && x.creato_da == matr
                 && (model.searchModel.id_tipologia == null
                     ? true
                     : x.id_tipologia == model.searchModel.id_tipologia)
                 && (model.searchModel.nome == null || model.searchModel.nome.Trim() == ""
                     ? true
                     : x.titolo.Contains(model.searchModel.nome)))
                .OrderBy(x => x.titolo).ToList();
            return View(model);
        }

        public ActionResult GetData(int idform, string idHotel = null)
        {
            var db = new digiGappEntities();
            MyRai_FormPrimario questionario = db.MyRai_FormPrimario.Find(idform);

            var workbook = new MyExcelWorkbook(questionario.titolo, questionario.anonimo);

            if (questionario.MyRai_FormTipologiaForm.tipologia != "Hotel")
            {
                FillWorkbook(questionario, workbook, false, null);
                workbook.Add_Riepilogo(questionario.MyRai_FormCompletati.Count());
            }
            else
            {
                var dbh = new alberghiEntities();
                List<Tuple<string, List<string>>> elencoHotelMatricole = GetElencoHotelMatricole(idHotel, questionario);
                foreach (var item in elencoHotelMatricole)
                {
                    myRaiData.Hotel hotel = dbh.Hotel.FirstOrDefault(x => x.Id_Hotel == item.Item1);
                    string sottoTitolo = "Albergo non trovato";
                    if (hotel != null) sottoTitolo = hotel.Nome + " - " + hotel.Città.Nome_Città + " (" + hotel.Città.Province.Sigla_Provincia + ")";

                    workbook.Add_Sottotitolo(sottoTitolo);
                    FillWorkbook(questionario, workbook, true, item.Item2);
                    workbook.Add_Riepilogo(item.Item2.Count());
                    workbook.ro += 2;
                }
            }

            workbook.FitWidth();
            return workbook.GetFileStream();
        }

        public ActionResult GetDataExp(int idForm, string idHotel = null)
        {
            var db = new digiGappEntities();
            MyRai_FormPrimario questionario = db.MyRai_FormPrimario.Find(idForm);
            string titoloQuestionario = questionario.titolo;

            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Dati");

            int col = 1;
            

            //WriteHeader
            foreach (var sec in questionario.MyRai_FormSecondario.Where(x => x.attivo).OrderBy(x => x.progressivo))
            {
                var listDom = sec.MyRai_FormDomande.Where(x => x.attiva && x.id_domanda_parent == null).OrderBy(x => x.progressivo);
                int countSubDom = listDom.Count();

                int secStartCol = col;
                ws.Cell(1, col).SetValue(sec.titolo);

                foreach (var dom in sec.MyRai_FormDomande.Where(x => x.attiva && x.id_domanda_parent == null).OrderBy(x => x.progressivo))
                {
                    int domStartCol = col;
                    

                    bool HasAnySub = false;
                    if (dom.id_tipologia == (int)EnumTipologiaDomanda.MasterPerMatrixRating)
                    {
                        ws.Cell(2, col).SetValue(dom.titolo);
                        foreach (var subDom in dom.MyRai_FormDomande1.Where(x => x.attiva))
                        {
                            HasAnySub = true;
                            ws.Cell(3, col).SetValue(subDom.titolo);
                            ScriviRisposte(ws, subDom, 4, col);
                            col++;
                        }
                    }
                    else
                    {
                        ws.Cell(3, col).SetValue(dom.titolo);
                        ScriviRisposte(ws, dom, 4, col);
                        col++;
                    }

                    ws.Range(2, domStartCol, 2 , col-1).Merge();
                }

                ws.Range(1, secStartCol, 1, col - 1).Merge();
            }

            foreach (var column in ws.Columns())
            {
                column.AdjustToContents();
            }

            ws.Range(3, 1, ws.RowsUsed().Count(), col - 1).CreateTable();

            MemoryStream M = new MemoryStream();
            wb.SaveAs(M);
            M.Position = 0;
            return new FileStreamResult(M, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = titoloQuestionario.Replace(" ", "_") + ".xlsx" };
        }

        private static void ScriviRisposte(IXLWorksheet ws, MyRai_FormDomande dom, int startRow, int col)
        {
            int row = startRow;

            var risposte = dom.MyRai_FormRisposteDate.OrderBy(x => x.matricola);
            EnumTipologiaDomanda tipoDom = (EnumTipologiaDomanda)dom.id_tipologia;

            foreach (var risp in risposte)
            {
                switch (tipoDom)
                {
                    case EnumTipologiaDomanda.Si_No:
                        ws.Cell(row, col).SetValue(risp.risposta_text);
                        break;
                    case EnumTipologiaDomanda.Risposta_singola_Lista_RadioButton:
                    case EnumTipologiaDomanda.Risposta_Singola_Lista_Tendina:
                    case EnumTipologiaDomanda.Risposta_Multipla_CheckBox:
                        if (risp.id_risposta != null)
                        {
                            ws.Cell(row, col).SetValue(risp.MyRai_FormRispostePossibili.item_risposta);
                        }
                        else
                        {
                            ws.Cell(row, col).SetValue(risp.risposta_text);
                        }
                        break;
                    case EnumTipologiaDomanda.ShortText:
                    case EnumTipologiaDomanda.LongText:
                    case EnumTipologiaDomanda.Precompilato:
                        ws.Cell(row, col).SetValue(risp.risposta_text);
                        break;
                    case EnumTipologiaDomanda.MasterPerMatrixRating:
                        break;
                    case EnumTipologiaDomanda.SlavePerMatrixRating:
                        break;
                    case EnumTipologiaDomanda.SlavePerRating_6:
                        ws.Cell(row, col).SetValue(risp.MyRai_FormRispostePossibili.item_risposta);
                        break;
                }

                ws.Cell(row, col).Style.Alignment.WrapText = false;

                row++;
            }

        }

        public ActionResult GetStats(int idform, string idHotel = null)
        {

            string[] colors = CommonHelper.GetParametro<string>(EnumParametriSistema.ColoriCharts).Split(',')
                             .Select(item => "#" + item.TrimStart('#')).ToArray();

            var db = new digiGappEntities();
            MyRai_FormPrimario formPrimario = db.MyRai_FormPrimario.Find(idform);

            List<FormStatisticsModel> statList = new List<FormStatisticsModel>();
            if (formPrimario.MyRai_FormTipologiaForm.tipologia != "Hotel")
            {
                FormStatisticsModel statModel = GetFormStatisticModel(formPrimario, colors, false, null);
                statModel.Titolo = formPrimario.titolo;
                statList.Add(statModel);
            }
            else
            {
                var dbh = new alberghiEntities();
                List<Tuple<string, List<string>>> elencoHotelMatricole = GetElencoHotelMatricole(idHotel, formPrimario);
                foreach (var item in elencoHotelMatricole)
                {
                    FormStatisticsModel model = GetFormStatisticModel(formPrimario, colors, true, item.Item2);
                    Hotel hotel = dbh.Hotel.FirstOrDefault(x => x.Id_Hotel == item.Item1);
                    model.Titolo = formPrimario.titolo;
                    if (hotel != null)
                        model.Titolo += ": " + hotel.Nome + " - " + hotel.Città.Nome_Città + " (" + hotel.Città.Province.Sigla_Provincia + ")";
                    else
                        model.Titolo += ": *Albergo non trovato*";
                    statList.Add(model);
                }
            }

            statList.Sort(delegate (FormStatisticsModel model1, FormStatisticsModel model2)
            {
                return model1.Titolo.CompareTo(model2.Titolo);
            });

            return View(statList);
        }

        public ActionResult GetFormDetails(int idform, string idHotel = null)
        {
            string[] colors = CommonHelper.GetParametro<string>(EnumParametriSistema.ColoriCharts).Split(',')
                 .Select(item => "#" + item.TrimStart('#')).ToArray();

            var db = new digiGappEntities();
            MyRai_FormPrimario formPrimario = db.MyRai_FormPrimario.Find(idform);
            if (formPrimario == null)
                return Http404();

            List<FormStatisticsModel> statList = new List<FormStatisticsModel>();
            if (formPrimario.MyRai_FormTipologiaForm.tipologia != "Hotel")
            {
                FormStatisticsModel statModel = GetFormDetailModel(db, formPrimario, colors, false, null);
                statModel.Titolo = formPrimario.titolo;
                statList.Add(statModel);
            }
            else
            {
                var dbh = new alberghiEntities();
                List<Tuple<string, List<string>>> elencoHotelMatricole = GetElencoHotelMatricole(idHotel, formPrimario);
                foreach (var item in elencoHotelMatricole)
                {
                    FormStatisticsModel model = GetFormDetailModel(db, formPrimario, colors, true, item.Item2);
                    Hotel hotel = dbh.Hotel.FirstOrDefault(x => x.Id_Hotel == item.Item1);
                    model.Titolo = formPrimario.titolo;
                    if (hotel != null)
                        model.Titolo += ": " + hotel.Nome + " - " + hotel.Città.Nome_Città + " (" + hotel.Città.Province.Sigla_Provincia + ")";
                    else
                        model.Titolo += ": *Albergo non trovato*";
                    statList.Add(model);
                }
            }

            statList.Sort(delegate (FormStatisticsModel model1, FormStatisticsModel model2)
            {
                return model1.Titolo.CompareTo(model2.Titolo);
            });

            return View("GetFormDetails", statList);
        }

        public double CalcoloPercentuale(int tot, int num)
        {
            if (num == 0)
                return 0;
            else
                return Math.Round((double)((double)num * 100 / (double)tot), 2);
        }
        private FormStatisticsModel GetFormDetailModel(digiGappEntities db, MyRai_FormPrimario formPrimario, string[] colors, bool filtroMatricola, List<string> elencoMatricole)
        {
            List<DatoDomanda> elencoDatoDomanda = new List<DatoDomanda>();

            FormStatisticsModel statModel = new FormStatisticsModel();
            statModel.formprimario = formPrimario;

            foreach (var dom in formPrimario.MyRai_FormSecondario.Where(x => x.attivo).OrderBy(x => x.progressivo).SelectMany(x => x.MyRai_FormDomande.Where(y => y.attiva).OrderBy(y => y.progressivo)))
            {
                List<MyRai_FormRisposteDate_Sentiment> sentiments = new List<MyRai_FormRisposteDate_Sentiment>();

                var elencoRisp = RisposteDateHelper.GetElencoRisposte(dom.MyRai_FormRisposteDate, filtroMatricola, elencoMatricole);
                int totRisp = elencoRisp.Count();
                int tmpCount = 0;

                if (dom.id_tipologia == 5 || dom.id_tipologia == 6 || dom.id_tipologia == 7)
                {
                    if (totRisp == 0)
                        continue;

                    var ids = elencoRisp.Select(x => x.id);
                    sentiments.AddRange(db.MyRai_FormRisposteDate_Sentiment.Where(x => ids.Contains(x.id_risposta)));

                    if (!sentiments.Any())
                        continue;
                };


                DatoDomanda d = new DatoDomanda();
                d.domanda = dom;
                d.PieItems = new List<PieItem>();
                if (dom.id_tipologia == 1)
                {
                    int counter = 0;

                    tmpCount = RisposteDateHelper.GetElencoRisposte(elencoRisp.Where(x => x.risposta_text == "SI"), filtroMatricola, elencoMatricole).Count();
                    d.PieItems.Add(new PieItem()
                    {
                        color = colors[counter % colors.Length],
                        label = String.Format("SI {0}%", CalcoloPercentuale(totRisp, tmpCount)),
                        data = new List<List<int>>() { new List<int>() { 1, tmpCount } }
                    });
                    counter++;

                    tmpCount = RisposteDateHelper.GetElencoRisposte(elencoRisp.Where(x => x.risposta_text == "NO"), filtroMatricola, elencoMatricole).Count();
                    d.PieItems.Add(new PieItem()
                    {
                        color = colors[counter % colors.Length],
                        label = String.Format("NO {0}%", CalcoloPercentuale(totRisp, tmpCount)),
                        data = new List<List<int>>() { new List<int>() { 1, tmpCount } }
                    });
                }
                else if (sentiments.Any())
                {
                    totRisp = elencoRisp.Count();
                    int totSentiments = sentiments.Count();

                    d.PieItems.Add(new PieItem()
                    {
                        color = "",
                        label = String.Format("Positivo {0}%", CalcoloPercentuale(totRisp, sentiments.Count(x => x.sentiment == "Positive"))),
                        data = new List<List<int>>() { new List<int>() { 1, sentiments.Count(x => x.sentiment == "Positive") } }
                    });
                    d.PieItems.Add(new PieItem()
                    {
                        color = "",
                        label = String.Format("Negativo {0}%", CalcoloPercentuale(totRisp, sentiments.Count(x => x.sentiment == "Negative"))),
                        data = new List<List<int>>() { new List<int>() { 1, sentiments.Count(x => x.sentiment == "Negative") } }
                    });
                    d.PieItems.Add(new PieItem()
                    {
                        color = "",
                        label = String.Format("Neutrale {0}%", CalcoloPercentuale(totRisp, sentiments.Count(x => x.sentiment == "Neutral"))),
                        data = new List<List<int>>() { new List<int>() { 1, sentiments.Count(x => x.sentiment == "Neutral") } }
                    });

                    int countVuote = elencoRisp.Count(x => String.IsNullOrWhiteSpace(x.risposta_text));
                    d.PieItems.Add(new PieItem()
                    {
                        color = "",
                        label = String.Format("Vuote {0}%", CalcoloPercentuale(totRisp, countVuote)),
                        data = new List<List<int>>() { new List<int>() { 1, countVuote } }
                    });

                    d.PieItems.Add(new PieItem()
                    {
                        color = "",
                        label = String.Format("Scartate {0}%", CalcoloPercentuale(totRisp, totRisp - totSentiments - countVuote)),
                        data = new List<List<int>>() { new List<int>() { 1, totRisp - totSentiments - countVuote } }
                    });

                    d.HasSentiment = true;
                    statModel.Sentiments.AddRange(sentiments);
    }
                else
                {
                    int counter = 0;
                    foreach (var rispostaPossibile in d.domanda.MyRai_FormRispostePossibili)
                    {
                        tmpCount = RisposteDateHelper.GetElencoRisposte(rispostaPossibile.MyRai_FormRisposteDate, filtroMatricola, elencoMatricole).Count();

                        d.PieItems.Add(new PieItem()
                        {
                            color = colors[counter % colors.Length],
                            label = String.Format("{0} {1}%", rispostaPossibile.item_risposta, CalcoloPercentuale(totRisp, tmpCount)),
                            data = new List<List<int>>() { new List<int>() { 1, tmpCount } }
                        });
                        counter++;
                    }
                    List<MyRai_FormRisposteDate> risposteFreeText = RisposteDateHelper.GetElencoRisposte(elencoRisp.Where(z => z.id_risposta == null), filtroMatricola, elencoMatricole).ToList();
                    bool addStar = false;
                    List<MyRai_FormRisposteDate> altro = new List<MyRai_FormRisposteDate>();
                    foreach (var risposta in risposteFreeText.GroupBy(x => x.risposta_text.ToLower()))
                    {
                        var perc = CalcoloPercentuale(totRisp, risposta.Count());
                        if (perc >= 3)
                        {
                            addStar = true;
                            d.PieItems.Add(new PieItem()
                            {
                                color = colors[counter % colors.Length],
                                label = String.Format("{0} {1}%*", risposta.Key.TitleCase(), perc),
                                data = new List<List<int>>() { new List<int>() { 1, risposta.Count() } },
                            });
                        }
                        else
                        {
                            altro.AddRange(risposta);
                        }
                    }

                    if (altro.Any())
                    {
                        d.PieItems.Add(new PieItem()
                        {
                            color = colors[counter % colors.Length],
                            label = String.Format("Altro {0}%{1}", CalcoloPercentuale(totRisp, altro.Count()), addStar ? "*" : ""),
                            data = new List<List<int>>() { new List<int>() { 1, altro.Count() } },
                        });
                    }

                    d.HasOtherCollapse = addStar;
                }

                d.JsonPieItems = new JavaScriptSerializer().Serialize(d.PieItems);

                elencoDatoDomanda.Add(d);
            }

            foreach (var datoDomanda in elencoDatoDomanda.Where(x => x.domanda.id_domanda_parent == null))
            {
                statModel.items.Add(datoDomanda);
                statModel.items.AddRange(elencoDatoDomanda.Where(x => x.domanda.id_domanda_parent == datoDomanda.domanda.id));
            }

            return statModel;
        }
    }


    public class MyExcelWorkbook : XLWorkbook
    {
        private bool _isAnonimo;
        private Dictionary<string, string> _matricole;

        public MyExcelWorkbook(string titolo, bool anonimo)
        {
            _isAnonimo = anonimo;
            _matricole = new Dictionary<string, string>();

            this.co = 1;
            this.ro = 1;
            this.titoloQuestionario = titolo;
            worksheet = this.Worksheets.Add("Dati");
            worksheet.Cell(ro, 1).Value = "Titolo questionario";
            worksheet.Cell(ro, 1).Style.Font.Bold = true;
            worksheet.Cell(ro, 2).Value = "Sezione";
            worksheet.Cell(ro, 2).Style.Font.Bold = true;
            worksheet.Cell(ro, 3).Value = "Domanda (* = obbligatoria)";
            worksheet.Cell(ro, 3).Style.Font.Bold = true;
            worksheet.Cell(ro, 4).Value = "Tipologia";
            worksheet.Cell(ro, 4).Style.Font.Bold = true;
            //worksheet.Cell(ro, 5).Value = "Risposte";
            //worksheet.Cell(ro, 5).Style.Font.Bold = true;
            this.ro++;
        }

        private int _co;
        public int co
        {
            get { return _co; }
            set
            {
                _co = value;
                if (_co > ColumnMax) ColumnMax = _co;
            }
        }
        public int ro { get; set; }
        public string titoloQuestionario { get; set; }
        public IXLWorksheet worksheet { get; set; }
        public int ColumnMax { get; set; }

        public void Add_Sottotitolo(string sottoTitolo)
        {
            this.ro++;
            worksheet.Cell(ro, 1).Value = sottoTitolo;
            worksheet.Cell(ro, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(0xF0F0F0);
            worksheet.Cell(ro, 1).Style.Font.Italic = true;
            worksheet.Cell(ro, 1).Style.Font.Bold = true;

            worksheet.Range(worksheet.Cell(ro, 1), worksheet.Cell(ro, 4)).Merge();
        }
        public void Add_Domanda_Titolo(MyRai_FormDomande dom)
        {
            this.ro++;
            worksheet.Cell(ro, 1).Value = dom.MyRai_FormSecondario.MyRai_FormPrimario.titolo;
            worksheet.Cell(ro, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(0xF0F0F0);

            worksheet.Cell(ro, 2).Value = dom.MyRai_FormSecondario.titolo;
            worksheet.Cell(ro, 2).Style.Fill.BackgroundColor = XLColor.FromArgb(0xF0F0F0);

            worksheet.Cell(ro, 3).Value = (dom.id_domanda_parent != null ? "    " : "") + dom.titolo + (dom.obbligatoria ? "*" : "");
            worksheet.Cell(ro, 3).Style.Fill.BackgroundColor = XLColor.FromArgb(0xF0F0F0);

            worksheet.Cell(ro, 4).Value = dom.MyRai_FormTipologieDomande.tipologia;
            worksheet.Cell(ro, 4).Style.Fill.BackgroundColor = XLColor.FromArgb(0xF0F0F0);
            this.co = 4;
        }
        public void Add_Risposte_Text(MyRai_FormDomande dom, bool filtroMatricola, List<string> elencoMatricole)
        {
            foreach (var r in RisposteDateHelper.GetElencoRisposte(dom.MyRai_FormRisposteDate, filtroMatricola, elencoMatricole))
            {
                //this.co++;
                //worksheet.Cell(ro, co).Value = r.risposta_text;
                if (_isAnonimo && String.IsNullOrWhiteSpace(r.risposta_text)) continue;

                this.ro++;
                worksheet.Cell(ro, 3).Style.Font.Italic = true;
                worksheet.Cell(ro, 3).Value = r.risposta_text;

                if (!_isAnonimo) Add_Matricola(r);
            }
        }
        public void Add_Risposte_SINO(MyRai_FormDomande dom, bool filtroMatricola, List<string> elencoMatricole)
        {
            var risposteSINO = RisposteDateHelper.GetElencoRisposte(dom.MyRai_FormRisposteDate, filtroMatricola, elencoMatricole).GroupBy(x => x.risposta_text).ToList();
            foreach (var r in risposteSINO)
            {
                this.ro++;
                worksheet.Cell(ro, 3).Value = r.Select(x => x.risposta_text).FirstOrDefault();
                worksheet.Cell(ro, 3).Style.Font.Italic = true;
                this.co++;
                worksheet.Cell(ro, 2).Value = r.Count().ToString();
                worksheet.Cell(ro, 2).Style.Font.Italic = true;

                if (!_isAnonimo) Add_Matricola_List(r);
            }
        }
        public void Add_Risposte_CB_RB_Tend(MyRai_FormDomande dom, bool filtroMatricola, List<string> elencoMatricole)
        {
            var risposte = RisposteDateHelper.GetElencoRisposte(dom.MyRai_FormRisposteDate, filtroMatricola, elencoMatricole).GroupBy(x => x.id_risposta).ToList();
            foreach (var r in risposte.OrderByDescending(x => x.Key))
            {
                if (r.Key != null)
                {

                    this.ro++;
                    worksheet.Cell(ro, 3).Value = r.First().MyRai_FormRispostePossibili.item_risposta;
                    worksheet.Cell(ro, 3).Style.Font.Italic = true;
                    this.co++;
                    worksheet.Cell(ro, 2).Value = r.Count().ToString();
                    worksheet.Cell(ro, 2).Style.Font.Italic = true;
                    //this.co++;
                    //worksheet.Cell(ro, this.co).Value = r.First().MyRai_FormRispostePossibili.item_risposta;
                    //this.co++;
                    //worksheet.Cell(ro, this.co).Value = r.Count().ToString();
                    if (!_isAnonimo) Add_Matricola_List(r);
                }
                else
                {
                    this.ro++;
                    //worksheet.Cell(ro, 3).Value = r.Select(x => x.risposta_text).FirstOrDefault();
                    //worksheet.Cell(ro, 3).Style.Font.Italic = true;
                    worksheet.Cell(ro, 3).Value = "Altro";
                    worksheet.Cell(ro, 3).Style.Font.Italic = true;
                    this.co++;
                    worksheet.Cell(ro, 2).Value = r.Count().ToString();
                    worksheet.Cell(ro, 2).Style.Font.Italic = true;

                    foreach (var other in r)
                    {
                        this.ro++;
                        worksheet.Cell(ro, 3).Value = other.risposta_text;
                        worksheet.Cell(ro, 3).Style.Font.Italic = true;

                        if (!_isAnonimo) Add_Matricola(other);
                    }
                }
            }
        }

        private void Add_Matricola_List(IEnumerable<MyRai_FormRisposteDate> r)
        {
            foreach (var item in r)
            {
                Add_Matricola(item);
            }
        }

        private void Add_Matricola(MyRai_FormRisposteDate item)
        {
            this.ro++;
            string nominativo = "";

            string matricola = item.matricola;
            if (item.MyRai_FormDomande.MyRai_FormSecondario.MyRai_FormPrimario.MyRai_FormTipologiaForm.tipologia == "Hotel")
                matricola = matricola.Split('-')[0].Trim();

            if (!_matricole.TryGetValue(matricola, out nominativo))
            {
                var dbCV = new cv_ModelEntities();
                var dipendente = dbCV.TDipendenti.FirstOrDefault(x => x.Matricola == matricola);
                if (dipendente != null)
                    nominativo = dipendente.Nominativo;// CommonHelper.GetNominativoPerMatricola(matricola);
                _matricole.Add(matricola, nominativo);
            }
            worksheet.Cell(ro, 2).Value = matricola + " - " + nominativo;
        }

        public void Add_Riepilogo(int numCompilazioni)
        {
            this.ro += 2;
            worksheet.Cell(ro, 1).Value = "Compilazioni trovate: " + numCompilazioni;
        }

        public void FitWidth()
        {
            for (int i = 1; i < ColumnMax; i++)
            {
                this.worksheet.Column(i).AdjustToContents();
            }
        }
        public ActionResult GetFileStream()
        {
            MemoryStream M = new MemoryStream();
            this.SaveAs(M);
            M.Position = 0;
            return new FileStreamResult(M,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            { FileDownloadName = titoloQuestionario.Replace(" ", "_") + ".xlsx" };
        }
    }
}