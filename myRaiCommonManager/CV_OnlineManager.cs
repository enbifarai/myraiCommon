using myRai.Business;
using myRai.Models;
using myRaiCommonModel.cvModels;
using myRaiCommonModel.cvModels.Pdf;
using myRai.Data.CurriculumVitae;
using myRaiHelper;
using myRaiServiceHub.it.rai.servizi.hrce;
using myRaiServiceHub.it.rai.servizi.hrgb;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Text.RegularExpressions;

namespace myRaiCommonManager
{
    public class CV_OnlineManager
    {
        public static string GetShareAllegati()
        {
            string configPath = "";
            using (myRaiData.digiGappEntities db = new myRaiData.digiGappEntities())
            {
                var row = db.MyRai_ParametriSistema.Where(p => p.Chiave.Equals("ShareAllegati", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                if (row != null)
                {
                    configPath = row.Valore1;
                }
                else
                {
                    configPath = "cv_media";
                }
            }
            return configPath;
        }

        public static string GetFiguraProfessionaleForzata(string matricola)
        {
            var db = new myRaiData.digiGappEntities();
            try
            {
                string[] fp = CommonManager.GetParametri<string>(EnumParametriSistema.ForzaFiguraProfessionale);

                if (fp == null || fp.Length < 2) return null;
                if (fp[0] == matricola)
                    return fp[1];
                else
                    return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static MemoryStream GetCVPdf(string serverMapPath, string fontPath, string matricola, string msgUltimoAgg, out int error)
        {
            error = 0;

            //if (Server != null)
            //{
            //    FontManager.FontPath = Server.MapPath("~/assets/fontG/open-sans-v13-latin-300.ttf");
            //}
            FontManager.FontPath = fontPath;

            CultureInfo culture = new CultureInfo("it-IT");
            TextInfo textInfo = culture.TextInfo;

            cvModel cvBox = new cvModel(false, false);

            Utente.Anagrafica anagrafica = new Utente.Anagrafica();
            if (matricola == null || matricola == CommonManager.GetCurrentUserMatricola())
                anagrafica = Utente.EsponiAnagrafica();
            else
            {
                if (!CommonManager.PdfAutorizzato(CommonManager.GetCurrentUserMatricola()))
                {
                    error = 403;
                }

                Service wsAnag = new Service();
                try
                {
                    wsAnag.Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
                    string str_temp = wsAnag.EsponiAnagrafica("RAICV;" + matricola + ";;E;0");
                    string[] temp = str_temp.ToString().Split(';');
                    if ((temp != null) && (temp.Count() > 16))
                    {
                        anagrafica = Utente.CaricaAnagrafica(temp);
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            float indentationLeft = 168f;

            cv_ModelEntities cvEnt;
            string selMatricola = anagrafica._matricola;
            string codFigProf = anagrafica._codiceFigProf;
            string sezMatricola = anagrafica._sezContabile;

            MemoryStream workStream = new MemoryStream();
            iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 24, 24, 24, 48);
            PdfWriter writer = PdfWriter.GetInstance(document, workStream);
            writer.CloseStream = false;

            List<RenderableItem> rowItemList = new List<RenderableItem>();

            string[] inquadramento = anagrafica._inquadramento.ToString().Split(';');
            if (inquadramento.Length > 1)
            {
                SingleValue[] values = new SingleValue[inquadramento.Length - 1];

                for (int i = 1; i < inquadramento.Length; i++)
                {
                    if (i == 1)
                    {
                        rowItemList.Add(new KeyValue { key = "SETTORE", value = inquadramento[i] });
                        continue;
                    }

                    rowItemList.Add(new SingleValue { newLine = false, value = inquadramento[i], IndentationLeft = 5f * (i - 1) });
                }
            }

            rowItemList.Add(new KeyValue { key = "CONTRATTO", value = anagrafica._contratto });
            rowItemList.Add(new KeyValue { key = "FIGURA PROFESSIONALE", value = textInfo.ToTitleCase(anagrafica._figProfessionale.ToLower()) });
            rowItemList.Add(new KeyValue { key = "MATRICOLA", value = anagrafica._matricola });

            string indirizzo = "";

            try
            {
                string[] AccountUtenteServizio = CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio);
                hrce_ws hrcews = new hrce_ws();
                retData retdata = new retData();
                hrcews.Credentials = new System.Net.NetworkCredential(AccountUtenteServizio[0], AccountUtenteServizio[1], "RAI");
                List<string> matricole = new List<string>();
                matricole.Add(selMatricola);
                string[] AppKeyhrce = CommonManager.GetParametri<string>(EnumParametriSistema.AppKeyhrce);

                Boolean usaServ = CommonManager.GetParametro<Boolean>(EnumParametriSistema.UsaServizioPerProfiloPersonale);
                if (usaServ)
                    retdata = hrcews.getDatiUtente(AppKeyhrce[0], matricole.ToArray());
                else
                    retdata.ds = ProfiloPersonaleManager.GetProfiloPersonaleFromDB(selMatricola);


                DataTable dr_recapiti = null;
                if (retdata.ds != null)
                    dr_recapiti = retdata.ds.Tables["Table1"];

                string matricolaUtente = selMatricola;
                if (dr_recapiti != null && dr_recapiti.Rows[0]["Matricola"].ToString() != matricolaUtente)
                {
                    dr_recapiti = null;
                }

                if (dr_recapiti != null)
                {
                    indirizzo = string.Format("{0} {1} {2}",
                                dr_recapiti.Rows[0]["INDIRIZZODOM"].ToString(),
                                dr_recapiti.Rows[0]["CAPDOM"].ToString()
                                , dr_recapiti.Rows[0]["CITTADOM"].ToString()
                                );

                    indirizzo = textInfo.ToTitleCase(indirizzo.ToLower());

                    indirizzo = string.Format("{0} ({1})",
                                indirizzo
                                , dr_recapiti.Rows[0]["PROVDOM"].ToString().ToUpper()
                                );
                }
            }
            catch (Exception)
            {
            }

            rowItemList.Add(new LineBreak());
            rowItemList.Add(new KeyValue { key = "DATA DI NASCITA", value = String.Format("{0:dd/MM/yyyy}", anagrafica._dataNascita) });
            rowItemList.Add(new KeyValue { key = "INDIRIZZO", value = indirizzo });
            rowItemList.Add(new LineBreak());
            rowItemList.Add(new KeyValue { key = "EMAIL", value = anagrafica._email });
            rowItemList.Add(new KeyValue { key = "TELEFONO AZIENDALE", value = anagrafica._telefono });

            #region ULTIMO AGGIORNAMENTO

            cv_ModelEntities db = new cv_ModelEntities();

            var aa = db.sp_GETDTAGGCV(selMatricola);
            var rbb = aa.ToList();

            if (rbb.Count > 0 && rbb[0] != null)
            {
                string updatedAt = rbb[0].Value.ToString("dd MMMM yyyy HH:mm:ss", culture);

                rowItemList.Add(new LineBreak());

                if (!String.IsNullOrEmpty(msgUltimoAgg))
                {
                    rowItemList.Add(new SmallItalic { value = string.Format("{0} {1}", msgUltimoAgg, updatedAt) });
                }
                else
                {
                    rowItemList.Add(new SmallItalic { value = string.Format("Aggiornato al: {0}", updatedAt) });
                }
            }

            #endregion

            string image = anagrafica._foto;

            writer.PageEvent = new myRaiCommonModel.cvModels.Pdf.ITextEvents(image, rowItemList);

            document.Open();

            List<ContentBlockInfo> contentBlockInfoList = new List<ContentBlockInfo>();

            List<RenderableItem> blockItemInfoList = new List<RenderableItem>();

            #region AUTOPRESENTAZIONE
            cvEnt = new cv_ModelEntities();
            blockItemInfoList = new List<RenderableItem>();
            var autopresList = cvEnt.TCVAllegato.Where(x => x.Matricola == selMatricola && x.Stato == "#").ToList();
            if (autopresList != null && autopresList.Count > 0)
            {
                var autopresentazione = autopresList[0];
                if (!String.IsNullOrWhiteSpace(autopresentazione.Path_name) || !String.IsNullOrWhiteSpace(autopresentazione.Note))
                {
                    BlockItemInfo block = new BlockItemInfo();
                    block.key = "";

                    if (!String.IsNullOrWhiteSpace(autopresentazione.Path_name))
                    {
                        string baseUrl = System.Web.HttpContext.Current.Request.Url.Scheme + "://" + System.Web.HttpContext.Current.Request.Url.Authority;

                        string configPath = GetShareAllegati();

                        string attachmentPath = "";

                        //if (string.IsNullOrEmpty(this._serverPath))
                        //    attachmentPath = Path.Combine(Server.MapPath("~"), configPath, autopresentazione.Matricola, autopresentazione.Path_name);
                        //else
                        //    attachmentPath = Path.Combine(this._serverPath, configPath, autopresentazione.Matricola, autopresentazione.Path_name);
                        attachmentPath = Path.Combine(serverMapPath, configPath, autopresentazione.Matricola, autopresentazione.Path_name);

                        if (autopresentazione.ContentType.Contains("image") && System.IO.File.Exists(attachmentPath))
                        {
                            var img = Image.GetInstance(attachmentPath);
                            float maxSize = PageSize.A4.Width - indentationLeft - 48;
                            if (img.Width > maxSize)
                                img.ScaleToFit(maxSize, maxSize);
                            img.SpacingBefore = 5F;
                            img.Alt = autopresentazione.Name;

                            block.chk = new Chunk(img, 0, 0, true); ;
                        }
                        else if (autopresentazione.ContentType == "website")
                        {
                            string link = autopresentazione.Path_name;
                            block.chk = new Chunk("Clicca qui per vedere l'autopresentazione", new FontManager("", BaseColor.BLACK).Italic).SetAction(new PdfAction(link, false));
                        }
                        else
                        {
                            string link = baseUrl + "/cv_online/GetFile/" + autopresentazione.Id;
                            block.chk = new Chunk("Clicca qui per vedere l'autopresentazione", new FontManager("", BaseColor.BLACK).Italic).SetAction(new PdfAction(link, false));
                        }
                    }

                    block.value = autopresentazione.Note;

                    blockItemInfoList.Add(block);
                }

                if (blockItemInfoList.Count > 0)
                {
                    contentBlockInfoList.Add(new ContentBlockInfo("MI PRESENTO", blockItemInfoList));
                }
            }
            blockItemInfoList = new List<RenderableItem>();
            #endregion

            #region ESPERIENZE

            cvEnt = new cv_ModelEntities();
            var esperienze = cvEnt.TCVEsperExRai.Where(x => x.Matricola == selMatricola && x.DataFine == null).OrderByDescending(z => z.DataFine).ToList();

            foreach (TCVEsperExRai esp in esperienze)
            {
                PdfExperience(cvEnt, codFigProf, blockItemInfoList, esp);
            }

            esperienze = cvEnt.TCVEsperExRai.Where(x => x.Matricola == selMatricola && x.DataFine != null).OrderByDescending(z => z.DataFine).ToList();
            foreach (TCVEsperExRai esp in esperienze)
            {
                PdfExperience(cvEnt, codFigProf, blockItemInfoList, esp);
            }

            if (blockItemInfoList.Count > 0)
            {
                contentBlockInfoList.Add(new ContentBlockInfo("ESPERIENZE", blockItemInfoList));
            }
            #endregion

            if ((codFigProf != "XAA" && codFigProf != "XDA" || CommonManager.AbilitatoEditoriali(sezMatricola))
                || cvEnt.TSVEsperProd.Any(f => f.STATO == "C" && f.COD_ANAGRAFIA != "IR" && f.MATRICOLA == selMatricola)
                || cvEnt.TCVConProf.Any(x => x.Matricola == selMatricola))
            {
                #region IMPEGNI EDITORIALI RAI

                blockItemInfoList = new List<RenderableItem>();

                cvEnt = new cv_ModelEntities();
                cvBox.impegniEditoriali = new List<cvModel.ImpegniRAI>();

                int prog = 0;
                List<TSVEsperProd> esperProd = cvEnt.TSVEsperProd.Where(f => f.STATO == "C" && f.COD_ANAGRAFIA != "IR" && f.MATRICOLA == selMatricola).OrderByDescending(p => p.FINE_PERIODO_ESP).ToList();
                foreach (var elem in esperProd)
                {
                    string tmp_ruolo = "";
                    cvModel.ImpegniRAI impegniRai = new cvModel.ImpegniRAI();
                    prog++;
                    impegniRai._desTitoloDefinit = elem.DES_TITOLO_DEFINIT;
                    impegniRai._idEsperienze = Convert.ToInt32(elem.ID_ESPERIENZE);
                    impegniRai._matricola = selMatricola;
                    impegniRai._matricolaSpett = elem.COD_MATRICOLA;
                    impegniRai._progDaStampare = prog;
                    var conProf = cvEnt.DConProf.Where(x => x.CodConProf == elem.COD_RUOLO).FirstOrDefault();
                    if (conProf != null)
                        tmp_ruolo = conProf.DescConProf;

                    //gestione date
                    impegniRai._ruolo = tmp_ruolo;
                    impegniRai._dtDataInizio = elem.INIZIO_PERIODO_ESP.Substring(6, 2) + "/" + elem.INIZIO_PERIODO_ESP.Substring(4, 2) + "/" + elem.INIZIO_PERIODO_ESP.Substring(0, 4);
                    impegniRai._dtDataFine = elem.FINE_PERIODO_ESP.Substring(6, 2) + "/" + elem.FINE_PERIODO_ESP.Substring(4, 2) + "/" + elem.FINE_PERIODO_ESP.Substring(0, 4);


                    if (!string.IsNullOrEmpty(impegniRai._ruolo) && !string.IsNullOrEmpty(impegniRai._desTitoloDefinit))
                    {
                        int fyear = Int32.Parse(impegniRai._dtDataInizio.Substring(6, 4));
                        int fmonth = Int32.Parse(impegniRai._dtDataInizio.Substring(3, 2));
                        int fday = Int32.Parse(impegniRai._dtDataInizio.Substring(0, 2));

                        DateTime fdate = new DateTime(fyear, fmonth, fday);

                        int tyear = Int32.Parse(impegniRai._dtDataFine.Substring(6, 4));
                        int tmonth = Int32.Parse(impegniRai._dtDataFine.Substring(3, 2));
                        int tday = Int32.Parse(impegniRai._dtDataFine.Substring(0, 2));

                        DateTime tdate = new DateTime(tyear, tmonth, tday);

                        string fromMonth = fdate.ToString("MMMM", culture);

                        string toMonth = tdate.ToString("MMMM", culture);

                        string value = string.Format("da {0} {1} a {2} {3}", fromMonth, fyear, toMonth, tyear);

                        if (toMonth.Equals(DateTime.Now.ToString("MMMM", culture)))
                        {
                            value = string.Format("attuale da {0} {1}", fromMonth, fyear);
                        }

                        string key = string.Format("{0} | {1}", impegniRai._ruolo, impegniRai._desTitoloDefinit);

                        blockItemInfoList.Add(new BlockItemInfo
                        {
                            key = key,
                            value = value
                        });
                    }
                }
                if (blockItemInfoList.Count > 0)
                {
                    contentBlockInfoList.Add(new ContentBlockInfo("IMPEGNI EDITORIALI RAI", blockItemInfoList));
                }

                #endregion

                #region ATTIVITà E COMPETENZE EDITORIALI

                blockItemInfoList = new List<RenderableItem>();

                cvEnt = new cv_ModelEntities();
                cvBox.competenzeRai = new List<cvModel.CompetenzeRAI>();

                //prelevo la figura professionale
                string figura_profesisonale = anagrafica._codiceFigProf;

                List<TCVConProf> tcvConProf = cvEnt.TCVConProf.Where(x => x.Matricola == selMatricola).ToList();
                List<DConProf> dConProf =
                    //cvEnt.DConProf.Where(x => x.FiguraProfessionale == figura_profesisonale && x.Stato=="0").ToList();
                    cvEnt.DConProf.Where(x => x.FiguraProfessionale == "XAA" || x.FiguraProfessionale == "XDA" && x.Stato == "0").ToList();

                foreach (var elem in dConProf)
                {
                    cvModel.CompetenzeRAI frk_compRai = new cvModel.CompetenzeRAI();

                    frk_compRai._codConProf = elem.CodConProf;
                    frk_compRai._descConProf = elem.DescConProf;
                    frk_compRai._figuraProfessionale = elem.FiguraProfessionale;
                    frk_compRai._matricola = selMatricola;

                    var tmp = tcvConProf.Where(x => x.CodConProf == elem.CodConProf).ToList();
                    if (tmp.Count() > 0)
                    {
                        frk_compRai._dataOraAgg = tmp.First().DataOraAgg;
                        frk_compRai._flagExtraRai = tmp.First().FlagExtraRai;
                        frk_compRai._flagPrincipale = tmp.First().FlagPrincipale;
                        frk_compRai._flagSecondario = tmp.First().FlagSecondario;
                        frk_compRai._stato = tmp.First().Stato;
                        frk_compRai._tipoAgg = tmp.First().TipoAgg;

                        if (frk_compRai._flagPrincipale == "1")
                        {
                            frk_compRai._flagChoice = "P";
                        }
                        else if (frk_compRai._flagSecondario == "1")
                        {
                            frk_compRai._flagChoice = "S";
                        }
                        else
                        {
                            frk_compRai._flagChoice = " ";
                        }
                    }
                    else
                    {
                        frk_compRai._dataOraAgg = null;
                        frk_compRai._flagChoice = "";
                        frk_compRai._flagExtraRai = "";
                        frk_compRai._flagPrincipale = "";
                        frk_compRai._flagSecondario = "";
                        frk_compRai._stato = "";
                        frk_compRai._tipoAgg = "";
                    }

                    if (!string.IsNullOrEmpty(frk_compRai._descConProf) && frk_compRai._flagPrincipale.Length > 0)
                    {
                        string raiExtraRai = !String.IsNullOrWhiteSpace(frk_compRai._flagExtraRai) && !frk_compRai._flagExtraRai.Equals("0", StringComparison.InvariantCultureIgnoreCase) ? "Extra Rai" : "Rai";
                        string value = frk_compRai._flagChoice == "P" ? "Principale" : frk_compRai._flagChoice == "S" ? "Secondaria" : "";

                        string key = string.Format("{0} | {1}", frk_compRai._descConProf, raiExtraRai);

                        blockItemInfoList.Add(new BlockItemInfo
                        {
                            key = key,
                            value = value
                        });
                    }
                }

                if (blockItemInfoList.Count > 0)
                {
                    contentBlockInfoList.Add(new ContentBlockInfo(("attività e competenze editoriali").ToUpper(), blockItemInfoList));
                }

                #endregion
            }

            #region TITOLI DI STUDIO E SPECIALIZZAZIONI

            blockItemInfoList = new List<RenderableItem>();

            cvEnt = new cv_ModelEntities();
            cvBox.curricula = new List<cvModel.Studies>();

            var istruzione = (cvEnt.TCVIstruzione.Where(m => m.Matricola == selMatricola).OrderByDescending(a => a.AnnoFine)).ToList();
            var specializz = (cvEnt.TCVSpecializz.Where(m => m.Matricola == selMatricola).OrderByDescending(a => a.DataFine)).ToList();

            #region Istruzione
            foreach (TCVIstruzione istr in istruzione)
            {
                cvModel.Studies frk = new cvModel.Studies(CommonManager.GetCurrentUserMatricola());
                using (var ctx = new cv_ModelEntities())
                {
                    var param = new SqlParameter("@param", istr.CodTitolo);
                    List<Utente.CV_DescTitoloLogo> tmp = ctx.Database.SqlQuery<Utente.CV_DescTitoloLogo>("exec sp_GETDESCTITOLO @param", param).ToList();

                    frk._descTipoTitolo = tmp[0].DescTipoTitolo; //tmp.GetValue(0,0).ToString();
                    frk._descTitolo = tmp[0].DescTitolo;//tmp.GetValue(0, 1).ToString();
                    frk._logo = tmp[0].Logo;//tmp.GetValue(0, 2).ToString();
                    //recupero descNazione tramite codNazione
                    // freak - imposto un valore a frk._codNazione
                    frk._codNazione = istr.CodNazione;

                    if (!String.IsNullOrWhiteSpace(frk._codNazione))
                    {
                        try
                        {
                            var sql2 = ctx.Database.SqlQuery<string>("SELECT DES_NAZIONE FROM DNAZIONE WHERE COD_SIGLANAZIONE = '" + frk._codNazione + "'").ToList();
                            frk._descNazione = sql2[0];
                        }
                        catch
                        {
                            frk._descNazione = "";
                        }
                    }
                }
                frk._codTitolo = istr.CodTitolo;
                frk._codTipoTitolo = istr.CodTipoTitolo;
                frk._corsoLaurea = istr.CorsoLaurea;
                frk._dataFine = istr.AnnoFine;
                frk._dataInizio = istr.AnnoInizio;
                frk._dataoraAgg = (DateTime)istr.DataOraAgg;
                frk._durata = istr.Durata;
                frk._indirizzoStudi = (istr.IndirizzoStudi != null) ? istr.IndirizzoStudi : "";
                frk._istituto = (istr.Istituto != null) ? istr.Istituto : "";
                frk._localitaStudi = (istr.LocalitaStudi != null) ? istr.LocalitaStudi : "";
                frk._lode = ((istr.Lode == null) || (istr.Lode == " ")) ? ' ' : Convert.ToChar(istr.Lode);
                frk._matricola = istr.Matricola;
                frk._prog = -1;
                frk._scala = istr.Scala;
                frk._stato = Convert.ToChar(istr.Stato);
                frk._tipoAgg = Convert.ToChar(istr.TipoAgg);
                frk._titoloSpecializ = null;
                frk._titoloTesi = istr.TitoloTesi;
                frk._voto = istr.Voto;
                frk._riconoscimento = "";
                frk._tableTarget = "I";
                if (!string.IsNullOrEmpty(frk._descTitolo))
                {
                    string voto = !string.IsNullOrEmpty(frk._voto) ? string.Format(" {0}", frk._voto) : "";
                    string lode = frk._lode == 'S' ? " con Lode" : "";
                    string istituto = !string.IsNullOrEmpty(frk._istituto) ? string.Format("{0}", frk._istituto) : "";
                    string localitaStudi = !string.IsNullOrEmpty(frk._localitaStudi) ? string.Format("{0}", frk._localitaStudi) : "";
                    string tiplogia = !string.IsNullOrEmpty(frk._logo) ? string.Format("{0}", frk._logo) : "";

                    string key = "";
                    string value = "";

                    key += tiplogia + " | " + frk._descTitolo;

                    value += istituto;
                    if (localitaStudi != "") value += " - " + localitaStudi;

                    if ((!string.IsNullOrEmpty(voto)) && (!string.IsNullOrEmpty(frk._scala)))
                    {
                        value += string.Format("\r\nVoto: {0} su {2}{1}", voto, lode, frk._scala);
                    }
                    else if ((!string.IsNullOrEmpty(voto)) && (string.IsNullOrEmpty(frk._scala)))
                    {
                        value += string.Format("\r\nVoto: {0}{1}", voto, lode);
                    }

                    value += "\r\nConseguito nel " + frk._dataFine;

                    blockItemInfoList.Add(new BlockItemInfo
                    {
                        key = key,
                        value = value
                    });
                }
            }
            #endregion

            #region Specializzazione
            foreach (TCVSpecializz spec in specializz)
            {
                cvModel.Studies frk = new cvModel.Studies(CommonManager.GetCurrentUserMatricola());
                //freak - riempire i campi
                using (var ctx = new cv_ModelEntities())
                {
                    var param = new SqlParameter("@param", spec.TipoSpecial);
                    List<Utente.CV_DescTitoloLogo> tmp = ctx.Database.SqlQuery<Utente.CV_DescTitoloLogo>("exec sp_GETDESCTITOLO @param", param).ToList();
                    frk._descTipoTitolo = tmp[0].DescTipoTitolo; //tmp.GetValue(0,0).ToString();
                    frk._descTitolo = tmp[0].DescTitolo;//tmp.GetValue(0, 1).ToString();
                    frk._logo = tmp[0].Logo;//tmp.GetValue(0, 2).ToString();
                    if (spec.TipoSpecial == "999")
                    {
                        frk._descTipoTitolo = "Specializzazione";
                        frk._logo = "Master";
                    }
                    //recupero descNazione tramite codNazione
                    // freak - imposto un valore a frk._codNazione
                    frk._codNazione = spec.CodNazione;
                    if (!String.IsNullOrWhiteSpace(frk._codNazione))
                    {
                        try
                        {
                            var sql2 = ctx.Database.SqlQuery<string>("SELECT DES_NAZIONE FROM DNAZIONE WHERE COD_SIGLANAZIONE = '" + frk._codNazione + "'").ToList();
                            frk._descNazione = sql2[0];
                        }
                        catch
                        {
                            frk._descNazione = "";
                        }
                    }
                }
                frk._codTitolo = spec.TipoSpecial;
                frk._corsoLaurea = "";// spec.Titolo; //freak - da controllare con Massimo Tesoro
                //frk._dataFine = spec.DataFine.Substring(6, 2) + "/" + spec.DataFine.Substring(4, 2) + "/" + spec.DataFine.Substring(0, 4);
                //frk._dataInizio = (spec.DataInizio.Length == 8) ? spec.DataInizio.Substring(6, 2) + "/" + spec.DataInizio.Substring(4, 2) + "/" + spec.DataInizio.Substring(0, 4) : "";
                frk._dataFine = !String.IsNullOrWhiteSpace(spec.DataFine) ? spec.DataFine.Substring(0, 4) : "";
                frk._dataInizio = !String.IsNullOrWhiteSpace(spec.DataInizio) ? spec.DataInizio.Substring(0, 4) : "";
                frk._dataoraAgg = (DateTime)spec.DataOraAgg;
                frk._durata = spec.Durata;
                frk._indirizzoStudi = (spec.IndirizzoSpecial != null) ? spec.IndirizzoSpecial : "";
                frk._istituto = (spec.Istituto != null) ? spec.Istituto : "";
                frk._localitaStudi = (spec.LocalitaSpecial != null) ? spec.LocalitaSpecial : "";
                frk._lode = ((spec.Lode == null) || (spec.Lode == " ")) ? ' ' : Convert.ToChar(spec.Lode);
                frk._matricola = spec.Matricola;
                frk._prog = spec.Prog;
                frk._scala = spec.Scala;
                frk._stato = Convert.ToChar(spec.Stato);
                frk._tipoAgg = Convert.ToChar(spec.TipoAgg);
                frk._titoloSpecializ = spec.Titolo;
                frk._titoloTesi = ""; //freak - da controllare con Massimo Tesoro
                frk._voto = spec.Voto;
                //frk._riconoscimento = (spec._riconoscimento != null) ? spec._riconoscimento : "";
                frk._tableTarget = "S";

                if (!string.IsNullOrEmpty(frk._descTitolo))
                {
                    string voto = !string.IsNullOrEmpty(frk._voto) ? string.Format(" {0}", frk._voto) : "";
                    string lode = frk._lode == 'S' ? " con Lode" : "";
                    string istituto = !string.IsNullOrEmpty(frk._istituto) ? string.Format("{0}", frk._istituto) : "";
                    string localitaStudi = !string.IsNullOrEmpty(frk._localitaStudi) ? string.Format("{0}", frk._localitaStudi) : "";
                    string tipologia = !string.IsNullOrEmpty(frk._logo) ? string.Format("{0}", frk._logo) : "";
                    string titoloSpec = !string.IsNullOrWhiteSpace(frk._titoloSpecializ) ? string.Format("{0}", frk._titoloSpecializ) : "";

                    string key = "";
                    string value = "";

                    key = frk._descTitolo;
                    if (tipologia != "") key += " | " + tipologia;
                    if (titoloSpec != "") key += " | " + titoloSpec;

                    value += istituto;
                    if (localitaStudi != "") value += " - " + localitaStudi;

                    if ((!string.IsNullOrEmpty(voto)) && (!string.IsNullOrEmpty(frk._scala)))
                    {
                        value += string.Format("\r\nVoto: {0} su {2}{1}", voto, lode, frk._scala);
                    }
                    else if ((!string.IsNullOrEmpty(voto)) && (string.IsNullOrEmpty(frk._scala)))
                    {
                        value += string.Format("\r\nVoto: {0}{1}", voto, lode);
                    }
                    value += "\r\nConseguito nel " + frk._dataFine;

                    blockItemInfoList.Add(new BlockItemInfo
                    {
                        key = key,
                        value = value
                    });
                }
            }
            #endregion

            if (blockItemInfoList.Count > 0)
            {
                contentBlockInfoList.Add(new ContentBlockInfo(("TITOLI DI STUDIO E SPECIALIZZAZIONI").ToUpper(), blockItemInfoList));
            }

            #endregion

            #region CERTIFICAZIONI E PREMI

            blockItemInfoList = new List<RenderableItem>();

            cvEnt = new cv_ModelEntities();

            var certificazioni = cvEnt.TCVCertifica.Where(c => c.Matricola == selMatricola);
            var descalb = cvEnt.DAlboProf.ToList();
            foreach (var certif in certificazioni)
            {
                string dataini = null; ;
                string datafin = null;
                string datapub = null;
                string databrev = null;
                string dataalbo = null;
                if (certif.MeseIni != null)
                {
                    var mese = new DateTime(Convert.ToInt16(certif.AnnoIni), Convert.ToInt16(certif.MeseIni), 01).ToString("MMMM");
                    dataini = mese.Substring(0, 1).ToUpper() + mese.Substring(1) + " " + certif.AnnoIni;
                }
                if (certif.MeseFin != null)
                {
                    var mese = new DateTime(Convert.ToInt16(certif.AnnoFin), Convert.ToInt16(certif.MeseFin), 01).ToString("MMMM");
                    datafin = mese.Substring(0, 1).ToUpper() + mese.Substring(1) + " " + certif.AnnoFin;
                }
                if (certif.DataPubblica != null)
                {
                    datapub = new DateTime(Convert.ToInt16(certif.DataPubblica.Substring(0, 4)), Convert.ToInt16(certif.DataPubblica.Substring(4, 2)), Convert.ToInt16(certif.DataPubblica.Substring(6, 2))).ToString("dd/MM/yyyy");
                }
                if (certif.DataBrevetto != null)
                {
                    databrev = new DateTime(Convert.ToInt16(certif.DataBrevetto.Substring(0, 4)), Convert.ToInt16(certif.DataBrevetto.Substring(4, 2)), Convert.ToInt16(certif.DataBrevetto.Substring(6, 2))).ToString("dd/MM/yyyy");
                }
                if (certif.DataAlboProf != null)
                {
                    dataalbo = new DateTime(Convert.ToInt16(certif.DataAlboProf.Substring(0, 4)), Convert.ToInt16(certif.DataAlboProf.Substring(4, 2)), Convert.ToInt16(certif.DataAlboProf.Substring(6, 2))).ToString("dd/MM/yyyy");
                }
                cvModel.Certificazioni cer = new cvModel.Certificazioni(CommonManager.GetCurrentUserMatricola())
                {
                    _annoFin = certif.AnnoFin,
                    _annoIni = certif.AnnoIni,
                    _autCertifica = certif.AutCertifica,
                    _codAlboProf = certif.CodAlboProf,
                    _dataAlboProf = dataalbo,
                    _dataBrevetto = databrev,
                    _dataOraAgg = certif.DataOraAgg,
                    _dataPubblica = datapub,
                    _descAlboProf = descalb.Where(x => x.CodAlboProf == certif.CodAlboProf).Select(x => x.DescAlboProf).FirstOrDefault(),
                    _editorePubblica = certif.EditorePubblica,
                    _flagRegBrevetto = certif.FlagRegBrevetto,
                    _inventore = certif.Inventore,
                    _matricola = certif.Matricola,
                    _meseFin = certif.MeseFin,
                    _meseIni = certif.MeseIni,
                    _dataIni = dataini,
                    _dataFin = datafin,
                    _nomeCertifica = certif.NomeCertifica,
                    _noteAlboProf = certif.NoteAlboProf,
                    _noteBrevetto = certif.NoteBrevetto,
                    _notePubblica = certif.NotePubblica,
                    _numBrevetto = certif.NumBrevetto,
                    _numLicenza = certif.NumLicenza,
                    _pressoAlboProf = certif.PressoAlboProf,
                    _prog = certif.Prog,
                    _tipo = certif.Tipo,
                    _tipoAgg = certif.TipoAgg,
                    _tipoBrevetto = certif.TipoBrevetto,
                    _titoloPubblica = certif.TitoloPubblica,
                    _uffBrevetto = certif.UffBrevetto,
                    _urlBrevetto = certif.UrlBrevetto,
                    _urlCertifica = certif.UrlCertifica,
                    _urlPubblica = certif.UrlPubblica,
                    _tipoPubblicazione = certif.TipoPubblicazione,
                    _tipoPubRiferimento = certif.TipoPubRiferimento,
                    _riferimentoPub = certif.RiferimentoPub
                };

                string tipoCerticifazioneLabel = "";
                string annoCertLabel = "";
                string nomeCert = "";
                string dataLabel = "";
                string linkLabel = "";
                string autCertifica = "Autorità attestato";

                switch (cer._tipo)
                {
                    case "1":
                        tipoCerticifazioneLabel = "Attestato";
                        annoCertLabel = " conseguito nel";
                        nomeCert = "Attestato";
                        break;
                    case "2":
                        tipoCerticifazioneLabel = "Pubblicazione";
                        annoCertLabel = " effettuata nel";
                        break;
                    case "3":
                        tipoCerticifazioneLabel = "Brevetto";
                        annoCertLabel = " conseguito nel";
                        dataLabel = "Data concessione";
                        linkLabel = "Url Brevetto";
                        break;
                    case "4":
                        tipoCerticifazioneLabel = "Iscrizione";
                        annoCertLabel = " Albo dal";
                        break;
                    case "5":
                        tipoCerticifazioneLabel = "Premio";
                        annoCertLabel = " conseguito nel";
                        nomeCert = "Premio";
                        dataLabel = "Data conseguimento";
                        linkLabel = "Link approfondimenti";
                        autCertifica = "Ente erogatore";
                        break;
                }

                if (!string.IsNullOrEmpty(cer._annoFin))
                {
                    annoCertLabel = string.Format("{0} {1}", annoCertLabel, cer._annoFin);
                }
                else
                {
                    annoCertLabel = "";
                }

                string value = "";

                if (!string.IsNullOrEmpty(tipoCerticifazioneLabel))
                    value += "\r\n" + "Tipo Certificazione: " + tipoCerticifazioneLabel;
                if (!string.IsNullOrEmpty(cer._annoFin))
                    value += "\r\n" + annoCertLabel;
                if (!string.IsNullOrEmpty(cer._nomeCertifica))
                    value += "\r\n" + "Nome " + nomeCert + ": " + cer._nomeCertifica;
                if (!string.IsNullOrEmpty(cer._numLicenza))
                    value += "\r\n" + "Numero Licenza: " + cer._numLicenza;
                if (!string.IsNullOrEmpty(cer._autCertifica))
                    value += "\r\n" + autCertifica + ": " + cer._autCertifica;
                if (!string.IsNullOrEmpty(cer._urlCertifica))
                    value += "\r\n" + "Url attestato: " + cer._urlCertifica;
                if (!string.IsNullOrEmpty(cer._noteCertifica))
                    value += "\r\n" + "Descrizione: " + cer._noteCertifica;
                if (!string.IsNullOrEmpty(cer._tipoBrevetto))
                    value += "\r\n" + "Titolo Brevetto: " + cer._tipoBrevetto;
                if (!string.IsNullOrEmpty(cer._numBrevetto))
                    value += "\r\n" + "Numero Brevetto: " + cer._numBrevetto;
                if (!string.IsNullOrEmpty(cer._uffBrevetto))
                    value += "\r\n" + "Ufficio brevetti: " + cer._uffBrevetto;
                if (!string.IsNullOrEmpty(cer._inventore))
                    value += "\r\n" + "Inventore/co-inventori: " + cer._inventore;
                if (!string.IsNullOrEmpty(cer._dataBrevetto))
                    value += "\r\n" + dataLabel + ": " + cer._dataBrevetto;
                if (!string.IsNullOrEmpty(cer._urlBrevetto))
                    value += "\r\n" + linkLabel + ": " + cer._urlBrevetto;
                if (!string.IsNullOrEmpty(cer._noteBrevetto))
                    value += "\r\n" + "Descrizione: " + cer._noteBrevetto;
                if (!string.IsNullOrEmpty(cer._titoloPubblica))
                    value += "\r\n" + "Titolo: " + cer._titoloPubblica;
                if (!string.IsNullOrEmpty(cer._editorePubblica))
                    value += "\r\n" + "Editore: " + cer._editorePubblica;
                if (!string.IsNullOrEmpty(cer._urlPubblica))
                    value += "\r\n" + "Url: " + cer._urlPubblica;
                if (!string.IsNullOrEmpty(cer._notePubblica))
                    value += "\r\n" + "Descrizione: " + cer._notePubblica;
                if (cer.GiornoPubblicazione != null)
                    value += "\r\n" + "Giorno Pubblicazione: " + cer.GiornoPubblicazione.ToString();
                if (!string.IsNullOrEmpty(cer.MesePubblicazione))
                    value += "\r\n" + "Mese Pubblicazione: " + cer.MesePubblicazione;
                if (cer.AnnoPubblicazione != null)
                    value += "\r\n" + "Anno Pubblicazione: " + cer.AnnoPubblicazione.ToString();
                if (!string.IsNullOrEmpty(cer._descAlboProf))
                    value += "\r\n" + "Albo Professionale: " + cer._descAlboProf;
                if (!string.IsNullOrEmpty(cer._descAlboProf))
                    value += "\r\n" + "Data iscrizione: " + cer._dataAlboProf;
                if (!string.IsNullOrEmpty(cer._pressoAlboProf))
                    value += "\r\n" + "Presso: " + cer._pressoAlboProf;
                if (!string.IsNullOrEmpty(cer._noteAlboProf))
                    value += "\r\n" + "Descrizione: " + cer._noteAlboProf;
                if (!string.IsNullOrEmpty(cer._tipoPubblicazione))
                    value += "\r\n" + "Tipo pubblicazione : " + cer._tipoPubblicazione;
                if (!string.IsNullOrEmpty(cer._tipoPubRiferimento))
                    value += "\r\n" + "Tipo riferimento : " + cer._tipoPubRiferimento;
                if (!string.IsNullOrEmpty(cer._riferimentoPub))
                    value += "\r\n" + "Riferimento : " + cer._riferimentoPub;
                blockItemInfoList.Add(new BlockItemInfo
                {
                    key = "",
                    value = value
                });
            }

            if (blockItemInfoList.Count > 0)
            {
                contentBlockInfoList.Add(new ContentBlockInfo("CERTIFICAZIONI E PREMI", blockItemInfoList));
            }

            #endregion

            #region Corsi Rai
            blockItemInfoList = new List<RenderableItem>();
            List<V_CVCorsiRai> listaRai = new List<V_CVCorsiRai>();

            try
            {
                //listaRai = cvEnt.V_CVCorsiRai.Where(m => m.matricola == selMatricola).OrderByDescending(x => x.DataInizioDate).ToList();
                listaRai = RaiAcademyManager.GetCorsiFatti(selMatricola).OrderByDescending(x => x.DataInizioDate).ToList();
            }
            catch (Exception ex)
            {

            }

            foreach (var elem in listaRai)
            {
                if (!string.IsNullOrEmpty(elem.Societa) && !string.IsNullOrEmpty(elem.TitoloCorso))
                {
                    string DataInizio = String.Empty;
                    string DataFine = String.Empty;
                    if (!string.IsNullOrEmpty(elem.DataInizio))
                    {
                        DataInizio = "Dal " + elem.DataInizio;
                    }
                    if (!string.IsNullOrEmpty(elem.DataFine))
                    {
                        DataFine = "Al " + elem.DataFine;
                    }

                    string key = String.Empty;
                    string value = String.Empty;
                    string periodo = DataInizio + " " + DataFine;

                    if (!string.IsNullOrEmpty(elem.TitoloCorso))
                        key += "Titolo: " + elem.TitoloCorso; //key += "\r\n" + "Titolo: " + elem.TitoloCorso;
                    if (!string.IsNullOrEmpty(elem.Societa))
                        value += "\r\n" + "Società: " + elem.Societa;
                    if (!string.IsNullOrEmpty(elem.DataInizio))
                        value += "\r\n" + "Periodo: " + periodo;
                    if (elem.Durata != null)
                        value += "\r\n" + "Durata: " + elem.Durata.Value.ToString() + " ore";

                    blockItemInfoList.Add(new BlockItemInfo
                    {
                        key = key,
                        value = value.Trim()
                    });
                }
            }

            if (blockItemInfoList.Count > 0)
            {
                contentBlockInfoList.Add(new ContentBlockInfo("CORSI RAI (Dati prelevati dagli archivi aziendali)", blockItemInfoList));
            }

            #endregion

            #region CORSI DI FORMAZIONE

            blockItemInfoList = new List<RenderableItem>();

            cvEnt = new cv_ModelEntities();
            cvBox.formazione = new List<cvModel.Formazione>();

            List<TCVFormExRai> formaz = new List<TCVFormExRai>();

            formaz = cvEnt.TCVFormExRai.Where(x => x.Matricola == selMatricola).ToList();
            foreach (var elem in formaz)
            {
                cvModel.Formazione frk_formazione = new cvModel.Formazione(CommonManager.GetCurrentUserMatricola());

                frk_formazione._anno = elem.Anno;
                frk_formazione._codNazione = elem.CodNazione;
                frk_formazione._corso = elem.Corso;
                frk_formazione._dataOraAgg = elem.DataOraAgg;
                if (!string.IsNullOrEmpty(elem.CodNazione))
                {
                    frk_formazione._descNazione = cvEnt.DNazione.Where(a => a.COD_SIGLANAZIONE == elem.CodNazione).FirstOrDefault().DES_NAZIONE;
                }
                else
                {
                    frk_formazione._descNazione = "";
                }
                frk_formazione._durata = elem.Durata;
                frk_formazione._localitaCorso = elem.LocalitaCorso;
                frk_formazione._matricola = elem.Matricola;
                frk_formazione._note = elem.Note;
                frk_formazione._presso = elem.Presso;
                frk_formazione._prog = elem.Prog;
                frk_formazione._stato = elem.Stato;
                frk_formazione._tipoAgg = elem.TipoAgg;

                string key = String.Empty;
                string value = String.Empty;
                if (!string.IsNullOrEmpty(frk_formazione._corso))
                    key += "Titolo: " + frk_formazione._corso; //key += "\r\n" + "Titolo: " + frk_formazione._corso;
                if (!string.IsNullOrEmpty(frk_formazione._presso))
                    value += "\r\n" + "Società: " + frk_formazione._presso;
                if (!string.IsNullOrEmpty(frk_formazione._anno))
                    value += "\r\n" + "Anno: " + frk_formazione._anno;
                if (!string.IsNullOrEmpty(frk_formazione._durata))
                    value += "\r\n" + "Durata: " + frk_formazione._durata;
                if (!string.IsNullOrEmpty(frk_formazione._localitaCorso))
                    value += "\r\n" + "Città: " + frk_formazione._localitaCorso;
                if (!string.IsNullOrEmpty(frk_formazione._descNazione))
                    value += "\r\n" + "Paese: " + frk_formazione._descNazione;
                if (!string.IsNullOrEmpty(frk_formazione._note))
                    value += "\r\n" + "Note: " + frk_formazione._note;

                blockItemInfoList.Add(new BlockItemInfo
                {
                    key = key,
                    value = value.Trim()
                });
            }

            if (blockItemInfoList.Count > 0)
            {
                contentBlockInfoList.Add(new ContentBlockInfo("CORSI DI FORMAZIONE", blockItemInfoList));
            }

            #endregion

            #region COMPETENZE INFORMATICHE

            blockItemInfoList = new List<RenderableItem>();

            cvEnt = new cv_ModelEntities();
            cvBox.conoscenzeInformatiche = new List<cvModel.ConoscenzeInformatiche>();

            List<TCVConInfo> TCVConInfo = new List<TCVConInfo>();

            TCVConInfo = cvEnt.TCVConInfo.Where(x => x.Matricola == selMatricola).ToList();
            var allList = from conInfo in cvEnt.DConInfo
                          join gruppoConInfo in cvEnt.DGruppoConInfo on conInfo.CodGruppoConInfo equals gruppoConInfo.CodGruppoConInfo
                          select new
                          {
                              CodConInfo = conInfo.CodConInfo,
                              DescInfo = conInfo.DescConInfo,
                              CodGruppoConInfo = conInfo.CodGruppoConInfo,
                              CodPosizione = conInfo.CodPosizione,
                              DescGruppoConInfo = gruppoConInfo.DescGruppoConInfo,
                          };


            foreach (var elem in allList)
            {
                cvModel.ConoscenzeInformatiche know = new cvModel.ConoscenzeInformatiche();
                know._codConInfo = elem.CodConInfo;
                know._codGruppoConInfo = elem.CodGruppoConInfo;
                know._codPosizione = elem.CodPosizione;
                know._descConInfo = elem.DescInfo;
                know._descGruppoConInfo = elem.DescGruppoConInfo;
                var tmp = TCVConInfo.Where(x => x.CodConInfo == elem.CodConInfo);
                if (elem.CodGruppoConInfo != "99")
                {
                    if (tmp.Count() > 0)
                    {
                        know._selectedConInfo = true;
                        know._altraConInfo = tmp.First().AltraConInfo;
                        know._codConInfoLiv = tmp.First().CodConInfoLiv;
                        know._note = tmp.First().Note;
                        know._matricola = tmp.First().Matricola;
                        know._prog = tmp.First().Prog;
                    }
                    else
                    {
                        know._selectedConInfo = false;
                        know._altraConInfo = "";
                        know._codConInfoLiv = "";
                        know._note = "";
                        know._matricola = "";
                        know._prog = 0;
                    }
                }
                string value = string.Format("{0} | {1}",
                    know._codGruppoConInfo, know._descConInfo);

                int level;
                Int32.TryParse(know._codConInfoLiv, out level);

                if (level > 0)
                {
                    blockItemInfoList.Add(new KeyLevel
                    {
                        key = value,
                        level = level,
                        range = 3
                    });
                }
            }

            var tmp_lista9999 = TCVConInfo.Where(x => x.CodConInfo == "9999").ToList();
            foreach (var elem_2 in tmp_lista9999.OrderByDescending(a => a.CodConInfoLiv))
            {
                cvModel.ConoscenzeInformatiche know = new cvModel.ConoscenzeInformatiche();

                know._altraConInfo = elem_2.AltraConInfo;
                know._codConInfo = elem_2.CodConInfo;
                know._codConInfoLiv = elem_2.CodConInfoLiv;
                know._codGruppoConInfo = "99";
                know._dataOraAgg = elem_2.DataOraAgg;
                know._matricola = elem_2.Matricola;
                know._note = elem_2.Note;
                know._prog = elem_2.Prog;
                know._selectedConInfo = true;
                know._stato = elem_2.Stato;
                know._tipoAgg = elem_2.TipoAgg;

                string value = string.Format("{0} | {1}",
                    know._codGruppoConInfo, know._altraConInfo);

                int level;
                Int32.TryParse(know._codConInfoLiv, out level);

                if (level > 0)
                {
                    blockItemInfoList.Add(new KeyLevel
                    {
                        key = value,
                        level = level,
                        range = 3
                    });
                }
            }

            if (blockItemInfoList.Count > 0)
            {
                contentBlockInfoList.Add(new ContentBlockInfo("COMPETENZE INFORMATICHE", blockItemInfoList));
            }

            #endregion

            #region COMPETENZE DIGITALI

            blockItemInfoList = new List<RenderableItem>();

            cvEnt = new cv_ModelEntities();

            List<TCVCompDigit> competenze = cvEnt.TCVCompDigit.Where(m => m.Matricola == selMatricola).ToList();

            if (competenze.Count > 0)
            {
                foreach (var elem in competenze.OrderByDescending(a => a.CodCompDigitLiv))
                {
                    cvModel.CompetenzeDigitali frk_comepetenze = new cvModel.CompetenzeDigitali();
                    string descCompDigit, descCompDigitLiv, descCompDigitLivLunga;

                    frk_comepetenze._codCompDigit = elem.CodCompDigit;
                    frk_comepetenze._matricola = selMatricola;
                    frk_comepetenze._stato = elem.Stato;
                    frk_comepetenze._tipoAgg = elem.TipoAgg;
                    frk_comepetenze._dataOraAgg = elem.DataOraAgg;
                    frk_comepetenze._codCompDigitLiv = elem.CodCompDigitLiv;
                    if (elem.CodCompDigitLiv == "")
                    {
                        descCompDigitLiv = "";
                        descCompDigitLivLunga = "";
                    }
                    else
                    {
                        descCompDigitLiv = (from compDigitLiv in cvEnt.DCompDigitLiv
                                            where (compDigitLiv.CodCompDigit == elem.CodCompDigit && compDigitLiv.CodCompDigitLiv == elem.CodCompDigitLiv)
                                            select compDigitLiv.DescCompDigitLiv).First().ToString();
                        descCompDigitLivLunga = (from compDigitLivLunga in cvEnt.DCompDigitLiv
                                                 where (compDigitLivLunga.CodCompDigit == elem.CodCompDigit && compDigitLivLunga.CodCompDigitLiv == elem.CodCompDigitLiv)
                                                 select compDigitLivLunga.DescCompDigitLivLunga).First().ToString();
                    }
                    descCompDigit = (from compDigit in cvEnt.DCompDigit
                                     where compDigit.CodCompDigit == elem.CodCompDigit
                                     select compDigit.DescCompDigit).First().ToString();
                    frk_comepetenze._descCompDigit = descCompDigit;
                    frk_comepetenze._descCompDigitLiv = descCompDigitLiv;
                    frk_comepetenze._descCompDigitLivLunga = Regex.Replace(descCompDigitLivLunga, "<.*?>", String.Empty);

                    if (!string.IsNullOrEmpty(frk_comepetenze._descCompDigit))
                    {
                        string value = frk_comepetenze._descCompDigitLivLunga;

                        string key = frk_comepetenze._descCompDigit + " | ";
                        if (!String.IsNullOrWhiteSpace(frk_comepetenze._descCompDigitLiv))
                            key += frk_comepetenze._descCompDigitLiv;
                        else
                            key += "nd";

                        blockItemInfoList.Add(new BlockItemInfo
                        {
                            key = key,
                            value = value.Trim()
                        });
                    }
                }
            }

            if (blockItemInfoList.Count > 0)
            {
                contentBlockInfoList.Add(new ContentBlockInfo("COMPETENZE DIGITALI", blockItemInfoList));
            }

            #endregion

            #region COMPETENZE LINGUISTICHE

            blockItemInfoList = new List<RenderableItem>();

            cvEnt = new cv_ModelEntities();
            cvBox.lingue = new List<cvModel.Languages>();

            List<TCVLingue> lingue = (cvEnt.TCVLingue.Where(m => m.Matricola == selMatricola).OrderBy(y => y.CodLinguaLiv)).ToList();

            foreach (TCVLingue lang in lingue)
            {
                cvModel.Languages frk = new cvModel.Languages(CommonManager.GetCurrentUserMatricola());

                frk._altraLingua = lang.AltraLingua;
                frk._codLingua = lang.CodLingua;
                frk._codLinguaLiv = lang.CodLinguaLiv;
                frk._dataOraAgg = lang.DataOraAgg;
                frk._livAscolto = lang.LivAscolto;
                frk._livInterazione = lang.LivInterazione;
                frk._livLettura = lang.LivLettura;
                frk._livProdOrale = lang.LivProdOrale;
                frk._livScritto = lang.LivScritto;
                frk._matricola = lang.Matricola;
                frk._stato = lang.Stato;
                frk._tipoAgg = lang.TipoAgg;

                //Descrizione Lingua
                DLingua tmp_lingua = cvEnt.DLingua.Where(m => m.CodLingua == lang.CodLingua).First();
                frk._descLingua = tmp_lingua.DescLingua;

                //Flag dello Stato
                frk._flagStato = tmp_lingua.FlagStato;

                //Descrizione Livello di Lingua
                DLinguaLiv tmp_lingualiv = cvEnt.DLinguaLiv.Where(m => m.CodLinguaLiv == lang.CodLinguaLiv).First();
                frk._descLinguaLiv = tmp_lingualiv.DescLinguaLiv;

                if (!string.IsNullOrEmpty(frk._descLingua))
                {
                    string key = textInfo.ToTitleCase(frk._descLingua) + "\r\n";

                    string value = "";
                    value += " Ascolto " + (!string.IsNullOrEmpty(frk._livAscolto) ? frk._livAscolto : "nd") + " - ";
                    value += " Lettura " + (!string.IsNullOrEmpty(frk._livLettura) ? frk._livLettura : "nd") + " - ";
                    value += " Interazione " + (!string.IsNullOrEmpty(frk._livInterazione) ? frk._livInterazione : "nd") + " - ";
                    value += " Produzione orale " + (!string.IsNullOrEmpty(frk._livProdOrale) ? frk._livProdOrale : "nd") + " - ";
                    value += " Scritto " + (!string.IsNullOrEmpty(frk._livScritto) ? frk._livScritto : "nd");

                    blockItemInfoList.Add(new BlockItemInfo
                    {
                        key = key,
                        value = value
                    });
                }
            }

            if (blockItemInfoList.Count > 0)
            {
                contentBlockInfoList.Add(new ContentBlockInfo("COMPETENZE LINGUISTICHE", blockItemInfoList));
            }

            #endregion

            #region COMPETENZE SPECIALISTICHE

            blockItemInfoList = new List<RenderableItem>();

            cvEnt = new cv_ModelEntities();
            cvBox.competenzeSpecialistiche = new List<cvModel.CompetenzeSpecialistiche>();

            string figura_professionale = anagrafica._codiceFigProf;

            string fpForzata = GetFiguraProfessionaleForzata(selMatricola);
            if (fpForzata != null) figura_professionale = fpForzata;

            List<DConProf> DConProf =
                cvEnt.DConProf.Where(x => x.FiguraProfessionale == figura_professionale && x.Stato == "0").ToList();
            List<TCVConProf> TCVConProf = cvEnt.TCVConProf.Where(x => x.Matricola == selMatricola).ToList();

            foreach (var elem in DConProf.OrderBy(x => x.CodConProfAggr).ThenBy(y => y.Posizione))
            {
                cvModel.CompetenzeSpecialistiche frk_compSpec = new cvModel.CompetenzeSpecialistiche();

                var tmp_tcvConProf = TCVConProf.Where(x => x.CodConProf == elem.CodConProf);

                frk_compSpec._codConProf = elem.CodConProf;
                frk_compSpec._codConProfAggr = elem.CodConProfAggr;
                frk_compSpec._dataOraAgg = null;
                frk_compSpec._descConProf = elem.DescConProf;
                frk_compSpec._descConProfLunga = elem.DescConProfLunga;
                frk_compSpec._figuraProfessionale = figura_professionale;

                //setto il flag _isSelected
                if (tmp_tcvConProf.Count() > 0)
                {
                    frk_compSpec._isSelected = true;
                    frk_compSpec._codConProfLiv = tmp_tcvConProf.First().CodConProfLiv;
                    frk_compSpec._flagPrincipale = tmp_tcvConProf.First().FlagPrincipale;
                }
                else
                {
                    frk_compSpec._isSelected = false;
                    frk_compSpec._codConProfLiv = null;
                }

                //setto il flag _isTitle
                if ((elem.DescConProf.Contains("skill")))
                {
                    frk_compSpec._isTitle = true;
                }
                else
                {
                    frk_compSpec._isTitle = false;
                }

                frk_compSpec._matricola = selMatricola;
                frk_compSpec._posizione = Convert.ToInt32(elem.Posizione);
                frk_compSpec._prog = 1;
                frk_compSpec._stato = "";
                frk_compSpec._tipoAgg = "";

                string value = string.Format("{0} | {1}",
                    frk_compSpec._figuraProfessionale, frk_compSpec._descConProf);

                int level;
                Int32.TryParse(frk_compSpec._codConProfLiv, out level);

                if (level > 0)
                {
                    blockItemInfoList.Add(new KeyLevel
                    {
                        key = value,
                        level = level,
                        range = 3,
                        favorite = frk_compSpec._flagPrincipale == "1"
                    });
                }
                else
                {
                    blockItemInfoList.Add(new KeyValue
                    {
                        key = frk_compSpec._descConProf,
                        value = ""
                    });
                }
            }

            if (blockItemInfoList.Count > 0)
            {
                contentBlockInfoList.Add(new ContentBlockInfo("COMPETENZE SPECIALISTICHE", blockItemInfoList));
            }

            #endregion

            #region AREE DI INTERESSE

            blockItemInfoList = new List<RenderableItem>();

            cvEnt = new myRai.Data.CurriculumVitae.cv_ModelEntities();
            cvBox.areeInteresse = new List<cvModel.AreeInteresse>();

            string descAreaOrg, descServizio, descTipoDispo, descAreaGeo;

            List<TCVAreaIntAz> interesse = cvEnt.TCVAreaIntAz.Where(m => m.Matricola == selMatricola).ToList();

            foreach (var area in interesse)
            {
                cvModel.AreeInteresse frk = new cvModel.AreeInteresse(CommonManager.GetCurrentUserMatricola());

                frk._areeIntDispo = area.AreeIntDispo;
                frk._codAreaGeo = area.CodAreaGeo;
                frk._codAreaOrg = area.CodAreaOrg;
                frk._codServizio = area.CodServizio;
                frk._codTipoDispo = area.CodTipoDispo;
                frk._dataOraAgg = area.DataOraAgg;
                frk._flagEsteroDispo = area.FlagEsteroDispo;
                frk._matricola = area.Matricola;
                frk._profIntDispo = area.ProfIntDispo;
                frk._prog = area.Prog;
                frk._stato = area.Stato;
                frk._tipoAgg = area.TipoAgg;

                var frk_geogio = cvEnt.DAreaGeoGio.Where(m => m.CodAreaGeoGio == area.CodAreaGeo).ToList();

                if (frk_geogio.Count > 0)
                {
                    descAreaGeo = frk_geogio.First().DesAreaGeoGio;
                }
                else
                {
                    descAreaGeo = null;
                }

                descAreaOrg = null;
                var frk_org = frk.AreaInteresseItems.FirstOrDefault(x => x.Value == area.CodAreaOrg);

                if (frk_org != null)
                    descAreaOrg = frk_org.Text;

                var frk_tipodisto = cvEnt.DTipoDispo.Where(m => m.CodTipoDispo == area.CodTipoDispo).ToList();

                if (frk_tipodisto.Count > 0)
                {
                    descTipoDispo = frk_tipodisto.First().DescTipoDispo;
                }
                else
                {
                    descTipoDispo = null;
                }

                var frk_servizio = cvEnt.VDServizioCV.Where(m => m.Codice == area.CodServizio).ToList();

                if (frk_servizio.Count > 0)
                {
                    descServizio = frk_servizio.First().Descrizione;
                }
                else
                {
                    descServizio = null;
                }

                frk._descAreaGeo = descAreaGeo;
                frk._descAreaOrg = descAreaOrg;
                frk._descServizio = descServizio;
                frk._descTipoDispo = descTipoDispo;

                List<string> elencoSedi = new List<string>();
                //cvEnt.TCVAreaIntAzEstero.Where(x => x.Matricola == area.Matricola && x.Prog == area.Prog).Select(y => y.Codice).ForEach(delegate (string codice)
                //{
                //    var sede = cvEnt.DTabellaCV.FirstOrDefault(x => x.NomeTabella == "LocalitaEsp" && x.Codice == codice);
                //    if (sede != null)
                //        elencoSedi.Add(sede.Descrizione);
                //});

                foreach (var item in cvEnt.TCVAreaIntAzEstero.Where(x => x.Matricola == area.Matricola && x.Prog == area.Prog).Select(y => y.Codice))
                {
                    var sede = cvEnt.DTabellaCV.FirstOrDefault(x => x.NomeTabella == "LocalitaEsp" && x.Codice == item);
                    if (sede != null)
                        elencoSedi.Add(sede.Descrizione);
                }


                string key = string.Format("Area: {0}", frk._descAreaOrg);
                string value = string.Format("Note: {0}", (frk._areeIntDispo != null ? frk._areeIntDispo.Trim() : ""));
                value += string.Format("\r\nProfili di interesse: {0}", (frk._profIntDispo != null ? frk._profIntDispo.Trim() : ""));
                value += string.Format("\r\nInteresse allo svolgimento di mansioni diverse da quelle attualmente svolte, nel rispetto delle competenze professionali acquisite: {0}", frk._descTipoDispo);
                value += string.Format("\r\nInteresse a svolgere la propria attività anche all'estero: {0}", frk._flagEsteroDispo == "S" ? "Sì" : "No");
                value += string.Format("\r\nPresso: {0}", string.Join(", ", elencoSedi));

                blockItemInfoList.Add(new BlockItemInfo()
                {
                    key = key,
                    value = value
                });
            }

            if (blockItemInfoList.Count > 0)
            {
                contentBlockInfoList.Add(new ContentBlockInfo("AREE DI INTERESSE AZIENDALE", blockItemInfoList));
            }

            #endregion

            #region SU DI TE

            blockItemInfoList = new List<RenderableItem>();
            matricola = anagrafica._matricola;
            cvEnt = new cv_ModelEntities();
            TCVAltreInf altreInformazioni;
            List<TCVAltreInfPat> listaPatenti = cvEnt.TCVAltreInfPat.Where(x => x.Matricola == matricola).ToList();
            try
            {
                altreInformazioni = cvEnt.TCVAltreInf.Where(x => x.Matricola == matricola).FirstOrDefault();
            }
            catch (Exception ex)
            {
                altreInformazioni = null;
            }

            if (altreInformazioni != null)
            {
                cvModel.AltreInfo frk_altreInfo = new cvModel.AltreInfo(CommonManager.GetCurrentUserMatricola());
                frk_altreInfo._dataOraAgg = altreInformazioni.DataOraAgg;
                frk_altreInfo._email = altreInformazioni.EMail;
                frk_altreInfo._matricola = matricola;
                frk_altreInfo._note = altreInformazioni.Note;
                frk_altreInfo._numTel1 = altreInformazioni.NumTel1;
                frk_altreInfo._numTel2 = altreInformazioni.NumTel2;
                frk_altreInfo._sitoWeb = altreInformazioni.Sitoweb;
                frk_altreInfo._stato = altreInformazioni.Stato;
                frk_altreInfo._tipoAgg = altreInformazioni.TipoAgg;
                frk_altreInfo._tipoTel1 = altreInformazioni.TipoTel1;
                frk_altreInfo._tipoTel2 = altreInformazioni.TipoTel2;

                frk_altreInfo._tipoPatente = new List<DTipoPatente>();

                if (listaPatenti.Count > 0)
                {
                    foreach (var elem in listaPatenti)
                    {
                        DTipoPatente item = new DTipoPatente();
                        item = cvEnt.DTipoPatente.Where(x => x.CodTipoPatente == elem.CodTipoPatente).First();
                        frk_altreInfo._tipoPatente.Add(item);
                    }
                }
                else
                {
                    frk_altreInfo._tipoPatente = null;
                }
                cvBox.altreInformazioni = frk_altreInfo;

                string key = "";
                string value = "";

                if (!String.IsNullOrEmpty(frk_altreInfo._email))
                {
                    key = string.Format("Email personale: {0}\r\n", frk_altreInfo._email);
                }
                if (!String.IsNullOrEmpty(frk_altreInfo._numTel1) && !String.IsNullOrEmpty(frk_altreInfo._tipoTel1))
                {
                    key += string.Format("{0}: {1}\r\n", frk_altreInfo._tipoTel1, frk_altreInfo._numTel1);
                }
                if (!String.IsNullOrEmpty(frk_altreInfo._numTel2) && !String.IsNullOrEmpty(frk_altreInfo._tipoTel2))
                {
                    key += string.Format("{0}: {1}\r\n", frk_altreInfo._tipoTel2, frk_altreInfo._numTel2);
                }
                if (!String.IsNullOrEmpty(frk_altreInfo._sitoWeb))
                {
                    key += string.Format("Sito web/blog/pagina social: {0}\r\n", frk_altreInfo._sitoWeb);
                }
                if (!String.IsNullOrEmpty(frk_altreInfo._indirizzo_domicilio))
                {
                    key += string.Format("Indirizzo domicilio: {0}\r\n", frk_altreInfo._indirizzo_domicilio);
                }
                if (!String.IsNullOrEmpty(frk_altreInfo._indirizzo_residenza))
                {
                    key += string.Format("Indirizzo residenza: {0}\r\n", frk_altreInfo._indirizzo_residenza);
                }

                if (frk_altreInfo._tipoPatente != null && frk_altreInfo._tipoPatente.Any())
                {
                    key += string.Format("Patente:");
                    frk_altreInfo._tipoPatente.ForEach(p =>
                    {
                        key += String.Format(" {0} ", p.DescTipoPatente);
                    });

                    key += "\r\n";
                }

                if (!String.IsNullOrEmpty(frk_altreInfo._note))
                {
                    key += string.Format("Ulteriori informazioni: {0}", frk_altreInfo._note);
                }

                if (!String.IsNullOrEmpty(key))
                {
                    blockItemInfoList.Add(new BlockItemInfo()
                    {
                        key = key
                    });
                }

                if (blockItemInfoList.Count > 0)
                {
                    contentBlockInfoList.Add(new ContentBlockInfo("SU DI TE", blockItemInfoList));
                }
            }
            #endregion

            #region Allegati
            blockItemInfoList = new List<RenderableItem>();
            var allegati = cvEnt.TCVAllegato.Where(x => x.Matricola == selMatricola && x.Stato != "#").ToList();

            string homeUrl = System.Web.HttpContext.Current.Request.Url.Scheme + "://" + System.Web.HttpContext.Current.Request.Url.Authority;

            foreach (TCVAllegato all in allegati)
            {
                cvModel.Allegati frk_all = new cvModel.Allegati();

                frk_all._contentType = all.ContentType;
                frk_all._dataOraAgg = all.DataOraAgg;
                frk_all._id = all.Id;
                frk_all._idBox = all.Id_box;
                frk_all._matricola = selMatricola;
                frk_all._name = all.Name;
                frk_all._pathName = all.Path_name;
                frk_all._size = all.Size;
                frk_all._stato = all.Stato;
                frk_all._tipoAgg = all.TipoAgg;
                frk_all._note = all.Note;

                BlockItemInfo block = new BlockItemInfo();
                block.key = "";
                string configPath = GetShareAllegati();

                string attachmentPath = "";

                //if (string.IsNullOrEmpty(this._serverPath))
                //{
                //    attachmentPath = Path.Combine(Server.MapPath("~"), configPath, frk_all._matricola, frk_all._pathName);
                //}
                //else
                //{
                //    attachmentPath = Path.Combine(this._serverPath, configPath, frk_all._matricola, frk_all._pathName);
                //}
                attachmentPath = Path.Combine(serverMapPath, configPath, frk_all._matricola, frk_all._pathName);

                if (frk_all._contentType.Contains("image") && System.IO.File.Exists(attachmentPath))
                {
                    var img = Image.GetInstance(attachmentPath);
                    float maxSize = PageSize.A4.Width - indentationLeft - 48;
                    if (img.Width > maxSize)
                        img.ScaleToFit(maxSize, maxSize);
                    img.SpacingBefore = 5F;
                    img.Alt = frk_all._name;

                    block.chk = new Chunk(img, 0, 0, true); ;
                }
                else if (frk_all._contentType == "website")
                {
                    string link = frk_all._pathName;
                    block.chk = new Chunk("Clicca qui per vedere " + frk_all._name, new FontManager("", BaseColor.BLACK).Italic).SetAction(new PdfAction(link, false));
                }
                else
                {
                    string link = homeUrl + "/cv_online/GetFile/" + frk_all._id;
                    block.chk = new Chunk("Clicca qui per vedere " + frk_all._name, new FontManager("", BaseColor.BLACK).Italic).SetAction(new PdfAction(link, false));
                }

                block.key = "Allegato " + frk_all._name;
                block.value = frk_all._note;

                blockItemInfoList.Add(block);
            }

            if (blockItemInfoList.Count > 0)
            {
                contentBlockInfoList.Add(new ContentBlockInfo("ALLEGATI", blockItemInfoList));
            }
            #endregion

            RightContentManager rm = new RightContentManager(writer.DirectContentUnder, document, contentBlockInfoList);

            rm.Title = string.Format("{0} {1}", anagrafica._nome, anagrafica._cognome);

            rm.SubTitle = anagrafica._figProfessionale;

            rm.IndentationLeft = indentationLeft;

            rm.Render();

            document.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return workStream;
        }

        private static void PdfExperience(cv_ModelEntities cvEnt, string codFigProf, List<RenderableItem> blockItemInfoList, TCVEsperExRai esp)
        {
            cvModel.Experiences frk_esp = new cvModel.Experiences(CommonManager.GetCurrentUserMatricola());
            frk_esp._areaAtt = esp.AreaAtt;
            frk_esp._codContinente = esp.CodContinente;

            frk_esp._descContinente = "";
            if (esp.CodContinente != null)
            {
                var tmp = cvEnt.DContinente.FirstOrDefault(x => x.CodContinente == esp.CodContinente);
                if (tmp != null)
                    frk_esp._descContinente = tmp.DesContinente;
            }

            frk_esp._codRedazione = esp.CodRedazione;

            frk_esp._descRedazione = "";
            if (esp.CodRedazione != null)
            {
                var tmp = cvEnt.DRedazione.FirstOrDefault(x => x.CodRedazione == esp.CodRedazione);
                if (tmp != null)
                    frk_esp._descRedazione = tmp.DesRedazione;
            }

            if (esp.CodiceFiguraProf != null)
            {
                var figuraProf = frk_esp.ListeFigureRai.FirstOrDefault(a => a.CodiceFiguraPro == esp.CodiceFiguraProf);
                if (figuraProf != null)
                    frk_esp._codiceFiguraProf = figuraProf.DescriFiguraPro;
                else
                    frk_esp._codiceFiguraProf = "";
            }
            else if (esp.CodFigProExtra != null && esp.CodFigProExtra != "" && esp.CodFigProExtra != "-1")
            {
                frk_esp._codiceFiguraProf = cvEnt.DTabellaCV.Where(x => x.NomeTabella == "FigProExtra" && x.Codice == esp.CodFigProExtra).FirstOrDefault().Descrizione.ToString();
            }
            else
            {
                frk_esp._codiceFiguraProf = "";
            }

            frk_esp._codDirezione = esp.CodDirezione;

            if (esp.CodDirezione == "-1")
            {
                frk_esp._direzione = esp.Direzione;
            }
            else
            {
                frk_esp._direzione = "";
                if (esp.CodDirezione != null && esp.CodDirezione != "")
                {
                    var tmp = cvEnt.VDServizioCV.FirstOrDefault(x => x.Codice == esp.CodDirezione);
                    if (tmp != null)
                        frk_esp._direzione = tmp.Descrizione;
                }
            }

            frk_esp._codSocieta = esp.CodSocieta;

            if ((esp.CodSocieta != null) && (esp.CodSocieta != "") && (esp.CodSocieta != "-1"))
            {
                var tmp = cvEnt.VDSocieta.FirstOrDefault(x => x.Codice == esp.CodSocieta);
                if (tmp != null)
                    frk_esp._societa = tmp.Descrizione;
                else
                    frk_esp._societa = esp.Societa;
            }
            else
            {
                frk_esp._societa = esp.Societa;
            }

            frk_esp._stato = esp.Stato;
            frk_esp._dataFine = esp.DataFine;
            frk_esp._dataInizio = esp.DataInizio;
            frk_esp._dataOraAgg = esp.DataOraAgg;
            frk_esp._descrizioneEsp = esp.DescrizioneEsp;
            frk_esp._flagEspEstero = esp.FlagEspEstero;
            frk_esp._flagEspRai = esp.FlagEspRai;

            if ((codFigProf == "MAA") || (codFigProf == "MBA"))
            {
                frk_esp._isGiornalista = "1";
            }
            else
            {
                frk_esp._isGiornalista = "0";
            }

            frk_esp._localitaEsp = esp.LocalitaEsp;
            frk_esp._matricola = esp.Matricola;
            frk_esp._nazione = esp.Nazione;
            frk_esp._prog = esp.Prog;
            frk_esp._tipoAgg = esp.TipoAgg;
            frk_esp._titoloEspQual = esp.TitoloEspQual;
            frk_esp._ultRuolo = esp.UltRuolo;

            if (esp.CodBudgetGest != null && esp.CodBudgetGest != "" && esp.CodBudgetGest != "-1")
            {
                var tmp = cvEnt.DTabellaCV.FirstOrDefault(x => x.NomeTabella == "Budget" && x.Codice == esp.CodBudgetGest);
                if (tmp != null)
                    frk_esp._budgetGest = tmp.Descrizione;
                else
                    frk_esp._budgetGest = "";
            }
            else
            {
                frk_esp._budgetGest = "";

            }

            if (esp.CodSettore != null && esp.CodSettore != "" && esp.CodSettore != "-1")
            {
                var tmp = cvEnt.DTabellaCV.FirstOrDefault(x => x.NomeTabella == "Settore" && x.Codice == esp.CodSettore);
                if (tmp != null)
                    frk_esp._codIndustry = tmp.Descrizione;
                else
                    frk_esp._codIndustry = "";
            }
            else if (esp.CodSettore == "-1")
            {
                frk_esp._codIndustry = esp.Settore;
            }
            else
            {
                frk_esp._codIndustry = "";

            }

            frk_esp._risorseGest = "";
            if (esp.CodRisorseGest != null && esp.CodRisorseGest != "" && esp.CodRisorseGest != "-1")
            {
                var tmp = cvEnt.DTabellaCV.FirstOrDefault(x => x.NomeTabella == "Risorse" && x.Codice == esp.CodRisorseGest);
                if (tmp != null)
                    frk_esp._risorseGest = tmp.Descrizione;

            }

            frk_esp._procura = esp.CodProcura;

            if (!string.IsNullOrEmpty(frk_esp._ultRuolo))
            {
                bool dataInizioValida = !String.IsNullOrWhiteSpace(frk_esp._dataInizio);// && frk_esp._dataInizio.Trim().Length == 8;

                int fyear = 0;
                int fmonth = 0;
                int fday = 0;

                DateTime fdate;
                string fromMonth = "";

                string dataInizio = "";
                string dataFine = "";

                string txInizio = "dal";
                string txFine = "al";

                Int32.TryParse(frk_esp._annoInizio, out fyear);
                Int32.TryParse(frk_esp._meseInizio, out fmonth);
                Int32.TryParse(frk_esp._giornoInizio, out fday);

                if (fday > 0)
                {
                    try
                    {
                        fdate = new DateTime(fyear, fmonth, fday);
                    }
                    catch
                    {
                        dataInizioValida = false;
                    }
                }

                if (dataInizioValida)
                {
                    fromMonth = CommonManager.TraduciMeseDaNumLett(frk_esp._meseInizio);

                    if (fday > 0)
                    {
                        if (fday == 1 || fday == 8 || fday == 11)
                            dataInizio = "dall'";
                        else
                            dataInizio = "dal ";
                        dataInizio += fday + " " + fromMonth + " " + fyear;
                    }
                    else if (fmonth > 0)
                    {
                        dataInizio = "da " + fromMonth + " " + fyear;
                    }
                    else
                    {
                        dataInizio = "dal " + fyear;
                    }
                }

                string value = "";
                if (!string.IsNullOrEmpty(frk_esp._societa))
                    value += "\r\n" + "Società: " + frk_esp._societa;
                if (!string.IsNullOrEmpty(frk_esp._codIndustry))
                    value += "\r\n" + "Settore: " + frk_esp._codIndustry;
                if (!string.IsNullOrEmpty(frk_esp._direzione))
                    value += "\r\n" + "Direzione: " + frk_esp._direzione;
                if (!string.IsNullOrEmpty(frk_esp._descRedazione))
                    value += "\r\n" + "Redazione: " + frk_esp._descRedazione;
                if (frk_esp._dataFine == null)
                {
                    if (dataInizioValida)
                        value += "\r\n" + "Periodo: " + dataInizio + " - in corso";
                    else
                        value += "\r\n" + "Periodo: non disponibile";
                }
                else
                {
                    bool dataFineValida = true;

                    int tyear = 0;
                    int tmonth = 0;
                    int tday = 0;
                    Int32.TryParse(frk_esp._annoFine, out tyear);
                    Int32.TryParse(frk_esp._meseFine, out tmonth);
                    Int32.TryParse(frk_esp._giornoFine, out tday);

                    if (tday > 0)
                    {
                        try
                        {
                            fdate = new DateTime(tyear, tmonth, tday);
                        }
                        catch
                        {
                            dataFineValida = false;
                        }
                    }

                    if (dataFineValida)
                    {
                        string toMonth = CommonManager.TraduciMeseDaNumLett(frk_esp._meseFine);

                        if (tday == 1 ||
                            tday == 8 ||
                            tday == 11)
                            txFine = "all'";

                        if (tday > 0)
                        {
                            if (tday == 1 || tday == 8 || tday == 11)
                                dataFine = "all'";
                            else
                                dataFine = "al ";
                            dataFine += tday + " " + toMonth + " " + tyear;
                        }
                        else if (tmonth > 0)
                        {
                            dataFine = "a " + toMonth + " " + tyear;
                        }
                        else
                        {
                            dataFine = "al " + tyear;
                        }
                    }

                    if (dataInizioValida && dataFineValida)
                        value += "\r\n" + "Periodo: " + dataInizio + " " + dataFine;
                    else
                        value += "\r\n" + "Periodo: non disponibile";

                    // se la data di fine è il mese e l'anno corrente ed
                    // il giorno termine esperienza è superiore alla data odierna,
                    // allora verrà stampata la dicitura "attuale da mese anno".
                    // Altrimenti verrà stampata la dicitura "da mese anno a mese anno"
                    // in precedenza calcolata
                    if (dataInizioValida && dataFineValida &&
                        tmonth.Equals(DateTime.Now.Month) &&
                        tday >= DateTime.Now.Day &&
                        tyear.Equals(DateTime.Now.Year))
                    {
                        value += "\r\n" + "Periodo: attuale " + dataInizio;
                    }
                }

                if (!string.IsNullOrEmpty(frk_esp._codiceFiguraProf))
                    value += "\r\n" + "Figura Professionale: " + frk_esp._codiceFiguraProf;
                if (!string.IsNullOrEmpty(frk_esp._localitaEsp))
                    value += "\r\n" + "Località: " + frk_esp._localitaEsp;
                if (!string.IsNullOrEmpty(frk_esp._nazione))
                    value += "\r\n" + "Nazione: " + frk_esp._nazione;
                if (!string.IsNullOrEmpty(frk_esp._areaAtt))
                    value += "\r\n" + "Area Attività: " + frk_esp._areaAtt;

                if (!string.IsNullOrEmpty(frk_esp._titoloEspQual))
                    value += "\r\n" + "Titolo Esperienza qualificante: " + frk_esp._titoloEspQual;
                if (!string.IsNullOrEmpty(frk_esp._descrizioneEsp))
                    value += "\r\n" + "Descrizione sintetica dell'esperienza: " + frk_esp._descrizioneEsp;
                if (!string.IsNullOrEmpty(frk_esp._budgetGest))
                    value += "\r\n" + "Budget: " + frk_esp._budgetGest;
                if (!string.IsNullOrEmpty(frk_esp._procura))
                    value += frk_esp._procura == "1" ? "\r\n" + "Procura: No" : "\r\n" + "Procura: Si";
                if (!string.IsNullOrEmpty(frk_esp._risorseGest))
                    value += "\r\n" + "Risorse Gestite/Coordinate: " + frk_esp._risorseGest;

                string key = String.Empty;
                if (!string.IsNullOrEmpty(frk_esp._areaAtt))
                    key += "Attività: " + frk_esp._ultRuolo;

                blockItemInfoList.Add(new BlockItemInfo
                {
                    key = key,
                    value = value.Trim()
                });
            }
        }
    }
}
