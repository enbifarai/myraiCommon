using System;
using System.Collections.Generic;
using System.Linq;
using myRai.Business;
using myRai.DataAccess;
using myRai.Models;
using myRaiCommonModel;
using myRaiData;
using myRaiHelper;
using digigappWS_ws1 = MyRaiServiceInterface.it.rai.servizi.digigappws;

namespace myRaiCommonManager
{
    public class ScrivaniaManager
    {
        public static Tuple<int, int> GetTotaleEccezioniDaApprovare()
        {
            ModelDash model = new ModelDash();

            model = HomeManager.GetDaApprovareModel(model, false, 0, "", 10);

            int qtot = model.elencoProfilieSedi.elencoSediEccezioni.SelectMany(x => x.eccezionidaValidare).Count();
            int qsca = model.elencoProfilieSedi.elencoSediEccezioni.SelectMany(x => x.eccezionidaValidare.Where(a => a.IsOverdue)).Count();
            return new Tuple<int, int>(qtot, qsca);
        }

        public static string SetMessaggioHome()
        {
            string[] mess= CommonManager.GetParametri<string>(EnumParametriSistema.MessaggioHome);
            if (mess == null) return null;

            if (String.IsNullOrWhiteSpace(mess[0]) || mess[1] == "OFF") return null;


            if (mess[1].Trim() == "ON")
                return mess[0];

            if (mess[1] == "AB" && Utente.IsAbilitatoGapp())
                return mess[0];
            else
                return null;

        }

        public static digigappWS_ws1.Evidenza[] SopprimiQuadrateDaDipendente(digigappWS_ws1.Evidenza[] giornate)
        {
            if (giornate == null || giornate.Length == 0) return giornate;
            string matr=CommonManager.GetCurrentUserMatricola();
            var L = giornate.ToList();
            L.RemoveAll(giornata =>
                          new digiGappEntities().MyRai_GiornateQuadrate.Any(gquad =>
                                    gquad.matricola == matr && gquad.data == giornata.data));
            return L.ToArray();
        }

        public static digigappWS_ws1.Evidenza[] MarcaturaQuadrabili(digigappWS_ws1.Evidenza[] giornate)
        {
            if (giornate == null || giornate.Length == 0) return giornate;
            foreach (var item in giornate)
            {
                item.IsQuadrabileDaDipendente = false;

                if (item.TipoEcc == digigappWS_ws1.TipoEccezione.MaggiorPresenza)
                    item.IsQuadrabileDaDipendente = true;

                if (Utente.GetQuadratura() ==  Quadratura.Settimanale && item.TipoEcc == digigappWS_ws1.TipoEccezione.Carenza)
                    item.IsQuadrabileDaDipendente = true;

                if (Utente.TipoDipendente()!="G" && item.TipoEcc == digigappWS_ws1.TipoEccezione.Incongruenza)
                    item.IsQuadrabileDaDipendente = true;

            }
            return giornate;
        }

