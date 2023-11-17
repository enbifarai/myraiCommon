using myRai.Business;
using myRaiData;
using myRaiHelper;
using myRaiServiceHub.it.rai.servizi.anagraficaws1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel.Services
{
    public class SpeseDiProduzioneServizio
    {

        private string matricola = "0" + CommonManager.GetCurrentUserMatricola();

        public SpeseDiProduzioneServizio(SpeseDiProduzioneEntities dbContext)
        {

        }


        public List<SpeseProduzioneViewModel> GetSpeseProduzione(bool isAperte, DateTime? dataal, DateTime? datadal, string stato, string idFSP, string matr="" )
        {
            decimal saldo = 0;
            /*
            string queryBase =
                        " SELECT  " +
                        " spese.ID as Id_FoglioSpese, " +
                        " imp.MP_Data as MP_Data, " +
                        " spese.Stato as Stato, " +
                        " ant.Importo_Totale as MP_Importo, " +
                        " imp.MA_Stato as MA_Stato, " +
                        " spese.WBS as WBS, " +
                        " RTRIM(imp.Tipo) as Tipo, " +
                        " isnull(rendicontoVoci.Valore_in_Euro,0) as MA_Importo_In_Euro, " +
                        " targ.Tipo as TipoTarghetta, " +
                        " convert(varchar,localita.Localita) as Località, " +
                        " anticipoVoci.Importo as Totale, " +
                        " ant.Importo_Totale as ImportoTotaleAnticipo, " +
                        " (isnull(rendicontoVoci.Valore_in_Euro,0)-ant.Importo_Totale) as Saldo, " +
                        "rend.Stato as StatoRendiconto" +
                        " FROM TImporti_SAP as imp " +
                        " join TFoglio_Spese as spese on imp.ID = spese.ID " +
                        " join TAnticipo as ant on spese.ID = ant.ID " +
                        " left outer join TRendiconto_Voci as rendicontoVoci on ant.ID = rendicontoVoci.ID " +
                        " join TAnticipo_Voci as anticipoVoci on spese.ID = anticipoVoci.ID " +
                        " join TRendiconto_Targhetta as targ on spese.ID = targ.ID " +
                        " join TFSP01 as tf on targ.Tipo = tf.Codice " +
                        " join TLocalita as localita on spese.ID = localita.ID " +
                        " left outer join TRendiconto as rend on ant.ID = rend.ID " +
           " where spese.Matricola='" + matricola + "' and spese.Data_Creazione >= '2020-01-01' " +
        */
            string queryBase =
                          " SELECT  " +
                          " spese.ID as Id_FoglioSpese, " +
                          " isnull(imp.MP_Data,'') as MP_Data, " +
                          " spese.Stato as Stato, " +
                          " ant.Importo_Totale as MP_Importo, " +
                          " isnull(imp.MA_Stato,'') as MA_Stato, " +
                          " spese.WBS as WBS, " +
                          " RTRIM(isnull(imp.Tipo,0)) as Tipo, " +
                          " isnull(rendicontoVoci.Valore_in_Euro,0) as MA_Importo_In_Euro, " +
                          " targ.Tipo as TipoTarghetta, " +
                          " convert(varchar,localita.Localita) as Località, " +
                          " isnull(anticipoVoci.Importo,0) as Totale, " +
                          " ant.Importo_Totale as ImportoTotaleAnticipo, " +
                          " (isnull(rendicontoVoci.Valore_in_Euro,0)-ant.Importo_Totale) as Saldo, " +
                          "rend.Stato as StatoRendiconto" +
                          " FROM TFoglio_Spese as spese" +
                         "  join TImporti_SAP as imp on imp.ID = spese.ID " +
                          " join TAnticipo as ant on spese.ID = ant.ID " +
                          " left outer join TRendiconto_Voci as rendicontoVoci on ant.ID = rendicontoVoci.ID " +
                          " left outer join TAnticipo_Voci as anticipoVoci on spese.ID = anticipoVoci.ID " +
                          " join TRendiconto_Targhetta as targ on spese.ID = targ.ID " +
                          " join TFSP01 as tf on targ.Tipo = tf.Codice " +
                          " join TLocalita as localita on spese.ID = localita.ID " +
                          " left outer join TRendiconto as rend on ant.ID = rend.ID " +
             " where spese.Matricola='" + (matr==""?"0"+matricola:"0"+matr) + "' and spese.Data_Creazione >= '2020-01-01' " +

                          // " and targ.Tipo <> 'DI' " +
                          " and  tf.NomeTab = '1013' ";


            var dataProssima = DateTime.Parse("2020/01/01");
            var dataFine = new DateTime(9999, 12, 31);
            List<SpeseProduzioneViewModel> listaSpese = new List<SpeseProduzioneViewModel>();
            var db = new SpeseDiProduzioneEntities();
            if ((string.IsNullOrEmpty(stato) && string.IsNullOrEmpty(idFSP) && dataal.HasValue == false && datadal.HasValue == false))
            {
                if (isAperte)
                {
                    string querySql = queryBase +
                        " and ant.Stato not like 'M%' " +
                      " and rend.Stato < 'Q8' and rend.Stato not like '%0' ";

                    var speseAperte = db.Database.SqlQuery<SpeseProduzioneViewModel>(querySql).ToList();
                    if (speseAperte != null && speseAperte.Any())
                    {
                        listaSpese = speseAperte.GroupBy(x => x.Id_FoglioSpese).OrderBy(x => x.Key).ToList().Select(groupImpID =>
                          new SpeseProduzioneViewModel()
                          {
                              Id_FoglioSpese = groupImpID.FirstOrDefault().Id_FoglioSpese,
                              MP_Data = groupImpID.FirstOrDefault().MP_Data,
                              Stato = groupImpID.LastOrDefault().Stato,
                              MP_Importo = groupImpID.FirstOrDefault().MP_Importo,
                              MA_Stato = groupImpID.FirstOrDefault().MA_Stato,
                              WBS = groupImpID.FirstOrDefault().WBS,
                              Tipo = groupImpID.LastOrDefault().Tipo.Trim(),
                              isAperta = true,
                              MA_Importo_In_Euro = groupImpID.FirstOrDefault().MA_Importo_In_Euro,
                              TipoTarghetta = groupImpID.FirstOrDefault().TipoTarghetta,
                              Località = groupImpID.FirstOrDefault().Località,
                              Totale = groupImpID.FirstOrDefault().Totale,
                              ImportoTotaleAnticipo = groupImpID.FirstOrDefault().ImportoTotaleAnticipo,
                              SenzaAnticipo = false,
                              Saldo = groupImpID.FirstOrDefault().Saldo,
                              StatoRendiconto = groupImpID.FirstOrDefault().StatoRendiconto
                          }).ToList();
                    }
                }
                else
                {
                    string querySql = queryBase +
                       " and (ant.Stato like 'M%' and rend.Stato >= 'Q8' or rend.Stato like '%0')";
                    var speseChiuse = db.Database.SqlQuery<SpeseProduzioneViewModel>(querySql).ToList();

                    if (speseChiuse != null && speseChiuse.Any())
                    {
                        listaSpese = speseChiuse.GroupBy(x => x.Id_FoglioSpese).OrderBy(x => x.Key).ToList().Select(groupImpID =>
                          new SpeseProduzioneViewModel()
                          {
                              Id_FoglioSpese = groupImpID.FirstOrDefault().Id_FoglioSpese,
                              MP_Data = groupImpID.FirstOrDefault().MP_Data,
                              Stato = groupImpID.LastOrDefault().Stato,
                              MP_Importo = groupImpID.FirstOrDefault().MP_Importo,
                              MA_Stato = groupImpID.FirstOrDefault().MA_Stato,
                              WBS = groupImpID.FirstOrDefault().WBS,
                              Tipo = groupImpID.LastOrDefault().Tipo.Trim(),
                              isAperta = true,
                              MA_Importo_In_Euro = groupImpID.FirstOrDefault().MA_Importo_In_Euro,
                              TipoTarghetta = groupImpID.FirstOrDefault().TipoTarghetta,
                              Località = groupImpID.FirstOrDefault().Località,
                              Totale = groupImpID.FirstOrDefault().Totale,
                              ImportoTotaleAnticipo = groupImpID.FirstOrDefault().ImportoTotaleAnticipo,
                              SenzaAnticipo = false,
                              Saldo = groupImpID.FirstOrDefault().Saldo,
                              StatoRendiconto = groupImpID.FirstOrDefault().StatoRendiconto

                          }).ToList();
                    }

                }

            }
            else
            {
                decimal idFogliospese = 0;
                string datada = datadal.Value.ToString("yyyy-MM-dd HH:mm:ss.fff");

                string dataa = dataal.Value.ToString("yyyy-MM-dd HH:mm:ss.fff");

                string querysql = queryBase + " and (spese.data_creazione between '" + datada + "' and '" + dataa + "')";
                /*  var searchFSP = from importi in db.TImporti_SAP
                                  join fogliospese in db.TFoglio_Spese on importi.ID equals fogliospese.ID
                                  join localita in db.TLocalita on fogliospese.ID equals localita.ID
                                  join anticipo in db.TAnticipo on fogliospese.ID equals anticipo.ID
                                  join targhetta in db.TRendiconto_Targhetta on fogliospese.ID equals targhetta.ID
                                  join tfsp01 in db.TFSP01 on targhetta.Tipo equals tfsp01.Codice
                                  where (anticipo.Periodo_Dal >= datadal.Value && anticipo.Periodo_Al <= dataal.Value && fogliospese.Matricola == matricola)
                                  select new { importi, fogliospese, targhetta, anticipo, localita, tfsp01 } into t1
                                  group t1 by t1.importi.ID;
                                  */
                if (idFSP != "")
                {
                    idFogliospese = Convert.ToDecimal(idFSP);
                    querysql = querysql + " and spese.id=" + idFogliospese;
                    /*
                    searchFSP = from importi in db.TImporti_SAP
                                join fogliospese in db.TFoglio_Spese on importi.ID equals fogliospese.ID
                                join localita in db.TLocalita on fogliospese.ID equals localita.ID
                                join anticipo in db.TAnticipo on fogliospese.ID equals anticipo.ID
                                join targhetta in db.TRendiconto_Targhetta on fogliospese.ID equals targhetta.ID
                                join tfsp01 in db.TFSP01 on targhetta.Tipo equals tfsp01.Codice
                                where (anticipo.Periodo_Dal >= datadal.Value && anticipo.Periodo_Al <= dataal.Value && fogliospese.Matricola == matricola)
                                || fogliospese.ID == idFogliospese
                                select new { importi, fogliospese, targhetta, anticipo, localita, tfsp01 } into t1
                                group t1 by t1.importi.ID; */
                }

                if (querysql != null)
                {
                    var filtrate = db.Database.SqlQuery<SpeseProduzioneViewModel>(querysql).ToList();
                    /* listaSpese = searchFSP.OrderBy(x => x.Key).Select(groupImpID =>
                       new SpeseProduzioneViewModel()
                       {
                           Tipo = groupImpID.FirstOrDefault().importi.Tipo.Trim(),
                           MA_Stato = groupImpID.FirstOrDefault().importi.MA_Stato,
                           Id_FoglioSpese = groupImpID.FirstOrDefault().anticipo.ID,
                           Id_TImportiSap = groupImpID.FirstOrDefault().anticipo.ID,
                           MA_Importo_In_Euro = groupImpID.Sum(x => x.importi.MA_Importo_In_Euro),
                           MP_Importo = groupImpID.Sum(x => x.importi.MP_Importo),
                           Sezione = groupImpID.FirstOrDefault().fogliospese.Sezione,
                           Stato = groupImpID.FirstOrDefault().fogliospese.Stato,
                           Località = groupImpID.FirstOrDefault().localita.Localita,
                           TipoTarghetta = groupImpID.FirstOrDefault().targhetta.Tipo,
                           MP_Data = groupImpID.FirstOrDefault().importi.MP_Data,
                           WBS = groupImpID.FirstOrDefault().fogliospese.WBS,
                           Saldo = saldo + groupImpID.Sum(x => x.importi.MP_Importo),
                           SenzaAnticipo = false,
                       }).ToList();*/
                    listaSpese = filtrate.GroupBy(x => x.Id_FoglioSpese).OrderBy(x => x.Key).ToList().Select(groupImpID =>
                        new SpeseProduzioneViewModel()
                        {
                            Id_FoglioSpese = groupImpID.FirstOrDefault().Id_FoglioSpese,
                            MP_Data = groupImpID.FirstOrDefault().MP_Data,
                            Stato = groupImpID.LastOrDefault().Stato,
                            MP_Importo = groupImpID.FirstOrDefault().MP_Importo,
                            MA_Stato = groupImpID.FirstOrDefault().MA_Stato,
                            WBS = groupImpID.FirstOrDefault().WBS,
                            Tipo = groupImpID.LastOrDefault().Tipo.Trim(),
                            isAperta = true,
                            MA_Importo_In_Euro = groupImpID.FirstOrDefault().MA_Importo_In_Euro,
                            TipoTarghetta = groupImpID.FirstOrDefault().TipoTarghetta,
                            Località = groupImpID.FirstOrDefault().Località,
                            Totale = groupImpID.FirstOrDefault().Totale,
                            ImportoTotaleAnticipo = groupImpID.FirstOrDefault().ImportoTotaleAnticipo,
                            SenzaAnticipo = false,
                            Saldo = groupImpID.FirstOrDefault().Saldo,
                            StatoRendiconto = groupImpID.FirstOrDefault().StatoRendiconto

                        }).ToList();
                    if (stato != "") listaSpese = listaSpese.Where(x => x.MA_Stato == stato).ToList();

                }


            }

            var tmpImportiAnticipo = (from anticipo in db.TAnticipo
                                      join voci in db.TAnticipo_Voci on anticipo.ID equals voci.ID
                                      join fsp in db.TFoglio_Spese on anticipo.ID equals fsp.ID
                                      where fsp.Matricola == matricola
                                      select new SpeseProduzioneViewModel()
                                      {
                                          MA_Importo_In_Euro = voci.Importo,
                                      }).Distinct().ToList();
            var tmpImportiRendiconto = (from fogliospese in db.TFoglio_Spese
                                        join rend in db.TRendiconto on fogliospese.ID equals rend.ID
                                        join voci in db.TRendiconto_Voci on rend.ID equals voci.ID
                                        where fogliospese.Matricola == matricola
                                        select new SpeseProduzioneViewModel()
                                        {
                                            Id_FoglioSpese = fogliospese.ID,
                                            MA_Importo_In_Euro = voci.Valore_in_Euro,
                                            Id_Rendiconto = voci.ProgressivoVoce
                                        }).Distinct().ToList();



            foreach (var item in listaSpese)
            {

                item.MA_Importo_In_Euro = tmpImportiRendiconto.Where(x => x.Id_FoglioSpese == item.Id_FoglioSpese).Sum(s => s.MA_Importo_In_Euro);
                item.Saldo = item.MA_Importo_In_Euro - item.MP_Importo;

            }


            if (listaSpese != null && listaSpese.Any())
            {
                foreach (var w in listaSpese)
                {
                    List<string> splittata = new List<string>();
                    if (!String.IsNullOrEmpty(w.WBS))
                    {
                        try
                        {
                            splittata = w.WBS.Split('.').ToList();
                            string matricolaInin = splittata[0];

                            string matricolaSpettacolo = splittata[1];

                            var dati = GetProduzioneByMatricola(matricolaSpettacolo, matricolaInin);

                            if (!String.IsNullOrEmpty(dati))
                            {
                                w.Titolo = dati;

                            }
                            else
                            {
                                w.Titolo = "NESSUNA DESCRIZIONE TROVATA...";
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                    if (!(w.Stato.Equals("AA") && w.ImportoTotaleAnticipo == 0))
                        w.SenzaAnticipo = true;
                    else
                        w.SenzaAnticipo = false;

                }
            }


            return listaSpese;
        }





        public List<SpeseProduzioneViewModel> GetDesctizioniAndImportiAnticipi(decimal id)
        {
            SpeseDiProduzioneEntities db = new SpeseDiProduzioneEntities();
            SpeseProduzioneViewModel result = new SpeseProduzioneViewModel();
            result.ListaDescrizioniAndImportiAnticipi = (from anticipo in db.TAnticipo
                                                         join voci in db.TAnticipo_Voci on anticipo.ID equals voci.ID
                                                         join tfsp02 in db.TFSP02 on voci.Voce equals tfsp02.Codice
                                                         where anticipo.ID == id && tfsp02.NomeTab.Equals("2004")
                                                         select new SpeseProduzioneViewModel()
                                                         {
                                                             MA_Importo_In_Euro = voci.Importo,
                                                             Descrizione = tfsp02.Descrizione.ToUpper()
                                                         }).Distinct().ToList();
            for (int i = 0; i < result.ListaDescrizioniAndImportiAnticipi.Count(); i++)
            {
                result.ListaDescrizioniAndImportiAnticipi[i].Descrizione = FirstCharToUpper(result.ListaDescrizioniAndImportiAnticipi[i].Descrizione);
            }

            return result.ListaDescrizioniAndImportiAnticipi;
        }
        private string GetProduzioneByMatricola(string term, string desc)
        {
            Dictionary<string, string> prodDes = null;
            if (SessionManager.Get("_SpeseProd_Desc") != null)
                prodDes = (Dictionary<string, string>)SessionManager.Get("_SpeseProd_Desc");
            else
            {
                prodDes = new Dictionary<string, string>();
                SessionManager.Set("_SpeseProd_Desc", prodDes);
            }

            string key = term + "." + desc;
            string titolo = "";
            if (!prodDes.TryGetValue(key, out titolo))
            {
                var db = new myRaiData.Incentivi.IncentiviEntities("IncentiviEntities_Cezanne");
                string sqlQuery = " SELECT TOP 1 [titolo_matricola_produzione] FROM[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_MATRICOLA_PRODUZIONE] " +
                " where cod_matricola_produzione = '" + term + "' and uorg_editoriale = '" + desc + "'";

                var listT = db.Database.SqlQuery<string>(sqlQuery);
                if (listT != null && listT.Any())
                    titolo = listT.First();
                else
                {

                    try
                    {
                        APWS sr = new APWS();
                        sr.Credentials = new System.Net.NetworkCredential(@"srvanapro", "bc14a3", "RAI");
                        ObjTVRicercaAnagrafieResult res = new ObjTVRicercaAnagrafieResult();

                        ObjInputRicercaMatricola ricercaMatricola = new ObjInputRicercaMatricola();

                        ricercaMatricola.Matricola = term;
                        ricercaMatricola.Uorg = desc;
                        ricercaMatricola.StatiInVita = true;

                        res = sr.TvRicercaAnagrafiaMatricola(ricercaMatricola);

                        var results = res.RisultatoTVRicercaAnagrafie.FirstOrDefault();
                        if (results != null)
                        {
                            titolo = results.TITOLO;
                        }
                        else
                        {
                            titolo = String.Empty;
                        }
                    }
                    catch (Exception ex)
                    {
                        //throw new Exception(ex.Message);
                        titolo = String.Empty;
                    }
                }

                prodDes.Add(key, titolo);
            }

            return titolo;
        }

        //TODO vedi progressivo
        public IEnumerable<SpeseProduzioneViewModel> GetDettaglioFoglioSpese(decimal id, bool isAperte)
        {
            SpeseDiProduzioneEntities db = new SpeseDiProduzioneEntities();
            //var tmp = from s in db.TImporti_SAP
            //          join tfsp01Tipo in db.TFSP01 on s.Tipo equals tfsp01Tipo.Codice
            //          join c in db.TFoglio_Spese on s.ID equals c.ID
            //          join d in db.TAnticipo on c.ID equals d.ID
            //          join rendiconto in db.TRendiconto on c.ID equals rendiconto.ID into rend
            //          from rendiconto1 in rend.DefaultIfEmpty()
            //          join targhetta in db.TRendiconto_Targhetta on c.ID equals targhetta.ID
            //          join voci in db.TRendiconto_Voci on c.ID equals voci.ID
            //          join tfsp01 in db.TFSP01 on targhetta.Tipo equals tfsp01.Codice
            //          join proc_Anticipo in db.TPROCURATORI on d.Procuratore equals proc_Anticipo.CID
            //          join proc_Rend in db.TPROCURATORI on rendiconto1.Procuratore equals proc_Rend.CID into procuratore
            //          from procur_Rendiconto in procuratore.DefaultIfEmpty()
            //          join cod in db.TFSP01 on s.Tipo equals cod.Codice
            //          join localita in db.TLocalita on c.ID equals localita.ID
            //          where c.Matricola == matricola && c.ID == id && tfsp01.NomeTab.Equals("1013") && !(targhetta.Tipo.Equals("DI"))
            //          orderby targhetta.Data_Prima_Modifica
            //          select new { s, c, d, cod, proc_Anticipo, localita, targhetta, tfsp01, tfsp01Tipo, rendiconto1, procur_Rendiconto, voci } into groupbyID
            //          group groupbyID by groupbyID.s.ID;

            /*     string querySql =
                           " SELECT  " +
                           " imp.MP_Data, " +
                           " CONVERT(varchar, rend.Data_Consegna_Cartaceo, 103) as DataConsegna, " +
                           "IsNull(procAnticipo.NOME + ' ' + procAnticipo.COGNOME, '') as ProcuratoreAnticipo," +
                           "IsNull(procRendiconto.NOME + ' ' + procRendiconto.COGNOME, '') as ProcuratoreRendiconto," +
                           " tfspTipo.descrizione as Tipo," +
                           " imp.MA_Stato as MA_Stato, " +
                           " convert(varchar,localita.Localita) as Località, " +
                           " spese.ID as Id_FoglioSpese, " +
                           " imp.MA_Importo_In_Euro as MA_Importo_In_Euro, " +
                           " imp.MP_Importo as MP_Importo, " +
                           " spese.Stato as Stato, " +
                           " ant.Periodo_Dal as Periodo_Dal, " +
                           " ant.Periodo_Al as Periodo_Al, " +
                           " ant.Importo_Totale as ImportoTotaleAnticipo, " +
                           " targ.Tipo as TipoTarghetta, " +
                           " spese.WBS as WBS, " +
                           " rend.stato as StatoRendiconto" +
                           " FROM TImporti_SAP as imp " +
                           " join TFSP01 as tfspTipo on imp.Tipo = tfspTipo.Codice" +
                           " join TFoglio_Spese as spese on imp.ID = spese.ID " +
                           " join TAnticipo as ant on spese.ID = ant.ID " +
                           " left outer join TRendiconto as rend on spese.ID = rend.ID " +
                           " join TRendiconto_Targhetta as targ on spese.ID = targ.ID " +
                           " left outer join TRendiconto_Voci as rendicontoVoci on spese.ID = rendicontoVoci.ID " +
                           " join TFSP01 as tf on targ.Tipo = tf.Codice " +
                           " left outer join TPROCURATORI as procAnticipo on ant.Procuratore = procAnticipo.CID" +
                           " left outer join TPROCURATORI as procRendiconto on rend.Procuratore = procRendiconto.CID" +
                           //" join TFS " +
                           " join TLocalita as localita on spese.ID = localita.ID " +
                           " where spese.ID ='" + id + "' " +
                    //        " and targ.Tipo <> 'DI' " +
                           " and  tf.NomeTab = '1013' " +
                           " order by targ.Data_Prima_Modifica ";
                           */
            string querySql =
           " SELECT  " +
           " isnull(imp.MP_Data,''), " +
           " CONVERT(varchar, rend.Data_Consegna_Cartaceo, 103) as DataConsegna, " +
           "IsNull(procAnticipo.NOME + ' ' + procAnticipo.COGNOME, '') as ProcuratoreAnticipo," +
           "IsNull(procRendiconto.NOME + ' ' + procRendiconto.COGNOME, '') as ProcuratoreRendiconto," +
           " tfspTipo.descrizione as Tipo," +
           " isnull(imp.MA_Stato,0) as MA_Stato, " +
           " convert(varchar,localita.Localita) as Località, " +
           " spese.ID as Id_FoglioSpese, " +
           " isnull(imp.MA_Importo_In_Euro,0) as MA_Importo_In_Euro, " +
           " isnull(imp.MP_Importo,0) as MP_Importo, " +
           " spese.Stato as Stato, " +
           " ant.Periodo_Dal as Periodo_Dal, " +
           " ant.Periodo_Al as Periodo_Al, " +
           " ant.Importo_Totale as ImportoTotaleAnticipo, " +
           " targ.Tipo as TipoTarghetta, " +
           " spese.WBS as WBS, " +
           " rend.stato as StatoRendiconto" +
           " FROM TFoglio_Spese as spese" +
           " join TImporti_SAP as imp on imp.ID = spese.ID " +
           " left outer join TFSP01 as tfspTipo on imp.Tipo = tfspTipo.Codice" +
           " join TAnticipo as ant on spese.ID = ant.ID " +
           " left outer join TRendiconto as rend on spese.ID = rend.ID " +
           " join TRendiconto_Targhetta as targ on spese.ID = targ.ID " +
           " left outer join TRendiconto_Voci as rendicontoVoci on spese.ID = rendicontoVoci.ID " +
           " join TFSP01 as tf on targ.Tipo = tf.Codice " +
           " left outer join TPROCURATORI as procAnticipo on ant.Procuratore = procAnticipo.CID" +
           " left outer join TPROCURATORI as procRendiconto on rend.Procuratore = procRendiconto.CID" +
           //" join TFS " +
           " join TLocalita as localita on spese.ID = localita.ID " +
           " where spese.ID ='" + id + "' " +
           //        " and targ.Tipo <> 'DI' " +
           " and  tf.NomeTab = '1013' " +
           " order by targ.Data_Prima_Modifica ";
            var dataProssima = DateTime.Today;

            IEnumerable<SpeseProduzioneViewModel> viewmodel = null;
            if (isAperte)
            {
                var speseAperte = db.Database.SqlQuery<SpeseProduzioneViewModel>(querySql).ToList();
                if (speseAperte != null && speseAperte.Any())
                {
                    viewmodel = speseAperte.GroupBy(x => x.Id_FoglioSpese).Select(groupImpID =>
                      new SpeseProduzioneViewModel()
                      {
                          DataConsegna = groupImpID.OrderByDescending(s => s.MP_Data).FirstOrDefault().DataConsegna,
                          Id_FoglioSpese = groupImpID.FirstOrDefault().Id_FoglioSpese,
                          ProcuratoreRendiconto = groupImpID.FirstOrDefault().ProcuratoreRendiconto,
                          ProcuratoreAnticipo = groupImpID.FirstOrDefault().ProcuratoreAnticipo,
                          Tipo = groupImpID.OrderByDescending(s => s.Tipo).FirstOrDefault().Descrizione,
                          Periodo_Al = groupImpID.FirstOrDefault().Periodo_Al,
                          Periodo_Dal = groupImpID.FirstOrDefault().Periodo_Dal,
                          MP_Data = groupImpID.FirstOrDefault().MP_Data,
                          Stato = groupImpID.LastOrDefault().Stato,
                          MP_Importo = groupImpID.Sum(x => x.MP_Importo),
                          MA_Stato = groupImpID.FirstOrDefault().MA_Stato,
                          WBS = groupImpID.FirstOrDefault().WBS,
                          isAperta = true,
                          MA_Importo_In_Euro = groupImpID.Sum(x => x.MA_Importo_In_Euro),
                          TipoTarghetta = groupImpID.LastOrDefault().TipoTarghetta,
                          Località = groupImpID.FirstOrDefault().Località,
                          Totale = groupImpID.FirstOrDefault().Totale,
                          ImportoTotaleAnticipo = groupImpID.FirstOrDefault().ImportoTotaleAnticipo,
                          SenzaAnticipo = false,
                          StatoRendiconto = groupImpID.FirstOrDefault().StatoRendiconto
                          //Saldo = groupImpID.FirstOrDefault().Saldo
                      });
                }



            }
            else
            {
                var speseChiuse = db.Database.SqlQuery<SpeseProduzioneViewModel>(querySql).ToList();


                if (speseChiuse != null && speseChiuse.Any())
                {
                    viewmodel = speseChiuse.GroupBy(x => x.Id_FoglioSpese).Select(groupImpID =>
                      new SpeseProduzioneViewModel()
                      {
                          DataConsegna = groupImpID.OrderByDescending(s => s.MP_Data).FirstOrDefault().DataConsegna,
                          Id_FoglioSpese = groupImpID.FirstOrDefault().Id_FoglioSpese,
                          ProcuratoreRendiconto = groupImpID.FirstOrDefault().ProcuratoreRendiconto,
                          ProcuratoreAnticipo = groupImpID.FirstOrDefault().ProcuratoreAnticipo,
                          Tipo = groupImpID.OrderByDescending(s => s.Tipo).FirstOrDefault().Descrizione,
                          Periodo_Al = groupImpID.FirstOrDefault().Periodo_Al,
                          Periodo_Dal = groupImpID.FirstOrDefault().Periodo_Dal,
                          MP_Data = groupImpID.FirstOrDefault().MP_Data,
                          Stato = groupImpID.LastOrDefault().Stato,
                          MP_Importo = groupImpID.Sum(x => x.MP_Importo),
                          MA_Stato = groupImpID.FirstOrDefault().MA_Stato,
                          WBS = groupImpID.FirstOrDefault().WBS,
                          isAperta = true,
                          MA_Importo_In_Euro = groupImpID.Sum(x => x.MA_Importo_In_Euro),
                          TipoTarghetta = groupImpID.LastOrDefault().TipoTarghetta,
                          Località = groupImpID.FirstOrDefault().Località,
                          Totale = groupImpID.FirstOrDefault().Totale,
                          ImportoTotaleAnticipo = groupImpID.FirstOrDefault().ImportoTotaleAnticipo,
                          SenzaAnticipo = false,
                          //Saldo = groupImpID.FirstOrDefault().Saldo
                      });
                }
            }


            if (viewmodel != null)
            {
                foreach (var w in viewmodel)
                {

                    List<string> splittata = new List<string>();
                    if (!String.IsNullOrEmpty(w.WBS))
                    {

                        try
                        {
                            splittata = w.WBS.Split('.').ToList();
                            string matricolaInin = splittata[0];

                            string matricolaSpettacolo = splittata[1];

                            var dati = GetProduzioneByMatricola(matricolaSpettacolo, matricolaInin);

                            if (!String.IsNullOrEmpty(dati))
                            {
                                w.Titolo = dati;
                            }
                            else
                            {
                                w.Titolo = "...NESSUNA DESCRIZIONE TROVATA...";
                            }

                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                    if (!(w.Stato.Equals("AA") && w.ImportoTotaleAnticipo == 0))
                        w.SenzaAnticipo = true;
                    else
                        w.SenzaAnticipo = false;
                }
            };

            return viewmodel;
        }
        public static string FirstCharToUpper(string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
                default: return input.First().ToString().ToUpper() + input.Substring(1).ToLower();
            }
        }


        #region DESCRIZIONI VOCI SPESE TARGHETTE 'DI','SE','PE', 'CO' CON IMPORTI 
        public List<SpeseProduzioneViewModel> GetDescrizioniAndImportiRendicontiConTarghettaInsegreteria(decimal id)
        {
            SpeseDiProduzioneEntities db = new SpeseDiProduzioneEntities();
            SpeseProduzioneViewModel result = new SpeseProduzioneViewModel();
            var tmp = (from fogliospese in db.TFoglio_Spese
                       join rend in db.TRendiconto on fogliospese.ID equals rend.ID
                       join voci in db.TRendiconto_Voci on rend.ID equals voci.ID
                       join targhetta in db.TRendiconto_Targhetta on rend.ID equals targhetta.ID
                       join tfp01 in db.TFSP01 on targhetta.Tipo equals tfp01.Codice
                       join tfsp02 in db.TFSP02 on voci.Voce equals tfsp02.Codice
                       where fogliospese.ID == id && tfp01.NomeTab.Equals("1013") && targhetta.Tipo.Equals("SE")
                       select new { fogliospese, voci, rend, targhetta, tfp01, tfsp02 } into groupbyID
                       group groupbyID by groupbyID.fogliospese.ID);

            if (tmp != null)
            {
                result.ListaDescrizioniAndImportiRendicontiInSegreteria = tmp.Select(s => new SpeseProduzioneViewModel()
                {
                    Id_FoglioSpese = s.FirstOrDefault().fogliospese.ID,
                    TipoTarghetta = s.FirstOrDefault().tfp01.Descrizione,
                    Saldo = (s.FirstOrDefault().targhetta.Saldo),
                }).ToList();


            }
            else
            {
                result.ListaDescrizioniAndImportiRendicontiInSegreteria = new List<SpeseProduzioneViewModel>();
            }
            return result.ListaDescrizioniAndImportiRendicontiInSegreteria;
        }
        public List<SpeseProduzioneViewModel> GetDescrizioneFromTFSP02ConTipoTaghettaSegreteria(decimal id)
        {
            var dataProssima = DateTime.Today;
            SpeseDiProduzioneEntities db = new SpeseDiProduzioneEntities();
            var descrizioneVociFromTFSP02 = from fogliospese in db.TFoglio_Spese
                                            join voci in db.TRendiconto_Voci on fogliospese.ID equals voci.ID
                                            join tfsp02 in db.TFSP02 on voci.Voce equals tfsp02.Codice
                                            join targhetta in db.TRendiconto_Targhetta on fogliospese.ID equals targhetta.ID
                                            where fogliospese.ID == id && targhetta.Tipo.Equals("SE")
                                            select new { voci, tfsp02, targhetta } into descrizioniAndImporti
                                            group descrizioniAndImporti by descrizioniAndImporti.voci.ProgressivoVoce;




            var listaDescrizioniAndImporti = descrizioneVociFromTFSP02.Select(s => new SpeseProduzioneViewModel()
            {
                Descrizione = s.FirstOrDefault().tfsp02.Descrizione,
                MA_Importo_In_Euro = s.FirstOrDefault().voci.Valore_in_Euro,
                Id_FoglioSpese = s.FirstOrDefault().voci.ID,
                progressivoVoce = s.FirstOrDefault().voci.ProgressivoVoce,
                valutaVoce = s.FirstOrDefault().voci.Valuta
            }).OrderByDescending(s => s.MA_Importo_In_Euro).ToList();
            for (int i = 0; i < listaDescrizioniAndImporti.Count(); i++)
            {
                listaDescrizioniAndImporti[i].Descrizione = FirstCharToUpper(listaDescrizioniAndImporti[i].Descrizione);
                var files = myRaiCommonTasks.Helpers.FileManager.GetFileByChiave(listaDescrizioniAndImporti[i].Id_FoglioSpese.ToString() + "_" + listaDescrizioniAndImporti[i].progressivoVoce.ToString()).Files;
                if (files.Count() > 0)
                {
                    listaDescrizioniAndImporti[i].idFile = files[0].Id;
                }
            }
            return listaDescrizioniAndImporti;
        }
        public List<SpeseProduzioneViewModel> GetDescrizioniAndImportiRendicontiConTarghettaInContabilita(decimal id)
        {
            SpeseDiProduzioneEntities db = new SpeseDiProduzioneEntities();
            SpeseProduzioneViewModel result = new SpeseProduzioneViewModel();
            var tmp = (from fogliospese in db.TFoglio_Spese
                       join rend in db.TRendiconto on fogliospese.ID equals rend.ID
                       join voci in db.TRendiconto_Voci on rend.ID equals voci.ID
                       join targhetta in db.TRendiconto_Targhetta on rend.ID equals targhetta.ID
                       join tfp01 in db.TFSP01 on targhetta.Tipo equals tfp01.Codice
                       join tfsp02 in db.TFSP02 on voci.Voce equals tfsp02.Codice
                       where fogliospese.ID == id && tfp01.NomeTab.Equals("1013") && targhetta.Tipo.Equals("CO")
                       select new { fogliospese, voci, rend, targhetta, tfp01, tfsp02 } into groupbyID
                       group groupbyID by groupbyID.fogliospese.ID);
            if (tmp != null)
            {
                result.ListaDescrizioniAndImportiRendicontiContabilita = tmp.Select(s => new SpeseProduzioneViewModel()
                {
                    Id_FoglioSpese = s.FirstOrDefault().fogliospese.ID,
                    TipoTarghetta = s.FirstOrDefault().tfp01.Descrizione,
                    Saldo = (s.FirstOrDefault().targhetta.Saldo),


                }).ToList();


            }
            else
            {
                result.ListaDescrizioniAndImportiRendicontiContabilita = new List<SpeseProduzioneViewModel>();
            }
            return result.ListaDescrizioniAndImportiRendicontiContabilita;
        }
        public List<SpeseProduzioneViewModel> GetDescrizioneFromTFSP02ConTipoTaghettaContabilita(decimal id)
        {
            var dataProssima = DateTime.Today;
            SpeseDiProduzioneEntities db = new SpeseDiProduzioneEntities();
            var descrizioneVociFromTFSP02 = (from fogliospese in db.TFoglio_Spese
                                             join voci in db.TRendiconto_Voci on fogliospese.ID equals voci.ID
                                             join tfsp02 in db.TFSP02 on voci.Voce equals tfsp02.Codice
                                             join targhetta in db.TRendiconto_Targhetta on fogliospese.ID equals targhetta.ID
                                             where fogliospese.ID == id && targhetta.Tipo.Equals("CO")
                                             select new SpeseProduzioneViewModel()
                                             {
                                                 Descrizione = tfsp02.Descrizione,
                                                 MA_Importo_In_Euro = voci.Valore_in_Euro,
                                                 progressivoVoce = voci.ProgressivoVoce,
                                                 Id_FoglioSpese = voci.ID

                                             }).Distinct().OrderByDescending(s => s.MA_Importo_In_Euro).ToList();
            for (int i = 0; i < descrizioneVociFromTFSP02.Count(); i++)
            {
                descrizioneVociFromTFSP02[i].Descrizione = FirstCharToUpper(descrizioneVociFromTFSP02[i].Descrizione);
                var files = myRaiCommonTasks.Helpers.FileManager.GetFileByChiave(descrizioneVociFromTFSP02[i].Id_FoglioSpese.ToString() + "_" + descrizioneVociFromTFSP02[i].progressivoVoce.ToString()).Files;
                if (files.Count() > 0)
                {
                    descrizioneVociFromTFSP02[i].idFile = files[0].Id;
                }
            }
            return descrizioneVociFromTFSP02;
        }
        public List<SpeseProduzioneViewModel> GetDescrizioniAndImportiRendicontiConTarghettaAlDipendente(decimal id)
        {
            SpeseDiProduzioneEntities db = new SpeseDiProduzioneEntities();
            SpeseProduzioneViewModel result = new SpeseProduzioneViewModel();
            var tmp = (from fogliospese in db.TFoglio_Spese
                       join rend in db.TRendiconto on fogliospese.ID equals rend.ID
                       join voci in db.TRendiconto_Voci on rend.ID equals voci.ID
                       join targhetta in db.TRendiconto_Targhetta on rend.ID equals targhetta.ID
                       join tfp01 in db.TFSP01 on targhetta.Tipo equals tfp01.Codice
                       join tfsp02 in db.TFSP02 on voci.Voce equals tfsp02.Codice
                       where fogliospese.ID == id && tfp01.NomeTab.Equals("1013") && targhetta.Tipo.Equals("DI")
                       select new { fogliospese, voci, rend, targhetta, tfp01, tfsp02 } into groupbyID
                       group groupbyID by groupbyID.fogliospese.ID);
            if (tmp != null)
            {
                result.ListaDescrizioniAndImportiRendicontiDipendente = tmp.Select(s => new SpeseProduzioneViewModel()
                {
                    Id_FoglioSpese = s.FirstOrDefault().fogliospese.ID,
                    TipoTarghetta = s.FirstOrDefault().tfp01.Descrizione,
                    Saldo = (s.FirstOrDefault().targhetta.Saldo)

                }).ToList();



            }
            else
            {
                result.ListaDescrizioniAndImportiRendicontiDipendente = new List<SpeseProduzioneViewModel>();
            }
            return result.ListaDescrizioniAndImportiRendicontiDipendente;
        }
        public List<SpeseProduzioneViewModel> GetDescrizioneFromTFSP02ConTipoTaghettaDipendente(decimal id)
        {
            var dataProssima = DateTime.Today;
            SpeseDiProduzioneEntities db = new SpeseDiProduzioneEntities();
            var descrizioneVociFromTFSP02 = from fogliospese in db.TFoglio_Spese
                                            join voci in db.TRendiconto_Voci on fogliospese.ID equals voci.ID
                                            join tfsp02 in db.TFSP02 on voci.Voce equals tfsp02.Codice
                                            join targhetta in db.TRendiconto_Targhetta on fogliospese.ID equals targhetta.ID
                                            where fogliospese.ID == id && targhetta.Tipo.Equals("DI")
                                            orderby voci.ProgressivoVoce
                                            select new { voci, tfsp02, targhetta } into descrizioniAndImporti
                                            group descrizioniAndImporti by descrizioniAndImporti.voci.ProgressivoVoce;




            var listaDescrizioniAndImporti = descrizioneVociFromTFSP02.Select(s => new SpeseProduzioneViewModel()
            {
                Descrizione = s.FirstOrDefault().tfsp02.Descrizione,
                MA_Importo_In_Euro = s.FirstOrDefault().voci.Valore_in_Euro,
                Id_FoglioSpese = s.FirstOrDefault().voci.ID,
                progressivoVoce = s.FirstOrDefault().voci.ProgressivoVoce,
                valutaVoce = s.FirstOrDefault().voci.Valuta

            })/*.OrderByDescending(s => s.MA_Importo_In_Euro)*/.ToList();
            for (int i = 0; i < listaDescrizioniAndImporti.Count(); i++)
            {

                listaDescrizioniAndImporti[i].Descrizione = FirstCharToUpper(listaDescrizioniAndImporti[i].Descrizione);
                var files = myRaiCommonTasks.Helpers.FileManager.GetFileByChiave(listaDescrizioniAndImporti[i].Id_FoglioSpese.ToString() + "_" + listaDescrizioniAndImporti[i].progressivoVoce.ToString()).Files;
                if (files.Count() > 0)
                {
                    listaDescrizioniAndImporti[i].idFile = files[0].Id;
                }
            }
            return listaDescrizioniAndImporti;
        }
        public List<SpeseProduzioneViewModel> GetDescrizioniAndImportiRendicontiConTarghettaAlPersonale(decimal id)
        {
            SpeseDiProduzioneEntities db = new SpeseDiProduzioneEntities();
            SpeseProduzioneViewModel result = new SpeseProduzioneViewModel();
            var tmp = (from fogliospese in db.TFoglio_Spese
                       join rend in db.TRendiconto on fogliospese.ID equals rend.ID
                       join voci in db.TRendiconto_Voci on rend.ID equals voci.ID
                       join targhetta in db.TRendiconto_Targhetta on rend.ID equals targhetta.ID
                       join tfp01 in db.TFSP01 on targhetta.Tipo equals tfp01.Codice
                       join tfsp02 in db.TFSP02 on voci.Voce equals tfsp02.Codice
                       where fogliospese.ID == id && tfp01.NomeTab.Equals("1013") && targhetta.Tipo.Equals("PE")
                       select new { fogliospese, voci, rend, targhetta, tfp01, tfsp02 } into groupbyID
                       group groupbyID by groupbyID.fogliospese.ID);
            if (tmp != null)
            {

                result.ListaDescrizioniAndImportiRendicontiPersonale = tmp.Select(s => new SpeseProduzioneViewModel()
                {
                    Id_FoglioSpese = s.FirstOrDefault().fogliospese.ID,
                    TipoTarghetta = s.FirstOrDefault().tfp01.Descrizione,
                    Saldo = (s.FirstOrDefault().targhetta.Saldo),

                }).ToList();
            }
            else
            {
                result.ListaDescrizioniAndImportiRendicontiPersonale = new List<SpeseProduzioneViewModel>();
            }
            return result.ListaDescrizioniAndImportiRendicontiPersonale;
        }
        public List<SpeseProduzioneViewModel> GetDescrizioneFromTFSP02ConTipoTaghettaPersonale(decimal id)
        {
            var dataProssima = DateTime.Today;
            SpeseDiProduzioneEntities db = new SpeseDiProduzioneEntities();
            var descrizioneVociFromTFSP02 = (from fogliospese in db.TFoglio_Spese
                                             join voci in db.TRendiconto_Voci on fogliospese.ID equals voci.ID
                                             join tfsp02 in db.TFSP02 on voci.Voce equals tfsp02.Codice
                                             join targhetta in db.TRendiconto_Targhetta on fogliospese.ID equals targhetta.ID
                                             where fogliospese.ID == id && targhetta.Tipo.Equals("PE")
                                             select new SpeseProduzioneViewModel()
                                             {
                                                 Descrizione = tfsp02.Descrizione,
                                                 MA_Importo_In_Euro = voci.Valore_in_Euro,
                                                 Id_FoglioSpese = fogliospese.ID,
                                                 progressivoVoce = voci.ProgressivoVoce
                                             }).Distinct().OrderByDescending(s => s.MA_Importo_In_Euro).ToList();

            var tmpImportiRendiconto = (from fogliospese in db.TFoglio_Spese
                                        join rend in db.TRendiconto on fogliospese.ID equals rend.ID
                                        join voci in db.TRendiconto_Voci on rend.ID equals voci.ID
                                        where fogliospese.ID == id
                                        select new { voci } into vociImporti
                                        group vociImporti by vociImporti.voci.ProgressivoVoce).Select(s => new SpeseProduzioneViewModel()
                                        {

                                            MA_Importo_In_Euro = s.FirstOrDefault().voci.Valore_in_Euro
                                        }).Distinct().OrderByDescending(s => s.MA_Importo_In_Euro).ToList();

            if (descrizioneVociFromTFSP02.Count() != 0)
            {
                int j;
                for (int i = 0; i <= descrizioneVociFromTFSP02.Count();)
                {
                    for (j = 0; j < tmpImportiRendiconto.Count();)
                    {
                        var files = myRaiCommonTasks.Helpers.FileManager.GetFileByChiave(descrizioneVociFromTFSP02[i].Id_FoglioSpese.ToString() + "_" + descrizioneVociFromTFSP02[i].progressivoVoce.ToString()).Files;
                        if (files.Count() > 0)
                        {
                            descrizioneVociFromTFSP02[i].idFile = files[0].Id;
                        }
                        descrizioneVociFromTFSP02[i].Descrizione = FirstCharToUpper(descrizioneVociFromTFSP02[i].Descrizione);
                        descrizioneVociFromTFSP02[i].MA_Importo_In_Euro = tmpImportiRendiconto[j].MA_Importo_In_Euro;
                        i++;
                        j++;
                    }
                    break;
                }
            }
            else
            {
                return descrizioneVociFromTFSP02 = new List<SpeseProduzioneViewModel>();
            }
            return descrizioneVociFromTFSP02;
        }
        #endregion
        public int GetLimitFoglioSpese()
        {
            int result = 25;
            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    var rec = db.MyRai_ParametriSistema.Where(w => w.Chiave.Equals("GetLimitElementForFoglioSpese")).FirstOrDefault();
                    if (rec != null)
                    {
                        result = int.Parse(rec.Valore1);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "GetLimitFoglioSpese"
                });

                result = 25;
            }

            return result;
        }
        public List<SpeseProduzioneViewModel> GetListaStati()
        {
            SpeseDiProduzioneEntities db = new SpeseDiProduzioneEntities();
            List<SpeseProduzioneViewModel> listaStati;

            listaStati = db.TImporti_SAP.Select(s => s.MA_Stato).Distinct().Select(s => new SpeseProduzioneViewModel()
            {
                MA_Stato = s
            }).ToList();

            return listaStati;
        }
        public IQueryable<SpeseProduzioneViewModel> GetRiepilogoImportoMeseCorrente()
        {
            IQueryable<SpeseProduzioneViewModel> riepilogo;
            SpeseDiProduzioneEntities db = new SpeseDiProduzioneEntities();
            var meseCorrente = DateTime.Now.Month.ToString();
            var annoCorrente = DateTime.Now.Year.ToString();
            riepilogo = (from s in db.TImporti_SAP
                         join c in db.TFoglio_Spese
                         on s.ID equals c.ID
                         where c.Matricola == matricola && s.MA_Stato == "C"
                         && s.MA_Data.EndsWith(annoCorrente) && s.MA_Data.Substring(1, 3) == (meseCorrente)
                         select new SpeseProduzioneViewModel()
                         {
                             MP_Importo = s.MP_Importo,
                             MA_Importo_In_Euro = s.MA_Importo_In_Euro,
                             MA_Stato = s.MA_Stato

                         });

            return riepilogo;
        }
    }
}