        public static digigappWS_ws1.Evidenza[] ClearCarenze( digigappWS_ws1.Evidenza[] giornate)
        {
            if (giornate == null || giornate.Length == 0) return giornate;
            if (giornate.Where(x => x.TipoEcc == digigappWS_ws1.TipoEccezione.Carenza).Count() > 0)
            {

                List<DateTime> LDateDaRimuovere = new List<DateTime>();
                DateTime Dmin = giornate.OrderBy(x => x.data).Select(x => x.data).First();
                DateTime Dmax = giornate.OrderByDescending(x => x.data).Select(x => x.data).First();
                MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client wcf1 = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
                wcf1.ClientCredentials.Windows.ClientCredential = CommonHelper.GetUtenteServizioCredentials();

                //var mod = new Models.DettaglioSettimanaleModel(
                //      wcf1.PresenzeSettimanali(CommonManager.GetCurrentUserMatricola(),
                //    Dmin.ToString("ddMMyyyy"), Dmax.ToString("ddMMyyyy")));
                var mod = new DettaglioSettimanaleModel(
                wcf1.GetPresenzeSettimanaliProtected(CommonManager.GetCurrentUserMatricola(),
                Dmin.ToString("ddMMyyyy"), Dmax.ToString("ddMMyyyy"),Utente.DataInizioValidita(),Dmin));

                if (mod.Settimana != null)
                {
                    foreach (var g in giornate
                        .Where(x => x.TipoEcc == digigappWS_ws1.TipoEccezione.Carenza))
                    {
                        var dettDay = mod.Settimana.Where(x => x.GiornoData == g.data).FirstOrDefault();
                        if (dettDay != null)

                        {
                            if (!dettDay.Delta.StartsWith("-"))
                                LDateDaRimuovere.Add(g.data);
                            else
                                if (g.carenza != dettDay.Delta) g.CarenzaEffettiva = dettDay.Delta;
                        }
                    }
                }

                if (LDateDaRimuovere.Count() > 0)
                {
                    var l = giornate.ToList();
                    l.RemoveAll(x => LDateDaRimuovere.Contains(x.data) && x.TipoEcc == digigappWS_ws1.TipoEccezione.Carenza);
                    giornate = l.ToArray();
                }
            }
            return giornate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="readFromSession">Se true, legge i risultati della chiamata ListeEvidenze dalla sessione </param>
        /// <returns></returns>
        public static ModelDash GetTotaliEvidenze(ModelDash model, bool readFromSession = false)
        {
            digigappWS_ws1.WSDigigapp datiBack_ws1 = new digigappWS_ws1.WSDigigapp( );
            datiBack_ws1.Credentials = CommonHelper.GetUtenteServizioCredentials();
            string dateBack = Utente.GetDateBackPerEvidenze( );

            if (readFromSession)
            {
                var getListaEvidenze = SessionHelper.Get(SessionVariables.ListaEvidenzeScrivania);

                if (getListaEvidenze != null)
                {
                    var sessionData = (SessionListaEvidenzeModel)getListaEvidenze;
                    model.listaEvidenze = sessionData.ListaEvidenze;
                }
                else
                {
                    model.listaEvidenze = new digigappWS_ws1.monthResponseEccezione( );
                }
            }
            else
            {
                model.listaEvidenze=ServiceWrapper.ListaEvidenzWrapper(datiBack_ws1, CommonManager.GetCurrentUserMatricola(),
                   dateBack,
                   DateTime.Now.AddDays(-1).ToString("ddMMyyyy"), 70);
                //model.listaEvidenze = datiBack_ws1.ListaEvidenze( CommonManager.GetCurrentUserMatricola( ) ,
                //   dateBack ,
                //   DateTime.Now.AddDays( -1 ).ToString( "ddMMyyyy" ) , 70 );
            }

            if (model.listaEvidenze.data != null)
            {
                if (myRai.Models.Utente.GetQuadratura() == Quadratura.Settimanale)
                {
                    using (digiGappEntities db = new digiGappEntities())
                    {
                        string sede = Utente.SedeGapp(DateTime.Now);
                        var listagiornate = model.listaEvidenze.data.giornate.ToList();
                        //var listadate = db.DIGIRESP_Archivio_PDF.Where(y => y.sede_gapp == sede && y.tipologia_pdf == "P").ToList();
                        //if (listadate.Count > 0)
                        //{
                        //    DateTime dmax = listadate.OrderByDescending(x => x.data_fine).Select(x => x.data_fine).First();
                        //    listagiornate.RemoveAll(x => x.TipoEcc == digigappWS_ws1.TipoEccezione.Carenza && x.data <= dmax);
                        //}
                        var listadate = db.DIGIRESP_Archivio_PDF.Where(y => y.sede_gapp == sede && y.tipologia_pdf == "P").Select(x => x.data_fine);
                        if (listadate.Count() > 0)
                        {
                            DateTime dmax = listadate.Max();
                            listagiornate.RemoveAll(x => x.TipoEcc == digigappWS_ws1.TipoEccezione.Carenza && x.data <= dmax);
                        }
                        // listagiornate.RemoveAll(x =>x.TipoEcc==digigappWS_ws1.TipoEccezione.Carenza && x.data <= (db.DIGIRESP_Archivio_PDF.Where(y => y.sede_gapp == sede && y.tipologia_pdf == "P").Max(z => z.data_fine)));
                        model.listaEvidenze.data.giornate = listagiornate.ToArray();
                    }

                    model.listaEvidenze.data.giornate = ClearCarenze(model.listaEvidenze.data.giornate);
                }

                model.listaEvidenze.data.giornate = SopprimiQuadrateDaDipendente(model.listaEvidenze.data.giornate);

                foreach (var item in model.listaEvidenze.data.giornate)
                {
                    if (item.TipoEcc == digigappWS_ws1.TipoEccezione.TimbratureInSW && Utente.TipoDipendente() != "G" && Utente.TipoDipendente() != "D")
                    {
                        model.TotaleEvidenzeDaGiustificare++;
                        model.TotaleEvidenzeDaGiustificareSWTIM++;
                        //model.TotaleEvidenzeDaGiustificareSoloAssIng++;
                    }
                    // se il dipendente non è giornalista, allora verranno conteggiati 
                    // anche i transiti sfasati
                        if ( Utente.TipoDipendente() != "G" )
                    {
						if ( item.TipoEcc == digigappWS_ws1.TipoEccezione.AssenzaIngiustificata && item.timbrature != null && item.timbrature.Count() > 0 )
                        {
                            model.TotaleEvidenzeDaGiustificare++;
                            model.TotaleEvidenzeDaGiustificareSoloAssIng++;
                        }
                    }

                    if (item.TipoEcc == digigappWS_ws1.TipoEccezione.AssenzaIngiustificata && (item.timbrature == null || item.timbrature.Count() == 0))
                    {
                        model.TotaleEvidenzeDaGiustificareSoloAssIng++;
                        model.TotaleEvidenzeDaGiustificare++;
                    }

                    if (item.TipoEcc == digigappWS_ws1.TipoEccezione.Carenza )
                    {
                        model.TotaleEvidenzeDaGiustificare++;
                        if (Utente.GetQuadratura() == Quadratura.Giornaliera  && Utente.TipoDipendente()!="G")
                            model.TotaleEvidenzeDaGiustificareCarenze++;
                    }
                    if (Utente.TipoDipendente() != "G" && item.TipoEcc == digigappWS_ws1.TipoEccezione.Incongruenza)
                    {
                        model.TotaleEvidenzeDaGiustificareSoloAssIng++;
                        model.TotaleEvidenzeDaGiustificare++;
                    }
                    if (Utente.GetQuadratura() == Quadratura.Giornaliera &&
                        item.TipoEcc == digigappWS_ws1.TipoEccezione.MaggiorPresenza)
                    {
                        model.TotaleEvidenzeDaGiustificare++;
                    }
                }
            }


            return model;
        }

        /// <summary>
        /// Restituisce le date in cui l'utente ha delle evidenze
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static List<DateTime> GetGiornateInEvidenzaPerCalendario(DateTime data)
        {
            List<DateTime> result = new List<DateTime>();
			digigappWS_ws1.WSDigigapp datiBack_ws1 = new digigappWS_ws1.WSDigigapp();
            datiBack_ws1.Credentials = CommonHelper.GetUtenteServizioCredentials();
			string dateBack = Utente.GetDateBackPerEvidenze();

            var getListaEvidenze = SessionHelper.Get( SessionVariables.ListaEvidenzeScrivania );
            digigappWS_ws1.monthResponseEccezione listaEvidenze = new digigappWS_ws1.monthResponseEccezione( );


            if (getListaEvidenze != null)
            {
                var sessionData = (SessionListaEvidenzeModel)getListaEvidenze;
                listaEvidenze = sessionData.ListaEvidenze;
            }

            //// ## FRANCESCO LISTAEVIDENZE
            //var listaEvidenze = datiBack_ws1.ListaEvidenze( CommonManager.GetCurrentUserMatricola(), dateBack, data.ToString( "ddMMyyyy" ), 70 );

            if (listaEvidenze.data != null)
            {
				if ( myRai.Models.Utente.GetQuadratura() == Quadratura.Settimanale )
                {
                    using (digiGappEntities db = new digiGappEntities())
                    {
						string sede = Utente.SedeGapp(data);
                        var listagiornate = listaEvidenze.data.giornate.ToList();
                        //var listadate = db.DIGIRESP_Archivio_PDF.Where( y => y.sede_gapp == sede && y.tipologia_pdf == "P" ).ToList();
                        //if ( listadate.Count > 0 )
                        //{
                        //	DateTime dmax = listadate.OrderByDescending( x => x.data_fine ).Select( x => x.data_fine ).First();
                        //	listagiornate.RemoveAll( x => x.TipoEcc == digigappWS_ws1.TipoEccezione.Carenza && x.data <= dmax );
                        //}
                        var listadate = db.DIGIRESP_Archivio_PDF.Where(y => y.sede_gapp == sede && y.tipologia_pdf == "P").Select(x => x.data_fine);
                        if (listadate.Count() > 0)
                        {
                            DateTime dmax = listadate.Max();
                            listagiornate.RemoveAll(x => x.TipoEcc == digigappWS_ws1.TipoEccezione.Carenza && x.data <= dmax);
                        }

                        listaEvidenze.data.giornate = listagiornate.ToArray();
                    }

                    listaEvidenze.data.giornate = ClearCarenze(listaEvidenze.data.giornate);
                }

                listaEvidenze.data.giornate = SopprimiQuadrateDaDipendente(listaEvidenze.data.giornate);

                foreach (var item in listaEvidenze.data.giornate)
                {
                    if (item.data.Date.Month != data.Date.Month)
                    {
                        continue;
                    }

                    // se il dipendente non è giornalista, allora verranno conteggiati 
                    // anche i transiti sfasati
					if ( Utente.TipoDipendente() != "G" )
                    {
						if ( item.TipoEcc == digigappWS_ws1.TipoEccezione.AssenzaIngiustificata && item.timbrature != null && item.timbrature.Count() > 0 )
                        {
                            result.Add(item.data);
                        }
                    }

					if ( item.TipoEcc == digigappWS_ws1.TipoEccezione.AssenzaIngiustificata && ( item.timbrature == null || item.timbrature.Count() == 0 ) )
                    {
                        result.Add(item.data);
                    }

					if ( item.TipoEcc == digigappWS_ws1.TipoEccezione.Carenza )
                    {
                        result.Add(item.data);
                    }

					if ( Utente.GetQuadratura() == Quadratura.Giornaliera &&
						item.TipoEcc == digigappWS_ws1.TipoEccezione.MaggiorPresenza )
                    {
                        result.Add(item.data);
                    }
                }
            }

            var dateSWTIM= listaEvidenze.data.giornate.Where(x => x.TipoEcc == digigappWS_ws1.TipoEccezione.TimbratureInSW).Select(x => x.data).ToList();
            if (dateSWTIM.Any())
            {
                result.AddRange(dateSWTIM);
                result= result.Distinct().ToList();
            }
            return result;
        }

        public static RaipermeNewsModel GetRaipermeNewsModel(string categoria_news,bool segnaLette = true)
        {
            
            RaipermeNewsModel model = new RaipermeNewsModel();
            if (SessionHelper.Get("NewsRead") !=null && !categoria_news.Equals("Release notes")) return model;

            var db = new digiGappEntities();

            
            string matr=CommonManager.GetCurrentUserMatricola();
            string pMatr = CommonHelper.GetCurrentUserPMatricola();
            var range = db.MyRai_LogAzioni.Where(x => x.operazione == "SESSION_START" && x.matricola == matr)
                .Select(x => x.data)
                .OrderByDescending(x => x).Take(2).ToList();

            List<myRaiData.MyRai_News> newslist = new List<MyRai_News>();
            var query = db.MyRai_News.AsQueryable();

            query = query.Where(x => x.categoria == categoria_news);
            // TODO: Duplicato??
            try
            {
                //NC - Temporaneamente nel try/catch in attesa dell'aggiunta della colonna categoria in produzione
            if (range.Count() < 2)
                    newslist = query
                    .OrderByDescending(x => x.priorita)
                    .ThenBy(x => x.data_info)
                    .ToList();
            else
                    newslist = query
                    .OrderByDescending(x => x.priorita)
                    .ThenBy(x => x.data_info)
                    .ToList();
            //.Where(x => x.data_info >= range[1] && x.data_info <= range[0]).ToList();
            }
            catch (Exception ex)
            {

            }



            if (Utente.IsAbilitatoGapp() && newslist.Count() > 0)
            {
                foreach (var news in newslist)
                {
                    if (news.destinatari_any == true ||
                        (Utente.IsBoss() && news.destinatari_L1 == true) ||
                        (Utente.IsBossLiv2() && news.destinatari_L2 == true) ||
                        (news.destinatari_tipodip != null && news.destinatari_tipodip.ToUpper().Trim().Split(',').Contains(Utente.TipoDipendente())) ||
                        (news.destinatari_sedigapp != null && news.destinatari_sedigapp.ToUpper().Trim().Split(',').Contains(Utente.SedeGapp(DateTime.Now ))) ||
                        (!String.IsNullOrWhiteSpace(news.destinatari_matricole) && news.destinatari_matricole.Trim().Split(',').Contains(matr)))
                    {
                        if (news.validita_inizio == null && news.validita_fine == null)
                        {
                            if (!(news.data_info >= range[1] && news.data_info <= range[0]))
                                continue;
                        }
                        else
                        {
                            //Se la notizia non è più valida
                            if ((news.validita_inizio != null && news.validita_inizio >= DateTime.Now) || (news.validita_fine != null && news.validita_fine <= DateTime.Now))
                                continue;
                            if (news.MyRai_News_Lette != null && news.MyRai_News_Lette.Any(x => x.Matricola == matr) && !news.categoria.Equals("Release notes"))

                                continue;

                        }

                        if (!String.IsNullOrWhiteSpace(news.controllo_aggiuntivo))
                        {
                            switch (news.controllo_aggiuntivo)
                            {
                                case "check_detax":
                                    if (!Check_Detax()) continue;
                                    break;
                                default:
                                    break;
                            }
                        }

                        if(news.categoria.Equals("Release notes") && !news.MyRai_News_Lette.Any(x => x.Matricola == matr))
                        {
                            news.isNew = true;
                        }
                        model.NewsItems.Add(news);

                        if (news.validita_inizio != null || news.validita_fine != null)
                        {
                            if(segnaLette && (!news.categoria.Equals("Release notes") || news.isNew))
                                
                        {
                            db.MyRai_News_Lette.Add(new MyRai_News_Lette()
                            {
                                Id_News = news.id,
                                Matricola = matr
                            });
                                DBHelper.Save(db, CommonManager.GetCurrentUserMatricola());
                            }
                        }

                    }
                }

                if (model.NewsItems.Count() > 0 && !categoria_news.Equals("Release notes")) 
                {

                    SessionHelper.Set("NewsRead", true);


                }




            }
            return model;
        }

        public static string GetEvidenzaPerGiornata(DateTime data, bool readFromSession = false)
        {
            digigappWS_ws1.WSDigigapp datiBack_ws1 = new digigappWS_ws1.WSDigigapp();
            datiBack_ws1.Credentials = new System.Net.NetworkCredential(
                            CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                            CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]
                            );
            string dateBack = Utente.GetDateBackPerEvidenze();
            var listaEvidenze = new digigappWS_ws1.monthResponseEccezione();

            if (readFromSession)
            {
                var getListaEvidenze = SessionHelper.Get(SessionVariables.ListaEvidenzeScrivania);

                if (getListaEvidenze != null)
                {
                    var sessionData = (SessionListaEvidenzeModel)getListaEvidenze;
                    listaEvidenze = sessionData.ListaEvidenze;
                }
                else
                {
                    listaEvidenze = new digigappWS_ws1.monthResponseEccezione();
                }
            }
            else
            {
                listaEvidenze = ServiceWrapper.ListaEvidenzWrapper(datiBack_ws1, CommonManager.GetCurrentUserMatricola(),
                   dateBack,
                   DateTime.Now.AddDays(-1).ToString("ddMMyyyy"), 70);
                //model.listaEvidenze = datiBack_ws1.ListaEvidenze( CommonManager.GetCurrentUserMatricola( ) ,
                //   dateBack ,
                //   DateTime.Now.AddDays( -1 ).ToString( "ddMMyyyy" ) , 70 );
            }

            if (listaEvidenze.data != null)
            {
                if (myRai.Models.Utente.GetQuadratura() == Quadratura.Settimanale)
                {
                    using (digiGappEntities db = new digiGappEntities())
                    {
                        string sede = Utente.SedeGapp(DateTime.Now);
                        var listagiornate = listaEvidenze.data.giornate.ToList();
                        //var listadate = db.DIGIRESP_Archivio_PDF.Where(y => y.sede_gapp == sede && y.tipologia_pdf == "P").ToList();
                        //if (listadate.Count > 0)
                        //{
                        //    DateTime dmax = listadate.OrderByDescending(x => x.data_fine).Select(x => x.data_fine).First();
                        //    listagiornate.RemoveAll(x => x.TipoEcc == digigappWS_ws1.TipoEccezione.Carenza && x.data <= dmax);
                        //}
                        var listadate = db.DIGIRESP_Archivio_PDF.Where(y => y.sede_gapp == sede && y.tipologia_pdf == "P").Select(x => x.data_fine);
                        if (listadate.Count() > 0)
                        {
                            DateTime dmax = listadate.Max();
                            listagiornate.RemoveAll(x => x.TipoEcc == digigappWS_ws1.TipoEccezione.Carenza && x.data <= dmax);
                        }
                        // listagiornate.RemoveAll(x =>x.TipoEcc==digigappWS_ws1.TipoEccezione.Carenza && x.data <= (db.DIGIRESP_Archivio_PDF.Where(y => y.sede_gapp == sede && y.tipologia_pdf == "P").Max(z => z.data_fine)));
                        listaEvidenze.data.giornate = listagiornate.ToArray();
                    }

                    listaEvidenze.data.giornate = ClearCarenze(listaEvidenze.data.giornate);
                }

                listaEvidenze.data.giornate = SopprimiQuadrateDaDipendente(listaEvidenze.data.giornate);
            }
            var ev = listaEvidenze.data.giornate.Where(x => x.data == data).FirstOrDefault();
            if (ev != null)
            {
                if (ev.TipoEcc == digigappWS_ws1.TipoEccezione.Carenza) return "C";
                if (ev.TipoEcc == digigappWS_ws1.TipoEccezione.Incongruenza) return "I";
                if (ev.TipoEcc == digigappWS_ws1.TipoEccezione.MaggiorPresenza) return "M";
                if (ev.TipoEcc == digigappWS_ws1.TipoEccezione.AssenzaIngiustificata)
                {
                    if (ev.timbrature == null || !ev.timbrature.Any())
                        return "A";
                    else
                        return "T";
                }
            }

            return null;
        }
        private static bool Check_Detax()
        {
            bool canSeeNews = false;

            MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client servizioWCF = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
            var result = servizioWCF.GetModuloDetassazione(CommonManager.GetCurrentUserPMatricola(), CommonManager.GetCurrentUserMatricola());

            canSeeNews = result.Esito && String.IsNullOrEmpty(result.Errore) && !result.Response.Equals("ND");

            return canSeeNews;
        }
    }
}