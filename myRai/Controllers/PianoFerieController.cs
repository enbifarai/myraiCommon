using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using myRaiData;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using myRaiHelper;
using myRaiCommonModel;
using myRaiHelper;
using myRaiCommonManager;
using ClosedXML.Excel;
using System.IO;
using System.Globalization;
using myRai.Business;
using myRai.Models;

namespace myRai.Controllers
{
    public class PianoFerieController : Controller
    {
        /// <summary>
        /// Render della view di index dell'interfaccia
        /// di visualizzazione piano ferie
        /// </summary>
        /// <returns></returns>
        public ActionResult Index ( )
        {
            PianoFerieControllerScope.Instance.ScopeModel = null;

            PianoFerieVM model = new PianoFerieVM( )
            {
                AnnoCorrente = DateTime.Now.Year ,
                MeseCorrente = DateTime.Now.Month ,
                SedeGapp = Utente.SedeGapp(DateTime.Now),
                SediGapp = Utility.GetSediGappResponsabileList(),
                Utenti = new List<PianoFerieCalendarItem>( )
            };

            return View( "~/Views/PianoFerie/Index.cshtml" , model );
        }

        [HttpPost]
        public ActionResult RimuoviGiorni(string matricola, string date, string nota, int quantisel)
        {
            var db = new digiGappEntities();
            List<string> Lgiorni = new List<string>();
            if (!String.IsNullOrWhiteSpace(date))
                Lgiorni = date.Split(',').ToList();

            string esito = FeriePermessiManager.RimuoviGiorniPFdipendente(matricola,Lgiorni, nota, quantisel );
            if (esito == null)
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true }
                };
            else
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = esito }
                };
        }
        /// <summary>
        /// Action per la renderizzazione del calendario 
        /// per il mese/anno/sede richiesti.
        /// </summary>
        /// <returns></returns>
        public ActionResult Calendario ( )
        {
            PianoFerieVM model = this.GetPianoFerie(DateTime.Now.Month, DateTime.Now.Year, Utente.SedeGapp(DateTime.Now));

            return View( "~/Views/PianoFerie/Calendario.cshtml" , model );
        }

        /// <summary>
        /// Action per la renderizzazione del calendario 
        /// per il mese/anno/sede richiesti.
        /// Tale action viene richiamata alla pressione delle
        /// frecce di navigazione tra i mesi
        /// </summary>
        /// <param name="mese"></param>
        /// <param name="anno"></param>
        /// <param name="sede"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetSituazioneFerie ( int mese , int anno , string sede )
        {
            if ( mese == 0 )
            {
                mese = 12;
                anno -= 1;
            }

            if ( mese > 12 )
            {
                mese = 1;
                anno += 1;
            }

            PianoFerieVM model = this.GetPianoFerie( mese , anno , sede );

            return View( "~/Views/PianoFerie/Calendario.cshtml" , model );
        }

        [HttpPost]
        public ActionResult GetSituazioneFerieModal ( int mese , int anno , string sede )
        {
            if ( mese == 0 )
            {
                mese = 12;
                anno -= 1;
            }

            if ( mese > 12 )
            {
                mese = 1;
                anno += 1;
            }

            PianoFerieVM model = this.GetPianoFerie( mese , anno , sede );

            return View( "~/Views/PianoFerie/CalendarioModal.cshtml" , model );
        }


        [HttpPost]
        public ActionResult saveNotaPF ( string matricola , int anno , string nota )
        {
            var db = new digiGappEntities( );
            var pf = db.MyRai_PianoFerie.Where( x => x.matricola == matricola && x.anno == anno ).FirstOrDefault( );
            if ( pf == null )
            {
                pf = new MyRai_PianoFerie( );
                pf.matricola = matricola;
                pf.anno = anno;
                db.MyRai_PianoFerie.Add( pf );
            }
            pf.nota_responsabile = nota;
            pf.nota_data = DateTime.Now;
            pf.nota_matricola = CommonManager.GetCurrentUserMatricola();
            db.SaveChanges( );
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                Data = new { result = true , error = "" }
            };
        }


        [HttpPost]
        public ActionResult saveNota ( string matricola , string data , string nota )
        {
            DateTime D;
            if ( DateTime.TryParseExact( data , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out D ) )
            {
                var db = new digiGappEntities( );
                var g = db.MyRai_PianoFerieGiorni.Where( x => x.matricola == matricola && x.data == D ).FirstOrDefault( );
                if ( g != null )
                {
                    g.nota_responsabile = nota;
                    g.nota_data = DateTime.Now;
                    g.nota_matricola = CommonManager.GetCurrentUserMatricola();
                    db.SaveChanges( );
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                        Data = new { result = true , error = "" }
                    };
                }
                else
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                        Data = new { result = false , error = "Not found" }
                    };
                }
            }
            else
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = false , error = "Data non valida" }
                };
            }
        }

        public ActionResult GetNotaPF ( string matricola , int anno )
        {
            var db = new digiGappEntities( );
            var pf = db.MyRai_PianoFerie.Where( x => x.matricola == matricola && x.anno == anno ).FirstOrDefault( );
            if ( pf == null )
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { nota = "" }
                };
            else
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { nota = pf.nota_responsabile }
                };

        }
        public ActionResult getNota ( string matricola , string data )
        {
            DateTime D;
            if ( DateTime.TryParseExact( data , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out D ) )
            {
                var db = new digiGappEntities( );
                var g = db.MyRai_PianoFerieGiorni.Where( x => x.matricola == matricola && x.data == D ).FirstOrDefault( );
                if ( g != null && !String.IsNullOrWhiteSpace( g.nota_responsabile ) )
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                        Data = new { nota = g.nota_responsabile }
                    };
                }

            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                Data = new { nota = "" }
            };

        }
        private ActionResult StornaSedeNew(string sede, int anno)
        {
            var db = new digiGappEntities();
            List<MyRai_PianoFerieSedi> PianiFerieSedi =
                db.MyRai_PianoFerieSedi.Where(x => x.anno == anno && x.sedegapp == sede
                  && x.data_firma == null && x.data_approvata != null
                ).ToList();
            if (PianiFerieSedi.Any())
            {
                string log = "Stornati:";
                foreach (var pf in PianiFerieSedi)
                {
                    log += pf.sedegapp + " id:" + pf.id + " - ";
                    List<MyRai_PianoFerie> PianiFerieDip = db.MyRai_PianoFerie.Where(x => x.Id_pdf_pianoferie_inclusa == pf.id).ToList();

                    if (PianiFerieDip.Any())
                    {
                        foreach (var pfdip in PianiFerieDip)
                        {
                            log += pfdip.matricola + ",";
                            pfdip.data_approvato = null;
                            pfdip.approvatore = null;
                        }
                    }
                }
                var ids = PianiFerieSedi.Select(x => x.id).ToList();
                foreach (int id  in ids)
                {
                    var pfsede = db.MyRai_PianoFerieSedi.Where(x => x.id == id).FirstOrDefault();
                    if (pfsede.data_firma == null)
                    {
                        db.MyRai_PianoFerieSedi.Remove(pfsede);
                    }
                }
                db.SaveChanges();
                Logger.LogAzione(new MyRai_LogAzioni()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "StornaSedeNew",
                    operazione = "Storno PF",
                    descrizione_operazione = log
                });
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = true }
                };
            }
            else
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = false, error = "Non ci sono approvazioni da stornare" }
                };
        }
        [HttpPost]
        public ActionResult StornaSede ( string sede , int anno )
        {
            var db = new digiGappEntities( );
            if ( !String.IsNullOrWhiteSpace( sede ) )
                sede = sede.Replace( "-" , "" );

            try
            {
                return StornaSedeNew(sede, anno);
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = false, error = ex.Message }
                };
            }


            try
            {
                var pdfRows = db.MyRai_PianoFerieSedi.Where( x => x.anno == anno && x.sedegapp == sede
                //  && x.data_approvata != null
                ).OrderByDescending( x => x.numero_versione ).ToList( );
                foreach ( var row in pdfRows )
                {
                    //row.data_approvata = null;
                    //row.data_storno_approvazione = DateTime.Now;
                    //row.matricola_storno = CommonManager.GetCurrentUserMatricola();
                    db.MyRai_PianoFerieSedi.Remove( row );
                }
                db.SaveChanges( );
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = true }
                };
            }
            catch ( Exception ex )
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = false , error = ex.Message }
                };
            }

        }



        public ActionResult AggiornaDescReparto ( )
        {
            var db = new digiGappEntities( );
            var list = db.MyRai_PianoFerieSedi.Where( x => x.sedegapp.Length > 5 && x.desc_reparto.StartsWith( "Reparto" ) ).ToList( );
            foreach ( var item in list )
            {
                string sede = item.sedegapp;
                LinkedTableDataController L = new LinkedTableDataController( );
                RepartoLinkedServer dett = L.GetDettagliReparto(sede.Substring(0, 5), sede.Substring(5));
                if ( dett != null )
                    item.desc_reparto = dett.Descr_Reparto;
                else
                    item.desc_reparto = "Reparto " + sede.Substring( 5 );
            }
            db.SaveChanges( );

            return Content( "OK" );
        }


        private byte[] GeneraPDF(string sede, int anno, bool modificaDB = true, string matricole = null)
        {
            var db = new digiGappEntities( );
            string reparto = null;
            if (sede.Contains("-")) reparto = sede.Split('-')[1];

            if (!String.IsNullOrWhiteSpace(sede)) sede = sede.Replace("-", "");

            List<MyRai_PianoFerie> PianoFerieConsolidati = db.MyRai_PianoFerie.Where(x => x.sedegapp == sede && x.anno == anno && x.data_consolidato != null).ToList();
            if (matricole != null)
                PianoFerieConsolidati = PianoFerieConsolidati.Where(x => matricole.Split(',').Contains(x.matricola)).ToList();

            if ( modificaDB )
            {
                foreach (var p in PianoFerieConsolidati)
                {
                    if ( p.data_approvato == null )
                    {
                        p.data_approvato = DateTime.Now;
                        p.approvatore = CommonManager.GetCurrentUserMatricola();
                    }
                }
                db.SaveChanges( );
            }



            string sedeDesc = db.L2D_SEDE_GAPP.Where( x => x.cod_sede_gapp.Trim( ) == sede.Trim( ).Substring( 0 , 5 ) ).Select( x => x.desc_sede_gapp ).FirstOrDefault( );

            string descReparto = null;
            if ( sede.Length > 5 )
            {
                LinkedTableDataController L = new LinkedTableDataController( );
                RepartoLinkedServer dett = L.GetDettagliReparto(sede.Substring(0, 5), sede.Substring(5));
                if (dett != null) descReparto = dett.Descr_Reparto;
                else descReparto = "Reparto " + sede.Substring(5);
            }
            var pdfRows = db.MyRai_PianoFerieSedi.Where( x => x.anno == anno && x.sedegapp == sede.Replace( "-" , "" ) ).OrderByDescending( x => x.numero_versione ).ToList( );
            int vers = 1;

            if ( pdfRows.Count > 0 )
            {
                vers = pdfRows.First( ).numero_versione + 1;
            }

            MyRaiServiceInterface.it.rai.servizi.digigappws.WSDigigapp wsService = new MyRaiServiceInterface.it.rai.servizi.digigappws.WSDigigapp()
            {
                Credentials = new System.Net.NetworkCredential(
                     CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                      CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1])
            };
            MyRaiServiceInterface.it.rai.servizi.digigappws.pianoFerieSedeGapp resp =
                wsService.getPianoFerieSedeGapp("P" + CommonManager.GetCurrentUserMatricola(),
                "01" + "01" + anno, "", "", sede.ToUpper().Substring(0, 5), 75);

            if (reparto != null)
                resp.dipendenti = resp.dipendenti.Where(x => x.reparto == reparto).ToArray();


            //giro
            resp.dipendenti = FeriePermessiManager.GetUlterioriDipendenti(sede, reparto, DateTime.Now.Year,
                resp.dipendenti, wsService);

            byte[] bytes = myRaiCommonTasks.CommonTasks.GeneraPdfPianoFerie(sede, sedeDesc, anno, 
                CommonManager.GetCurrentUserMatricola(), descReparto, vers, modificaDB, matricole,
                resp);

            var MatricoleApprovate = db.MyRai_PianoFerie.Where( x => x.sedegapp == sede.Replace( "-" , "" ) && x.anno == anno && x.data_approvato != null ).Select( x => x.matricola ).ToList( );
            string matr = String.Join( "," , MatricoleApprovate.ToArray( ) );

            var PFinclusi = db.MyRai_PianoFerie.Where(x => x.sedegapp == sede.Replace("-", "") && x.anno == anno && x.data_approvato != null).ToList();

            if ( modificaDB )
            {
                var VersioniPrecedenti = db.MyRai_PianoFerieSedi.Where(x => x.sedegapp == sede.Replace("-", "") && x.anno == anno && x.data_firma == null
                && x.numero_versione < vers).Select(x => x.id).ToList();
                if (VersioniPrecedenti.Any())
                {
                    foreach (int id in VersioniPrecedenti)
                    {
                        var p = db.MyRai_PianoFerieSedi.Where(x => x.id == id).FirstOrDefault();
                        db.MyRai_PianoFerieSedi.Remove(p);
                    }
                }
                var PianoFerieSede = new myRaiData.MyRai_PianoFerieSedi( )
                {
                    anno = anno ,
                    data_approvata = DateTime.Now ,
                    desc_reparto = descReparto ,
                    desc_sede = sedeDesc ,
                    matricola_approvatore = CommonManager.GetCurrentUserMatricola(),
                    numero_versione = vers ,
                    sedegapp = sede.Replace( "-" , "" ) ,
                    matricole_incluse = matr ,
                    pdf = bytes
                };
                db.MyRai_PianoFerieSedi.Add( PianoFerieSede );
                db.SaveChanges();
                foreach (var pf in PFinclusi)
                {
                    if (pf.Id_pdf_pianoferie_inclusa != null && db.MyRai_PianoFerieSedi.Any(x => x.id == pf.Id_pdf_pianoferie_inclusa))
                    {
                        continue;
                    }
                    pf.Id_pdf_pianoferie_inclusa = PianoFerieSede.id;
                }
                db.SaveChanges( );
            }


            return bytes;
        }
        [HttpPost]
        public ActionResult ApprovaSede(string sede, int anno, string matricole)
        {
            var db = new digiGappEntities( );
            try
            {
                GeneraPDF(sede, anno, true, matricole);
                Logger.LogAzione(new MyRai_LogAzioni()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "ApprovaSede",
                    operazione = "Approva PF",
                    descrizione_operazione = "Matricole :" + matricole
                });
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = true }
                };
            }
            catch ( Exception ex )
            {
                Logger.LogErrori( new MyRai_LogErrori( )
                {
                    applicativo = "PORTALE" ,
                    data = DateTime.Now ,
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "ApprovaSede" ,
                    error_message = ex.ToString( )
                } );
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = false , error = ex.Message }
                };
            }
        }

        public ActionResult DownloadPDFPF ( string sede , int anno , string att )
        {
            if ( att == "1" )
            {
                return GetPDFNow( sede , anno );
            }
            var db = new myRaiData.digiGappEntities( );
            var sedi = Utility.GetSediGappResponsabileList();
            if ( !sedi.Any( x => x.CodiceSede == sede.Substring( 0 , 5 ).ToUpper( ) ) )
                throw new Exception( "NON AUTORIZZATO" );


            sede = sede.Replace( "-" , "" );
            var sedepdf = db.MyRai_PianoFerieSedi.Where( x => x.sedegapp == sede && x.anno == anno ).OrderByDescending( x => x.numero_versione ).FirstOrDefault( );
            if ( sedepdf != null )
                return File( sedepdf.pdf , "application/pdf" , "PianoFerie_" + sede + "_" + anno + "_ver" + sedepdf.numero_versione + ".pdf" );
            else
                return null;
        }
        public ActionResult ViewPdf(int idPdf)
        {
            var db = new myRaiData.digiGappEntities();
            var pdf = db.MyRai_PianoFerieSedi.Where(x => x.id == idPdf).FirstOrDefault();

            if (pdf == null) return null;

            byte[] byteArray = pdf.pdf;
            MemoryStream pdfStream = new MemoryStream();
            pdfStream.Write(byteArray, 0, byteArray.Length);
            pdfStream.Position = 0;

            Response.AppendHeader("content-disposition", "inline; filename=" + pdf.pdf + ".pdf");
            return new FileStreamResult(pdfStream, "application/pdf");
        }
        public ActionResult GetPDFNow(string sede, int anno)
        {
            byte[] b = GeneraPDF( sede , anno , false );
            return File( b , "application/pdf" , "PianoFerie_" + sede + "_" + anno + "_" + DateTime.Now.ToString( "ddMMyyyy" ) + ".pdf" );
        }

        //public ActionResult getPfPDF(string sede, int anno)
        //{
        //    //http://localhost:53693/pianoferie/getpfpdf?sede=dde30&anno=2019
        //    var db = new myRaiData.digiGappEntities();
        //    string sedeDesc = db.L2D_SEDE_GAPP.Where(x => x.cod_sede_gapp.Trim() == sede.Trim().Substring(0, 5)).Select(x => x.desc_sede_gapp).FirstOrDefault();

        //    string descReparto = null;
        //    if (sede.Length > 5)
        //    {
        //        LinkedTableDataController L = new LinkedTableDataController();
        //        RepartoLinkedServer dett = L.GetDettagliReparto(sede.Substring(0, 5), sede.Substring(6));
        //        if (dett != null) descReparto = dett.Descr_Reparto;
        //        else descReparto = "Reparto " + sede.Substring(6);
        //    }
        //    var pdfRows = db.MyRai_PianoFerieSedi.Where(x => x.anno == anno && x.sedegapp == sede.Replace("-", "")).OrderByDescending(x => x.numero_versione).ToList();
        //    int vers = 1;

        //    if (pdfRows.Count > 0)
        //    {
        //        vers = pdfRows.First().numero_versione + 1;
        //    }

        //    byte[] bytes = myRaiCommonTasks.CommonTasks.GeneraPdfPianoFerie(sede, sedeDesc, anno, CommonManager.GetCurrentUserMatricola(), descReparto, vers);

        //    var MatricoleApprovate = db.MyRai_PianoFerie.Where(x => x.sedegapp == sede.Replace("-", "") && x.anno == anno && x.data_approvato != null).Select(x => x.matricola).ToList();
        //    string matr = String.Join(",", MatricoleApprovate.ToArray());



        //    var PianoFerieSede = new myRaiData.MyRai_PianoFerieSedi()
        //    {
        //        anno = anno,
        //        data_approvata = DateTime.Now,
        //        desc_reparto = descReparto,
        //        desc_sede = sedeDesc,
        //        matricola_approvatore = CommonManager.GetCurrentUserMatricola(),
        //        numero_versione = 1,
        //        sedegapp = sede.Replace("-", ""),
        //        matricole_incluse = matr,
        //        pdf = bytes
        //    };
        //    db.MyRai_PianoFerieSedi.Add(PianoFerieSede);
        //    db.SaveChanges();
        //    return File(bytes, "application/pdf", "PianoFerie_" + sede + "_" + anno + ".pdf");
        //}

        public void AddSedeReparti ( string text , string value , List<SedeRepartiForSelect> L )
        {
            if ( !L.Any( x => x.text == text ) )
                L.Add( new SedeRepartiForSelect( ) { text = text , value = value } );
        }
        public List<SedeRepartiForSelect> getSediReparti ( List<Sede> sedi , int anno )
        {
            var db = new digiGappEntities( );
            var AB = CommonManager.getAbilitazioni();

            List<SedeRepartiForSelect> L = new List<SedeRepartiForSelect>( );
            LinkedTableDataController lt = new LinkedTableDataController( );

            foreach ( var sede in sedi )
            {
                if ( sede.RepartiSpecifici.Any( x => x.reparto == "*" ) )
                {
                    var results = AB.ListaAbilitazioni.Where( x => x.Sede.StartsWith( sede.CodiceSede ) );
                    if ( results.Any( x => x.Sede.Length > 5 ) )
                    {
                        foreach ( var result in results.Where( a => a.Sede.Length > 5 ) )
                        {
                            string repar = result.Sede.Substring( 5 );
                            var r = lt.GetDettagliReparto(result.Sede.Substring(0, 5), repar);
                            if ( r != null )
                                AddSedeReparti( sede.CodiceSede + " " + sede.DescrizioneSede + " - " + r.Descr_Reparto , sede.CodiceSede + "-" + repar , L );
                            else
                                AddSedeReparti( sede.CodiceSede + " " + sede.DescrizioneSede + " - Reparto " + repar , sede.CodiceSede + "-" + repar , L );
                        }
                    }
                    // else
                    AddSedeReparti( sede.CodiceSede + " " + sede.DescrizioneSede , sede.CodiceSede , L );
                }
                else
                {
                    foreach ( var reparto in sede.RepartiSpecifici )
                    {
                        var r = lt.GetDettagliReparto(sede.CodiceSede.Substring(0, 5), reparto.reparto);
                        if ( r != null )
                            AddSedeReparti( sede.CodiceSede + " " + sede.DescrizioneSede + " -  " + r.Descr_Reparto , sede.CodiceSede + "-" + reparto.reparto , L );
                        else
                            AddSedeReparti( sede.CodiceSede + " " + sede.DescrizioneSede + " - Reparto " + reparto.reparto , sede.CodiceSede + "-" + reparto.reparto , L );
                    }
                }
            }
            return L.OrderBy( x => x.text ).ToList( );
        }

        public ActionResult IsFElocked ( int id_richiesta )
        {
            string matr = CommonManager.GetCurrentUserMatricola();
            string sede = Utente.SedeGapp(DateTime.Now);
            string rep = Utente.Reparto();

            myRaiCommonTasks.CommonTasks.CancellazioneFerie locked = myRaiCommonTasks.CommonTasks.IsDayToReplan( id_richiesta , matr , sede , rep );
            //bool esente = CommonManager.IsEsentePianoFerie(matr);

            if ( locked == myRaiCommonTasks.CommonTasks.CancellazioneFerie.CancellazioneNormale )
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = false }
                };

            else if ( locked == myRaiCommonTasks.CommonTasks.CancellazioneFerie.CancellazioneConRipianificazione )
            {
                var db = new digiGappEntities( );
                var rich = db.MyRai_Richieste.Where( x => x.id_richiesta == id_richiesta ).FirstOrDefault( );
                if ( rich != null )
                {
                    var cp = FeriePermessiManager.PuoStornareSenzaRipianificazione( rich.periodo_dal.Year , rich.matricola_richiesta );
                    if ( cp.StornoPossibileSenzaRipianificazione )
                    {
                        Logger.LogAzione( new MyRai_LogAzioni( )
                        {
                            applicativo = "PORTALE" ,
                            data = DateTime.Now ,
                            matricola = matr ,
                            provenienza = "IsFElocked" ,
                            operazione = "StornoFerieRipianificazione" ,
                            descrizione_operazione = "Storno senza ripian per " + rich.periodo_dal.ToString( "dd/MM/yyyy" ) + " - FE inserite:" + cp.FerieInserite + " - FE minimo:" + cp.FerieMinimo
                                                     + " - Storni NON ripian trovati:" + cp.StorniNonRipianificati
                        } );
                        return new JsonResult
                        {
                            JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                            Data = new { result = false } //no ripian
                        };
                    }
                }

                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = true }
                };
            }
            else return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { error = "Non è possibile ripianificare questa richiesta, rivolgersi alla segreteria per dettagli" }
                };
        }


        private PianoFerieApprovatoreModel GetPianoFerieModel( String sede , int? anno )
        {
            PianoFerieApprovatoreModel model = null;

            WSDigigapp wsService = new WSDigigapp( )
            {
                Credentials = new System.Net.NetworkCredential(
                        CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                        CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1])
            };
            if ( anno == null )
                anno = DateTime.Now.Year;

            bool almenoUnUtenteRichiede2021 = false;
            
            var sedi = Utility.GetSediGappResponsabileList();
            var ListSediReparti = getSediReparti( sedi , ( int ) anno );
            if ( sedi.Any( ) )
            {
                if ( String.IsNullOrWhiteSpace( sede ) )
                    sede = ListSediReparti[0].value;

                string repartoReq = null;

                if ( sede.Length > 5 )
                {
                    repartoReq = sede.Substring( 6 );
                }

                pianoFerieSedeGapp resp = wsService.getPianoFerieSedeGapp("P" + Utente.Matricola(), "01" + "01" + anno, "", "", sede.Substring(0, 5), 75);
                pianoFerieSedeGapp resp2 = null;

                var db = new myRaiData.digiGappEntities();

                var AB = CommonManager.getAbilitazioni();
                List<string> SediAb = AB.ListaAbilitazioni.Where( x => x.Sede.ToUpper( ).StartsWith( sede.Substring( 0 , 5 ).ToUpper( ) ) ).Select( x => x.Sede ).ToList( );

                Dipendente[] MieiDipendenti = null;
                Dipendente[] MieiDipendenti2 = null;

                if ( repartoReq == null )
                    MieiDipendenti = resp.dipendenti.Where( x => !SediAb.Contains( sede + x.reparto ) || String.IsNullOrWhiteSpace( x.reparto ) || x.reparto.Trim( ) == "0" || x.reparto.Trim( ) == "00" ).ToArray( );
                else
                    MieiDipendenti = resp.dipendenti.Where( x => x.reparto == repartoReq ).ToArray( );

                MieiDipendenti = FeriePermessiManager.GetUlterioriDipendenti(sede.Substring(0, 5), repartoReq,
                    DateTime.Now.Year, MieiDipendenti, wsService);

                //var PianoFeriePerGiroItalia = db.MyRai_PianoFerie.Where(x => x.sedegapp == sede.Substring(0, 5) + repartoReq &&
                //                              x.anno == DateTime.Now.Year).ToList();
                //pianoFerieSedeGapp resp8REC0 = null;
                //foreach (var item in PianoFeriePerGiroItalia)
                //{
                //    var sedePrecedente = HomeManager.GetSedeGappPrecedente(item.matricola, DateTime.Now);
                //    if (sedePrecedente != null) //è in tab
                //    {
                //        var md = MieiDipendenti.Where(x => x.matricola =="0"+ item.matricola).FirstOrDefault();
                //        if (md != null)
                //        {
                //            //gia in list
                //            continue;
                //        }
                //        if (resp8REC0 == null)
                //        {
                //          //  var respp = wsService.getPianoFerie(item.matricola, "01012021", 80, "");
                          
                //            resp8REC0 = wsService.getPianoFerieSedeGapp("P" + Utente.Matricola(),
                //           "01" + "01" + anno, "", "", "8REC0", 75);
                //        }
                //        var dip = resp8REC0.dipendenti.Where(x => x.matricola == "0" + item.matricola).FirstOrDefault();
                //        if (dip != null)
                //        {
                //            var list = MieiDipendenti.ToList();
                //            list.Add(dip);
                //            MieiDipendenti = list.OrderBy(x => x.matricola).ToArray();
                //        }
                //    }
                //}

                resp.dipendenti = MieiDipendenti;

                List<string> cessate = CommonManager.GetCessate(MieiDipendenti.Select(x => x.matricola.Substring(1)).ToArray());

                if ( cessate.Any( ) )
                {
                    List<Dipendente> LD = new List<Dipendente>( );
                    foreach ( var d in resp.dipendenti )
                    {
                        if ( !cessate.Contains( d.matricola.Substring( 1 ) ) )
                            LD.Add( d );
                    }
                    resp.dipendenti = LD.ToArray( );
                }

                foreach ( var e in resp.dipendenti )
                {
                    almenoUnUtenteRichiede2021 = myRaiCommonTasks.CommonTasks.Richiede2021( e.matricola );

                    if ( almenoUnUtenteRichiede2021 )
                    {
                        break;
                    }
                }

                if ( almenoUnUtenteRichiede2021 )
                {
                    resp2 = wsService.getPianoFerieSedeGapp("P" + Utente.Matricola(), "01" + "01" + (anno + 1), "", "", sede.Substring(0, 5), 75);

                    if ( repartoReq == null )
                        MieiDipendenti2 = resp2.dipendenti.Where( x => !SediAb.Contains( sede + x.reparto ) || String.IsNullOrWhiteSpace( x.reparto ) || x.reparto.Trim( ) == "0" || x.reparto.Trim( ) == "00" ).ToArray( );
                    else
                        MieiDipendenti2 = resp2.dipendenti.Where( x => x.reparto == repartoReq ).ToArray( );

                    resp2.dipendenti = MieiDipendenti2;

                    List<string> cessate2 = CommonManager.GetCessate(MieiDipendenti2.Select(x => x.matricola.Substring(1)).ToArray());

                    if ( cessate2.Any( ) )
                    {
                        List<Dipendente> LD = new List<Dipendente>( );
                        foreach ( var d in resp2.dipendenti )
                        {
                            if ( !cessate2.Contains( d.matricola.Substring( 1 ) ) )
                                LD.Add( d );
                        }
                        resp2.dipendenti = LD.ToArray( );
                    }
                }

                model = new PianoFerieApprovatoreModel( )
                {
                    response = resp ,
                    Sedi = sedi ,
                    SediRepartiList = getSediReparti( sedi , ( int ) anno ) ,
                    SedeSelezionataCodice = sede ,
                    SedeSelezionataDesc = sedi.Where( x => x.CodiceSede == sede.Substring( 0 , 5 ) ).Select( x => x.DescrizioneSede ).FirstOrDefault( ) ,
                    //    SedeSelezionataApprovataData = db.MyRai_PianoFerieSedi.Where(x => x.sedegapp == sede.Replace("-", "")).OrderByDescending(x => x.data_approvato).Select(x => x.data_approvato).FirstOrDefault()
                };

                var pfsede = db.MyRai_PianoFerieSedi.Where( x => x.data_approvata != null && x.anno == anno &&
                 x.sedegapp == sede.Replace( "-" , "" ) ).OrderByDescending( x => x.numero_versione ).FirstOrDefault( );

                if ( pfsede == null )
                {
                    model.SedeSelezionataApprovataData = null;
                    model.SedeSelezionataFirmataData = null;
                }
                else
                {
                    model.SedeSelezionataApprovataData = pfsede.data_approvata;
                    model.SedeSelezionataFirmataData = pfsede.data_firma;
                }

                if ( model.SedeSelezionataCodice.Length > 5 )
                {
                    LinkedTableDataController L = new LinkedTableDataController( );
                    var dett = L.GetDettagliReparto( model.SedeSelezionataCodice.Substring( 0 , 5 ) , model.SedeSelezionataCodice.Substring( 5 ) );
                    if ( dett != null )
                        model.SedeSelezionataRepartoDescrittiva = dett.Descr_Reparto;
                    else
                        model.SedeSelezionataRepartoDescrittiva = "Reparto " + model.SedeSelezionataCodice.Substring( 5 );
                }

                DateTime D = DateTime.Now;

                int? IdTipologiaEsenteFerie = db.MyRai_InfoDipendente_Tipologia.Where( x => x.info == "Esente Piano Ferie" && x.data_inizio <= D && ( x.data_fine >= D || x.data_fine == null ) )
                                    .Select( x => x.id ).FirstOrDefault( );

                model.EsentatiPianoFerie = new List<string>( );

                if ( IdTipologiaEsenteFerie != null )
                {
                    string[] MieiDipMatricole = MieiDipendenti.Select( x => x.matricola.Substring( 1 ) ).ToArray( );
                    model.EsentatiPianoFerie = db.MyRai_InfoDipendente.Where( x =>
                                                              MieiDipMatricole
                                                             .Contains( x.matricola ) && x.id_infodipendente_tipologia == IdTipologiaEsenteFerie && D >= x.data_inizio && D <= x.data_fine
                                                              && x.valore != null && x.valore.ToLower( ) == "true" )
                                                            .Select( x => x.matricola ).ToList( );
                }

                foreach ( var dip in MieiDipendenti )
                {
                    string matDip = dip.matricola.Substring( 1 );
                    if ( cessate.Contains( matDip ) )
                        continue;

                    var pfmatr = db.MyRai_PianoFerie.Where( x => x.matricola == matDip && x.anno == anno
                     && x.data_consolidato != null
                    ).FirstOrDefault( );

                    List<DateTime> FE = new List<DateTime>( );
                    List<DateTime> RR = new List<DateTime>( );
                    List<DateTime> RN = new List<DateTime>( );
                    List<DateTime> PR = new List<DateTime>( );
                    List<DateTime> PF = new List<DateTime>( );
                    List<DateTime> PX = new List<DateTime>( );
                    List<DateTime> ConNote = new List<DateTime>( );

                    List<DateTime> Gapp_FE = new List<DateTime>( );
                    List<DateTime> Gapp_PF = new List<DateTime>( );
                    List<DateTime> Gapp_PR = new List<DateTime>( );

                    FE.AddRange( db.MyRai_PianoFerieGiorni.Where( x => x.data.Year == anno && x.matricola == matDip && x.eccezione == "FE" ).Select( x => x.data ).ToList( ) );
                    RR.AddRange( db.MyRai_PianoFerieGiorni.Where( x => x.data.Year == anno && x.matricola == matDip && x.eccezione == "RR" ).Select( x => x.data ).ToList( ) );
                    RN.AddRange( db.MyRai_PianoFerieGiorni.Where( x => x.data.Year == anno && x.matricola == matDip && x.eccezione == "RN" ).Select( x => x.data ).ToList( ) );
                    PR.AddRange( db.MyRai_PianoFerieGiorni.Where( x => x.data.Year == anno && x.matricola == matDip && x.eccezione == "PR" ).Select( x => x.data ).ToList( ) );
                    PF.AddRange( db.MyRai_PianoFerieGiorni.Where( x => x.data.Year == anno && x.matricola == matDip && x.eccezione == "PF" ).Select( x => x.data ).ToList( ) );
                    PX.AddRange( db.MyRai_PianoFerieGiorni.Where( x => x.data.Year == anno && x.matricola == matDip && x.eccezione == "PX" ).Select( x => x.data ).ToList( ) );
                    ConNote.AddRange( db.MyRai_PianoFerieGiorni.Where( x => x.data.Year == anno && x.matricola == matDip && x.nota_responsabile != null && x.nota_responsabile.Trim( ) != "" ).Select( x => x.data ).ToList( ) );

                    Gapp_FE.AddRange( dip.ferie.giornate.Where( x => x.tipoGiornata == "D" ).Select( x => x.data ).ToList( ) );
                    Gapp_PF.AddRange( dip.ferie.giornate.Where( x => x.tipoGiornata == "1" ).Select( x => x.data ).ToList( ) );
                    Gapp_PR.AddRange( dip.ferie.giornate.Where( x => x.tipoGiornata == "2" ).Select( x => x.data ).ToList( ) );
                    if (FE.Any())
                    {
                        var FEstornate = db.MyRai_Eccezioni_Richieste.Where(x => x.MyRai_Richieste.matricola_richiesta == matDip &&
                        x.cod_eccezione == "FE" && x.azione=="C"
                        && (x.id_stato == 20 || x.id_stato==70)).Select(x=>x.data_eccezione).ToList();
                        FE.RemoveAll(x => FEstornate.Contains(x));
                    }

                    if ( almenoUnUtenteRichiede2021 )
                    {
                        FE.AddRange( db.MyRai_PianoFerieGiorni.Where( x => ( x.data.Year == anno + 1 && x.data.Month <= 3 ) && x.matricola == matDip && x.eccezione == "FE" ).Select( x => x.data ).ToList( ) );

                        RR.AddRange( db.MyRai_PianoFerieGiorni.Where( x => ( x.data.Year == anno + 1 && x.data.Month <= 3 ) && x.matricola == matDip && x.eccezione == "RR" ).Select( x => x.data ).ToList( ) );

                        RN.AddRange( db.MyRai_PianoFerieGiorni.Where( x => ( x.data.Year == anno + 1 && x.data.Month <= 3 ) && x.matricola == matDip && x.eccezione == "RN" ).Select( x => x.data ).ToList( ) );

                        PR.AddRange( db.MyRai_PianoFerieGiorni.Where( x => ( x.data.Year == anno + 1 && x.data.Month <= 3 ) && x.matricola == matDip && x.eccezione == "PR" ).Select( x => x.data ).ToList( ) );

                        PF.AddRange( db.MyRai_PianoFerieGiorni.Where( x => ( x.data.Year == anno + 1 && x.data.Month <= 3 ) && x.matricola == matDip && x.eccezione == "PF" ).Select( x => x.data ).ToList( ) );

                        PX.AddRange( db.MyRai_PianoFerieGiorni.Where( x => ( x.data.Year == anno + 1 && x.data.Month <= 3 ) && x.matricola == matDip && x.eccezione == "PX" ).Select( x => x.data ).ToList( ) );

                        ConNote.AddRange( db.MyRai_PianoFerieGiorni.Where( x => ( x.data.Year == anno + 1 && x.data.Month <= 3 ) && x.matricola == matDip && x.nota_responsabile != null && x.nota_responsabile.Trim( ) != "" ).Select( x => x.data ).ToList( ) );

                        var dip2 = MieiDipendenti2.Where( w => w.matricola.Equals( dip.matricola ) ).FirstOrDefault( );
                        if ( dip2 != null )
                        {
                            Gapp_FE.AddRange( dip2.ferie.giornate.Where( x => x.tipoGiornata == "D" ).Select( x => x.data ).ToList( ) );
                            Gapp_PF.AddRange( dip2.ferie.giornate.Where( x => x.tipoGiornata == "1" ).Select( x => x.data ).ToList( ) );
                            Gapp_PR.AddRange( dip2.ferie.giornate.Where( x => x.tipoGiornata == "2" ).Select( x => x.data ).ToList( ) );
                        }
                    }

                    if ( pfmatr != null )
                    {
                        model.PianoFerieDipendenti.Add( new GiorniPianoFeriePerDip( )
                        {
                            Matricola = matDip ,
                            GiorniFerie = FE ,
                            GiorniRR = RR ,
                            GiorniRN = RN ,

                            GiorniPR = PR ,
                            GiorniPF = PF ,
                            GiorniPX = PX ,

                            GiorniConNote = ConNote ,
                            NotaSuPianoFerieDaResp = pfmatr.nota_responsabile ,
                            Gapp_FE = Gapp_FE ,
                            Gapp_PF = Gapp_PF ,
                            Gapp_PR = Gapp_PR ,
                            DataApprovazione = pfmatr.data_approvato 
                        } );
                    }
                    else
                    {
                        if ( model.SedeSelezionataFirmataData != null )
                        {
                            model.PianoFerieDipendenti.Add( new GiorniPianoFeriePerDip( )
                            {
                                Matricola = matDip ,
                                GiorniFerie = FE ,
                                GiorniRR = RR ,
                                GiorniRN = RN ,

                                GiorniPR = PR ,
                                GiorniPF = PF ,
                                GiorniPX = PX ,

                                GiorniConNote = ConNote ,
                                NotaSuPianoFerieDaResp = null ,
                                Gapp_FE = Gapp_FE ,
                                Gapp_PF = Gapp_PF ,
                                Gapp_PR = Gapp_PR ,
                                DataApprovazione = null ,
                                NessunInvioPianoFerie = true

                            } );
                        }
                        else
                        {
                            model.PianoFerieDipendenti.Add( new GiorniPianoFeriePerDip( )
                            {
                                Matricola = matDip ,
                                GiorniFerie = new List<DateTime>( ) ,
                                GiorniRR = new List<DateTime>( ) ,
                                GiorniRN = new List<DateTime>( ) ,
                                GiorniPF = new List<DateTime>( ) ,
                                GiorniPR = new List<DateTime>( ) ,
                                GiorniPX = new List<DateTime>( ) ,

                                GiorniConNote = new List<DateTime>( ) ,
                                Gapp_FE = Gapp_FE ,
                                Gapp_PF = Gapp_PF ,
                                Gapp_PR = Gapp_PR ,
                                NonInviato = true
                            } );
                        }
                    }
                }
            }
            model.AlmenoUnUtenteRichiede2021 = false;// almenoUnUtenteRichiede2021;

            var db2 = new digiGappEntities( );
            model.MatricoleCheRichiedonoAnnoSuccessivo = db2.MyRai_ArretratiExcel2019.Where( w => w.estensione_marzo == true ).ToList( ).Select( w => w.matricola ).ToList( );
            return model;
        }

        public ActionResult GetPfApp(String sede, int? anno)
        {
            WSDigigapp wsService = new WSDigigapp()
            {
                Credentials = new System.Net.NetworkCredential(
                        CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                        CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1])
            };
            if (anno == null) anno = DateTime.Now.Year;

            var sedi = Utility.GetSediGappResponsabileList();
            var ListSediReparti = getSediReparti(sedi, (int)anno);

            if (sedi.Any())
            {
                PianoFerieApprovatoreModel model = GetPianoFerieModel( sede , anno );

                if (model != null)
                {
                    int changed= FeriePermessiManager.AllineaSediPF(model, anno);
                    if (changed>0)
                        model = GetPianoFerieModel(sede, anno);

                return View( model );
            }
            else
                {
                    return Content( "ERRORE NEL CARICAMENTO DEI DATI" );
                }
            }
            else return Content("NON AUTORIZZATO");
        }

        /// <summary>
        /// Reperimento degli utenti e delle rispettive eccezioni richieste 
        /// per il mese/anno/sede passati
        /// </summary>
        /// <param name="mese"></param>
        /// <param name="anno"></param>
        /// <param name="sede"></param>
        /// <returns></returns>
        private PianoFerieVM GetPianoFerie ( int mese , int anno , string sede )
        {
            PianoFerieVM model = new PianoFerieVM( );
            try
            {
                PianoFerieVM sessionModel = PianoFerieControllerScope.Instance.ScopeModel;

                if ( sessionModel != null )
                {
                    if ( sessionModel.AnnoCorrente.Equals( anno ) &&
                        sessionModel.MeseCorrente.Equals( mese ) &&
                        sessionModel.SedeGapp.Equals( sede ) )
                    {
                        return sessionModel;
                    }
                }

                model.AnnoCorrente = anno;
                model.MeseCorrente = mese;
                model.SedeGapp = sede;
                model.Utenti = new List<PianoFerieCalendarItem>( );
                model.SediGapp = Utility.GetSediGappResponsabileList();

                DateTime start = new DateTime( model.AnnoCorrente , model.MeseCorrente , 1 , 0 , 0 , 0 );
                DateTime stop = start.AddMonths( 1 );
                stop = stop.AddDays( -1 );

                int giorniNelMese = stop.Day;
                int bCount = 0;

                WSDigigapp wsService = new WSDigigapp( )
                {
                    Credentials = new System.Net.NetworkCredential(
                        CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                        CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1])
                };

                //pianoFerieSedeGapp response = wsService.getPianoFerieSedeGapp( "P"+ Utente.Matricola(), "01" + mese.ToString().PadLeft( 2, '0' ) + anno.ToString(), "", "", sede, 75 );

                pianoFerieSedeGapp response = wsService.getPianoFerieSedeGapp("P" + Utente.Matricola(), "01" + "01" + anno.ToString(), "", "", sede, 75);

                List<MyRai_Eccezioni_Ammesse> eccAmmesse = null;

                if ( response.esito )
                {
                    using ( digiGappEntities db = new digiGappEntities( ) )
                    {
                        var eAmm = from ammesse in db.MyRai_Eccezioni_Ammesse
                                   where ammesse.id_raggruppamento == 1 &&
                                   ammesse.flag_attivo == true
                                   select new { ammesse };

                        if ( eAmm != null && eAmm.Any( ) )
                        {
                            eccAmmesse = new List<MyRai_Eccezioni_Ammesse>( );
                            foreach ( var a in eAmm )
                            {
                                eccAmmesse.Add( a.ammesse );
                            }
                        }
                    }

                    if ( response.dipendenti != null &&
                        response.dipendenti.Any( ) )
                    {
                        foreach ( var item in response.dipendenti )
                        {
                            PianoFerieCalendarItem newObj = new PianoFerieCalendarItem( );
                            newObj.Matricola = item.matricola.Substring( 1 );
                            newObj.Nominativo = item.cognome.Trim( ) + " " + item.nome.Trim( ).Substring( 0 , 1 ) + ".";
                            newObj.TipoDip = string.Empty;
                            newObj.Foto = CommonManager.GetUrlFoto(item.matricola.Substring(1));
                            newObj.Eccezioni = new List<PFCalendarDayDetail>( );

                            Ferie fer = item.ferie;

                            if ( fer != null && fer.giornate != null && fer.giornate.Any( ) )
                            {
                                bCount = 0;
                                foreach ( var g in fer.giornate )
                                {
                                    bool currentMese = ( Int32.Parse( g.dataTeorica.Substring( 3 , 2 ) ) == mese );

                                    if ( !currentMese )
                                    {
                                        continue;
                                    }

                                    CodiciGiornata codiciGiornata = CodiciGiornataHelper.GetCodiceGiornata( g.tipoGiornata );

                                    if ( g.tipoGiornata.Equals( "A" ) ||
                                        g.tipoGiornata.Equals( "B" ) )
                                    {
                                        if ( g.tipoGiornata.Equals( "B" ) && currentMese )
                                        {
                                            bCount++;
                                        }

                                        newObj.Eccezioni.Add( new PFCalendarDayDetail( )
                                        {
                                            Giorno = int.Parse( g.dataTeorica.Substring( 0 , 2 ) ) ,
                                            CodiceEccezione = g.tipoGiornata ,
                                            DescrizioneEccezione = "" ,
                                            ColorePie = "bg-gray"
                                        } );
                                        continue;
                                    }

                                    List<string> codiciG = new List<string>( );
                                    bool isValid = false;

                                    if ( !String.IsNullOrEmpty( codiciGiornata.Codice ) )
                                    {
                                        codiciG = codiciGiornata.Codice.Split( '|' ).ToList( );
                                    }

                                    if ( codiciG.Count( ) == 0 )
                                    {
                                        newObj.Eccezioni.Add( new PFCalendarDayDetail( )
                                        {
                                            Giorno = int.Parse( g.dataTeorica.Substring( 0 , 2 ) ) ,
                                            CodiceEccezione = "Z" ,
                                            DescrizioneEccezione = codiciGiornata.Descrizione ,
                                            ColorePie = "bg-white"
                                        } );
                                    }
                                    else
                                    {
                                        newObj.Eccezioni.Add( new PFCalendarDayDetail( )
                                        {
                                            Giorno = int.Parse( g.dataTeorica.Substring( 0 , 2 ) ) ,
                                            CodiceEccezione = codiciGiornata.Codice ,
                                            DescrizioneEccezione = codiciGiornata.Descrizione ,
                                            ColorePie = codiciGiornata.Colore
                                        } );
                                    }
                                }
                            }

                            // se sono tutte giornate B allora è inutile mostrare l'utente nell'elenco
                            // probabilmente è un utente che è andato in pensione o che comunque è
                            // fuori contratto per tutto il mese
                            if ( bCount == giorniNelMese )
                            {
                                continue;
                            }

                            using ( digiGappEntities db = new digiGappEntities( ) )
                            {
                                var richieste = from eccezione in db.MyRai_Eccezioni_Richieste
                                                join richiesta in db.MyRai_Richieste
                                                    on eccezione.id_richiesta equals richiesta.id_richiesta
                                                join ammesse in db.MyRai_Eccezioni_Ammesse
                                                    on eccezione.cod_eccezione equals ammesse.cod_eccezione
                                                where ( richiesta.id_stato == ( int ) EnumStatiRichiesta.InApprovazione ||
                                                richiesta.id_stato == ( int ) EnumStatiRichiesta.Approvata ) &&
                                                richiesta.codice_sede_gapp == sede &&
                                                ammesse.id_raggruppamento == 1 &&
                                                richiesta.matricola_richiesta == item.matricola.Substring( 1 ) &&
                                                ( eccezione.data_eccezione >= start && eccezione.data_eccezione <= stop )
                                                orderby eccezione.data_eccezione
                                                select new { ecc = eccezione , stato = ( EnumStatiRichiesta ) richiesta.id_stato , descEcc = ammesse.desc_eccezione };

                                if ( richieste != null && richieste.Any( ) )
                                {
                                    foreach ( var e in richieste )
                                    {
                                        newObj.Eccezioni.Add( new PFCalendarDayDetail( )
                                        {
                                            Giorno = e.ecc.data_eccezione.Day ,
                                            CodiceEccezione = e.ecc.cod_eccezione ,
                                            Eccezione = e.descEcc
                                            //Stato = e.stato
                                        } );
                                    }
                                }
                            }

                            model.Utenti.Add( newObj );
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                model.AnnoCorrente = anno;
                model.MeseCorrente = mese;
                model.SedeGapp = sede;
            }

            PianoFerieControllerScope.Instance.ScopeModel = model;
            return model;
        }

        public ActionResult EsportaPianoFerie(string sede, int? anno)
        {
            FileStreamResult myFile = null;
            try
            {
                myFile = EsportaPianoFerieInternal( sede , anno );
            }
            catch ( Exception ex )
            {
                myFile = null;
            }
            return myFile;
        }

        private FileStreamResult EsportaPianoFerieInternal ( string sede , int? anno )
        {
            XLWorkbook workbook = new XLWorkbook( );
            MemoryStream ms = new MemoryStream( );
            var cultureInfo = CultureInfo.GetCultureInfo( "it-IT" );

            // nome del tab excel
            var worksheet = workbook.Worksheets.Add( "ReportPianoFerie" );
            worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
            worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            worksheet.PageSetup.PagesWide = 1;

            if (!anno.HasValue)
            {
                anno = DateTime.Now.Year;
            }

            // imposta il numero di colonne del foglio excel di output
            DateTime data1 = new DateTime( anno.GetValueOrDefault( ) , 1 , 1 );
            DateTime data2 = new DateTime( anno.GetValueOrDefault( ) , 12 , 31 );
            PianoFerieApprovatoreModel model = GetPianoFerieModel( sede , anno );

            var diff = data2 - data1;

            int defaultCell = ( int ) diff.TotalDays + 10;

            if (model.AlmenoUnUtenteRichiede2021)
            {
                DateTime data3 = new DateTime( anno.GetValueOrDefault( ) + 1 , 3 , 31 );
                diff = data3 - data1;
                defaultCell = ( int ) diff.TotalDays + 10;
            }

            int row = 1;
            int countSettimane = 1;

            #region Intestazione

            // le prime 8 colonne delle prime 3 righe saranno vuote
            worksheet.Cell( row , 1 ).Value = " ";
            worksheet.Cell( row , 1 ).Style.Font.FontSize = 15;
            worksheet.Cell( row , 1 ).Style.Border.BottomBorder = XLBorderStyleValues.None;
            worksheet.Cell( row , 1 ).Style.Border.TopBorder = XLBorderStyleValues.None;
            worksheet.Cell( row , 1 ).Style.Border.LeftBorder = XLBorderStyleValues.None;
            worksheet.Cell( row , 1 ).Style.Border.RightBorder = XLBorderStyleValues.None;
            worksheet.Cell( row , 1 ).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell( row , 1 ).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell( row , 1 ).Style.Alignment.WrapText = true;
            worksheet.Row( row ).Height = 18;
            //worksheet.Range( row , 1 , row , 8 ).Merge( );

            worksheet.Cell( 2 , 1 ).Value = " ";
            worksheet.Cell( 2 , 1 ).Style.Font.FontSize = 15;
            worksheet.Cell( 2 , 1 ).Style.Border.BottomBorder = XLBorderStyleValues.None;
            worksheet.Cell( 2 , 1 ).Style.Border.TopBorder = XLBorderStyleValues.None;
            worksheet.Cell( 2 , 1 ).Style.Border.LeftBorder = XLBorderStyleValues.None;
            worksheet.Cell( 2 , 1 ).Style.Border.RightBorder = XLBorderStyleValues.None;
            worksheet.Cell( 2 , 1 ).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell( 2 , 1 ).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell( 2 , 1 ).Style.Alignment.WrapText = true;
            worksheet.Row( 2 ).Height = 18;
            //worksheet.Range( 2 , 1 , 2 , 8 ).Merge( );

            worksheet.Cell( 3 , 1 ).Value = " ";
            worksheet.Cell( 3 , 1 ).Style.Font.FontSize = 15;
            worksheet.Cell( 3 , 1 ).Style.Border.BottomBorder = XLBorderStyleValues.None;
            worksheet.Cell( 3 , 1 ).Style.Border.TopBorder = XLBorderStyleValues.None;
            worksheet.Cell( 3 , 1 ).Style.Border.LeftBorder = XLBorderStyleValues.None;
            worksheet.Cell( 3 , 1 ).Style.Border.RightBorder = XLBorderStyleValues.None;
            worksheet.Cell( 3 , 1 ).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell( 3 , 1 ).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell( 3 , 1 ).Style.Alignment.WrapText = true;
            worksheet.Row( 3 ).Height = 18;

            #region Intestazioni colonne

            worksheet.Cell( 1 , 1 ).Value = "SEDE GAPP";
            worksheet.Cell( 1 , 1 ).Style.Font.FontSize = 15;
            worksheet.Cell( 1 , 1 ).Style.Border.BottomBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 1 ).Style.Border.TopBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 1 ).Style.Border.LeftBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 1 ).Style.Border.RightBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 1 ).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell( 1 , 1 ).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell( 1 , 1 ).Style.Alignment.WrapText = true;

            worksheet.Cell( 1 , 2 ).Value = "NOMINATIVO";
            worksheet.Cell( 1 , 2 ).Style.Font.FontSize = 15;
            worksheet.Cell( 1 , 2 ).Style.Border.BottomBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 2 ).Style.Border.TopBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 2 ).Style.Border.LeftBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 2 ).Style.Border.RightBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 2 ).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell( 1 , 2 ).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell( 1 , 2 ).Style.Alignment.WrapText = true;

            worksheet.Cell( 1 , 3 ).Value = "ANNO PREC.";
            worksheet.Cell( 1 , 3 ).Style.Font.FontSize = 15;
            worksheet.Cell( 1 , 3 ).Style.Border.BottomBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 3 ).Style.Border.TopBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 3 ).Style.Border.LeftBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 3 ).Style.Border.RightBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 3 ).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell( 1 , 3 ).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell( 1 , 3 ).Style.Alignment.WrapText = true;

            worksheet.Cell( 1 , 4 ).Value = "SPETT.";
            worksheet.Cell( 1 , 4 ).Style.Font.FontSize = 15;
            worksheet.Cell( 1 , 4 ).Style.Border.BottomBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 4 ).Style.Border.TopBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 4 ).Style.Border.LeftBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 4 ).Style.Border.RightBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 4 ).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell( 1 , 4 ).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell( 1 , 4 ).Style.Alignment.WrapText = true;

            worksheet.Cell( 1 , 5 ).Value = "PIAN.";
            worksheet.Cell( 1 , 5 ).Style.Font.FontSize = 15;
            worksheet.Cell( 1 , 5 ).Style.Border.BottomBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 5 ).Style.Border.TopBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 5 ).Style.Border.LeftBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 5 ).Style.Border.RightBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 5 ).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell( 1 , 5 ).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell( 1 , 5 ).Style.Alignment.WrapText = true;
                            
            worksheet.Cell( 1 , 6 ).Value = "FRUITE";
            worksheet.Cell( 1 , 6 ).Style.Font.FontSize = 15;
            worksheet.Cell( 1 , 6 ).Style.Border.BottomBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 6 ).Style.Border.TopBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 6 ).Style.Border.LeftBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 6 ).Style.Border.RightBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 6 ).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell( 1 , 6 ).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell( 1 , 6 ).Style.Alignment.WrapText = true;
                            
            worksheet.Cell( 1 , 7 ).Value = "MIN RICH.";
            worksheet.Cell( 1 , 7 ).Style.Font.FontSize = 15;
            worksheet.Cell( 1 , 7 ).Style.Border.BottomBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 7 ).Style.Border.TopBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 7 ).Style.Border.LeftBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 7 ).Style.Border.RightBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 7 ).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell( 1 , 7 ).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell( 1 , 7 ).Style.Alignment.WrapText = true;
                            
            worksheet.Cell( 1 , 8 ).Value = "RES.";
            worksheet.Cell( 1 , 8 ).Style.Font.FontSize = 15;
            worksheet.Cell( 1 , 8 ).Style.Border.BottomBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 8 ).Style.Border.TopBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 8 ).Style.Border.LeftBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 8 ).Style.Border.RightBorder = XLBorderStyleValues.None;
            worksheet.Cell( 1 , 8 ).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell( 1 , 8 ).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Cell( 1 , 8 ).Style.Alignment.WrapText = true;

            worksheet.Row( 4 ).Height = 18;

            worksheet.Range( 1 , 1 , 4 , 1 ).Merge( );
            worksheet.Range( 1 , 2 , 4 , 2 ).Merge( );
            worksheet.Range( 1 , 3 , 4 , 3 ).Merge( );
            worksheet.Range( 1 , 4 , 4 , 4 ).Merge( );
            worksheet.Range( 1 , 5 , 4 , 5 ).Merge( );
            worksheet.Range( 1 , 6 , 4 , 6 ).Merge( );
            worksheet.Range( 1 , 7 , 4 , 7 ).Merge( );
            worksheet.Range( 1 , 8 , 4 , 8 ).Merge( );

            #endregion

            MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client wcf1 = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client( );
            wcf1.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            MyRaiServiceInterface.MyRaiServiceReference1.GetPianoFerieAnnoResponse resp = null;
            try
            {
                resp = wcf1.GetPianoFerieAnno( sede , anno.GetValueOrDefault( ) , false );
            }
            catch ( Exception ex )
            {
            }

            int colStart = 9;
            int meseColStart = 9;
            int giorniColStart = 9;
            int colEnd = 9;

            // le prime 3 righe servono per l'intestazione della parte destra del foglio
            // riga 1 Mese
            // riga 2 settimana
            // riga 3 giorno

            int mesi = 12;

            if ( model.AlmenoUnUtenteRichiede2021 )
            {
                mesi = 15;
            }

            for ( int mese = 1 ; mese <= mesi ; mese++ )
            {
                int year = anno.GetValueOrDefault( );
                int m = mese;
                if ( m > 12 )
                {
                    m = m - 12;
                    year++;
                }

                DateTime inizioMese = new DateTime( year , m , 1 );
                DateTime fineMese = inizioMese.AddMonths( 1 );

                fineMese = fineMese.AddDays( -1 );
                string descrizioneMese = inizioMese.ToString( "MMMM" , CultureInfo.CreateSpecificCulture( "it" ) );
                descrizioneMese = ( mese > 12 ? "'" : "" ) + descrizioneMese + " " + ( mese > 12 ? " " + year.ToString( ) : "" );
                int giorniDelMese = ( int ) ( fineMese - inizioMese ).TotalDays + 1;

                worksheet.Cell( row , colStart ).Value = descrizioneMese;
                worksheet.Cell( row , colStart ).Style.Font.FontSize = 15;
                worksheet.Cell( row , colStart ).Style.Border.BottomBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , colStart ).Style.Border.TopBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , colStart ).Style.Border.LeftBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , colStart ).Style.Border.RightBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , colStart ).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell( row , colStart ).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell( row , colStart ).Style.Alignment.WrapText = true;
                worksheet.Range( row , colStart , row , ( colStart + ( giorniDelMese - 1 ) ) ).Merge( );

                colStart += giorniDelMese;

                #region Disegno Settimane

                string descrizioneSettimana = "Sett.";

                for ( int day = 1 ; day <= giorniDelMese ; day++ )
                {
                    DateTime giorno = new DateTime( year , m , day );

                    worksheet.Cell( row + 2, giorniColStart ).Value = day;
                    worksheet.Cell( row + 2 , giorniColStart ).Style.Font.FontSize = 15;
                    worksheet.Cell( row + 2 , giorniColStart ).Style.Border.BottomBorder = XLBorderStyleValues.None;
                    worksheet.Cell( row + 2 , giorniColStart ).Style.Border.TopBorder = XLBorderStyleValues.None;
                    worksheet.Cell( row + 2 , giorniColStart ).Style.Border.LeftBorder = XLBorderStyleValues.None;
                    worksheet.Cell( row + 2 , giorniColStart ).Style.Border.RightBorder = XLBorderStyleValues.None;
                    worksheet.Cell( row + 2 , giorniColStart ).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell( row + 2 , giorniColStart ).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell( row + 2 , giorniColStart ).Style.Alignment.WrapText = true;

                    var culture = new System.Globalization.CultureInfo( "it-IT" );
                    var descrizioneGiorno = culture.DateTimeFormat.GetDayName( giorno.DayOfWeek );

                    worksheet.Cell( row + 3 , giorniColStart ).Value = descrizioneGiorno.Substring( 0 , 1 ).ToUpper( );
                    worksheet.Cell( row + 3 , giorniColStart ).Style.Font.FontSize = 15;
                    worksheet.Cell( row + 3 , giorniColStart ).Style.Border.BottomBorder = XLBorderStyleValues.None;
                    worksheet.Cell( row + 3 , giorniColStart ).Style.Border.TopBorder = XLBorderStyleValues.None;
                    worksheet.Cell( row + 3 , giorniColStart ).Style.Border.LeftBorder = XLBorderStyleValues.None;
                    worksheet.Cell( row + 3 , giorniColStart ).Style.Border.RightBorder = XLBorderStyleValues.None;
                    worksheet.Cell( row + 3 , giorniColStart ).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell( row + 3 , giorniColStart ).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell( row + 3 , giorniColStart ).Style.Alignment.WrapText = true;

                    giorniColStart++;

                    // domenica == 0
                    bool isDomenica = ( ( int ) giorno.DayOfWeek == 0 );

                    if (!isDomenica)
                    {
                        colEnd++;
                    }
                    else
                    {
                        descrizioneSettimana = countSettimane + "° Sett.";
                        worksheet.Cell( row + 1 , meseColStart ).Value = descrizioneSettimana;
                        worksheet.Cell( row + 1 , meseColStart ).Style.Font.FontSize = 15;
                        worksheet.Cell( row + 1 , meseColStart ).Style.Border.BottomBorder = XLBorderStyleValues.None;
                        worksheet.Cell( row + 1 , meseColStart ).Style.Border.TopBorder = XLBorderStyleValues.None;
                        worksheet.Cell( row + 1 , meseColStart ).Style.Border.LeftBorder = XLBorderStyleValues.None;
                        worksheet.Cell( row + 1 , meseColStart ).Style.Border.RightBorder = XLBorderStyleValues.None;
                        worksheet.Cell( row + 1 , meseColStart ).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell( row + 1 , meseColStart ).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell( row + 1 , meseColStart ).Style.Alignment.WrapText = true;
                        worksheet.Range( row + 1 , meseColStart , row + 1 , colEnd ).Merge( );
                        meseColStart = colEnd + 1;
                        colEnd++;
                        countSettimane++;
                    }
                }

                #endregion
            }

            #endregion

            #region Corpo foglio

            if ( model == null )
            {
                return null;
            }

            row = 5;        

            foreach ( var dip in model.response.dipendenti )
            {

                DateTime Dcursor = new DateTime( anno.GetValueOrDefault( ) , 1 , 1 );

                var NonInviatoSoloVisGapp = model.PianoFerieDipendenti.Any( x => x.Matricola == dip.matricola.Substring( 1 ) && x.NonInviato );
                var Inviato = model.PianoFerieDipendenti.Any( x => x.Matricola == dip.matricola.Substring( 1 ) && !x.NonInviato );
                var NonInviatoMaSedeConvalidata = model.PianoFerieDipendenti.Any( x => x.Matricola == dip.matricola.Substring( 1 ) && x.NessunInvioPianoFerie );
                var RowFerie = model.PianoFerieDipendenti.Where( x => x.Matricola == dip.matricola.Substring( 1 ) ).FirstOrDefault( );
                var esentato = model.EsentatiPianoFerie.Contains( dip.matricola.Substring( 1 ) );

                worksheet.Row( row ).Height = 18;

                worksheet.Cell( row , 1 ).Value = sede.ToUpper( );
                worksheet.Cell( row , 1 ).Style.Font.FontSize = 15;
                worksheet.Cell( row , 1 ).Style.Border.BottomBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 1 ).Style.Border.TopBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 1 ).Style.Border.LeftBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 1 ).Style.Border.RightBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 1 ).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell( row , 1 ).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell( row , 1 ).Style.Alignment.WrapText = true;

                string descrizioneDipendente = String.Format( "{0} - {1} {2}" , dip.matricola.Substring( 1 ) , dip.cognome , dip.nome );
                worksheet.Cell( row , 2 ).Value = descrizioneDipendente;
                worksheet.Cell( row , 2 ).Style.Font.FontSize = 15;
                worksheet.Cell( row , 2 ).Style.Border.BottomBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 2 ).Style.Border.TopBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 2 ).Style.Border.LeftBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 2 ).Style.Border.RightBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 2 ).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell( row , 2 ).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell( row , 2 ).Style.Alignment.WrapText = true;

                worksheet.Cell( row , 3 ).Value = dip.ferie.ferieAnniPrecedenti.ToString( "F2" );
                worksheet.Cell( row , 3 ).Style.Font.FontSize = 15;
                worksheet.Cell( row , 3 ).Style.Border.BottomBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 3 ).Style.Border.TopBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 3 ).Style.Border.LeftBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 3 ).Style.Border.RightBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 3 ).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell( row , 3 ).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell( row , 3 ).Style.Alignment.WrapText = true;

                worksheet.Cell( row , 4 ).Value = dip.ferie.ferieSpettanti.ToString( "F2" );
                worksheet.Cell( row , 4 ).Style.Font.FontSize = 15;
                worksheet.Cell( row , 4 ).Style.Border.BottomBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 4 ).Style.Border.TopBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 4 ).Style.Border.LeftBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 4 ).Style.Border.RightBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 4 ).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell( row , 4 ).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell( row , 4 ).Style.Alignment.WrapText = true;

                int tot = 0;
                try
                {
                    if ( resp != null )
                    {
                        if ( resp.ListaSedi != null && resp.ListaSedi.Any( ) )
                        {
                            var currSede = resp.ListaSedi.Where( w => w.Sede.Equals( sede ) ).FirstOrDefault( );

                            if ( currSede != null && currSede.Dipendenti != null && currSede.Dipendenti.Any( ) )
                            {
                                var currDip = currSede.Dipendenti.Where( w => w.Matricola.Equals( dip.matricola.Substring( 1 ) ) ).FirstOrDefault( );

                                if ( currDip != null && currDip.Giorni != null && currDip.Giorni.Any( ) )
                                {
                                    tot = currDip.Giorni.Count( w => w.eccezione.Equals( "FE" ) || w.eccezione.Equals( "FE " ) );
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    tot = 0;
                }

                worksheet.Cell( row , 5 ).Value = ( dip.ferie.ferieUsufruite + dip.ferie.feriePianificate + tot ).ToString( "F2" );
                worksheet.Cell( row , 5 ).Style.Font.FontSize = 15;
                worksheet.Cell( row , 5 ).Style.Border.BottomBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 5 ).Style.Border.TopBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 5 ).Style.Border.LeftBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 5 ).Style.Border.RightBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 5 ).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell( row , 5 ).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell( row , 5 ).Style.Alignment.WrapText = true;

                worksheet.Cell( row , 6 ).Value = dip.ferie.ferieUsufruite.ToString( "F2" );
                worksheet.Cell( row , 6 ).Style.Font.FontSize = 15;
                worksheet.Cell( row , 6 ).Style.Border.BottomBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 6 ).Style.Border.TopBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 6 ).Style.Border.LeftBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 6 ).Style.Border.RightBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 6 ).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell( row , 6 ).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell( row , 6 ).Style.Alignment.WrapText = true;

                worksheet.Cell( row , 7 ).Value = dip.ferie.ferieMinime.ToString( "F2" );
                worksheet.Cell( row , 7 ).Style.Font.FontSize = 15;
                worksheet.Cell( row , 7 ).Style.Border.BottomBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 7 ).Style.Border.TopBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 7 ).Style.Border.LeftBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 7 ).Style.Border.RightBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 7 ).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell( row , 7 ).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell( row , 7 ).Style.Alignment.WrapText = true;

                float calc = ( dip.ferie.ferieAnniPrecedenti + dip.ferie.ferieSpettanti ) - ( dip.ferie.ferieUsufruite + tot );

                worksheet.Cell( row , 8 ).Value = calc;
                worksheet.Cell( row , 8 ).Style.Font.FontSize = 15;
                worksheet.Cell( row , 8 ).Style.Border.BottomBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 8 ).Style.Border.TopBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 8 ).Style.Border.LeftBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 8 ).Style.Border.RightBorder = XLBorderStyleValues.None;
                worksheet.Cell( row , 8 ).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell( row , 8 ).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell( row , 8 ).Style.Alignment.WrapText = true;

                if ( Inviato )
                {
                    worksheet.Cell( row , 2 ).Style.Font.Bold = true;
    }

                int cellaDip = 9;


                while ( true )
                {
                    string tipoGiornata = "";
                    try
                    {
                        var giornata = dip.ferie.giornate.Where( w => w.data.Date.Equals( Dcursor.Date ) ).FirstOrDefault( );
                        if (giornata != null)
                        {
                            tipoGiornata = giornata.tipoGiornata;
                        }
                    }
                    catch(Exception ex)
                    {
                        tipoGiornata = "";
                    }
                    
                    bool utenteConAnnoSuccessivo = false;
                    if ( model.AlmenoUnUtenteRichiede2021 )
                    {
                        utenteConAnnoSuccessivo = model.MatricoleCheRichiedonoAnnoSuccessivo.Count( w => w.Equals( dip.matricola.Substring( 1 ) ) ) > 0;
                    }

                    bool SatSun = ( int ) Dcursor.DayOfWeek == 0 || ( int ) Dcursor.DayOfWeek == 6 || ( ( Dcursor.Year > DateTime.Now.Year && model.AlmenoUnUtenteRichiede2021 && !utenteConAnnoSuccessivo ) );

                    bool IsFeriePian = RowFerie != null && ( RowFerie.GiorniFerie.Any( x => x == Dcursor ) || RowFerie.GiorniRR.Any( x => x == Dcursor ) ||
                        RowFerie.GiorniPR.Any( x => x == Dcursor ) ||
                        RowFerie.GiorniPF.Any( x => x == Dcursor ) ||
                        RowFerie.GiorniPX.Any( x => x == Dcursor ) ||
                        RowFerie.GiorniRN.Any( x => x == Dcursor ) );

                    bool HasNote = RowFerie != null && RowFerie.GiorniConNote.Any( x => x == Dcursor );
                    bool IsFE_GAPP = RowFerie != null && RowFerie.Gapp_FE.Contains( Dcursor );
                    bool IsPF_GAPP = RowFerie != null && RowFerie.Gapp_PF.Contains( Dcursor );
                    bool IsPR_GAPP = RowFerie != null && RowFerie.Gapp_PR.Contains( Dcursor );

                    // se SatSun allora giorno in grigio
                    
                    worksheet.Cell( row , cellaDip ).Style.Font.FontSize = 15;
                    worksheet.Cell( row , cellaDip ).Style.Border.BottomBorder = XLBorderStyleValues.None;
                    worksheet.Cell( row , cellaDip ).Style.Border.TopBorder = XLBorderStyleValues.None;
                    worksheet.Cell( row , cellaDip ).Style.Border.LeftBorder = XLBorderStyleValues.None;
                    worksheet.Cell( row , cellaDip ).Style.Border.RightBorder = XLBorderStyleValues.None;
                    worksheet.Cell( row , cellaDip ).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell( row , cellaDip ).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell( row , cellaDip ).Style.Alignment.WrapText = true;
                    worksheet.Row( row ).Height = 18;

                    bool haScritto = false;

                    worksheet.Cell( row , cellaDip ).Value = descrizioneDipendente;
                    if ( !IsFeriePian )
                    {                        
                        if ( IsFE_GAPP )
                        {
                            worksheet.Cell( row , cellaDip ).Value = "FE";
                            worksheet.Cell( row , cellaDip ).Style.Fill.BackgroundColor = XLColor.FromArgb( 72 , 191 , 255 );
                            haScritto = true;
                        }
                        else if (IsPF_GAPP)
                        {
                            worksheet.Cell( row , cellaDip ).Style.Fill.BackgroundColor = XLColor.FromArgb( 29 , 146 , 245 );
                            worksheet.Cell( row , cellaDip ).Value = "PF";
                            haScritto = true;
                        }
                        else if (IsPR_GAPP)
                        {
                            worksheet.Cell( row , cellaDip ).Style.Fill.BackgroundColor = XLColor.FromArgb( 52 , 49 , 164 );
                            worksheet.Cell( row , cellaDip ).Value = "PR";
                            haScritto = true;
                        }
                        else
                        {
                            worksheet.Cell( row , cellaDip ).Value = " ";
                        }
                    }
                    else
                    {
                        bool appr = RowFerie != null && RowFerie.DataApprovazione != null;
                        bool rr = RowFerie.GiorniRR.Any( x => x == Dcursor );
                        bool rn = RowFerie.GiorniRN.Any( x => x == Dcursor );
                        bool pf = RowFerie.GiorniPF.Any( x => x == Dcursor );
                        bool pr = RowFerie.GiorniPR.Any( x => x == Dcursor );
                        bool px = RowFerie.GiorniPX.Any( x => x == Dcursor );

                        if (rr)
                        {
                            worksheet.Cell( row , cellaDip ).Style.Font.FontColor = XLColor.FromArgb( 72 , 191 , 255 );
                            worksheet.Cell( row , cellaDip ).Value = "RR";
                            haScritto = true;
                        }
                        else if ( rn )
                        {
                            worksheet.Cell( row , cellaDip ).Style.Font.FontColor = XLColor.FromArgb( 72 , 191 , 255 );
                            worksheet.Cell( row , cellaDip ).Value = "RN";
                            haScritto = true;
                        }
                        else
                        {
                            worksheet.Cell( row , cellaDip ).Style.Font.FontColor = XLColor.FromArgb( 72 , 191 , 255 );
                            worksheet.Cell( row , cellaDip ).Value = "FE";
                            haScritto = true;
                        }
                        
                        if ( pf )
                        {
                            worksheet.Cell( row , cellaDip ).Style.Font.FontColor = XLColor.FromArgb( 0 , 94 , 179 );
                            worksheet.Cell( row , cellaDip ).Value = "PF";
                            haScritto = true;
                        }
                        if ( pr )
                        {
                            worksheet.Cell( row , cellaDip ).Style.Font.FontColor = XLColor.FromArgb( 0 , 94 , 179 );
                            worksheet.Cell( row , cellaDip ).Value = "PR";
                            haScritto = true;
                        }
                        if ( px )
                        {
                            worksheet.Cell( row , cellaDip ).Style.Font.FontColor = XLColor.FromArgb( 0 , 94 , 179 );
                            worksheet.Cell( row , cellaDip ).Value = "PX";
                            haScritto = true;
                        }
                    }

                    if ( (SatSun && !haScritto) || tipoGiornata == "A" )
                    {
                        worksheet.Cell( row , cellaDip ).Style.Fill.BackgroundColor = XLColor.FromArgb( 174 , 198 , 207 );
                    }

                    if ( model.AlmenoUnUtenteRichiede2021 )
                    {
                        DateTime dataLimite = new DateTime( DateTime.Now.Year + 1 , 3 , 31 );
                        if ( Dcursor.Date >= dataLimite.Date )
                        {
                            break;
                        }
                    }
                    else
                    {
                    if ( Dcursor.Month == 12 && Dcursor.Day == 31 )
                    {
                        break;
                    }
                    }
                    Dcursor = Dcursor.AddDays( 1 );

                    cellaDip++;
                }
                row++;
            }

            worksheet.Range( 5 , 1 , row - 1 , 1 ).Merge( );
            #endregion

            for ( int riga = 1 ; riga < row ; riga++ )
            {
                for ( int colonna = 1 ; colonna < defaultCell ; colonna++ )
                {
                    worksheet.Cell( riga , colonna ).Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                    worksheet.Cell( riga , colonna ).Style.Border.TopBorder = XLBorderStyleValues.Medium;
                    worksheet.Cell( riga , colonna ).Style.Border.LeftBorder = XLBorderStyleValues.Medium;
                    worksheet.Cell( riga , colonna ).Style.Border.RightBorder = XLBorderStyleValues.Medium;

                    if ( colonna == 1 )
                    {
                        worksheet.Column( colonna ).Width = 11;
                    }

                    worksheet.Column( colonna ).AdjustToContents( );

                    if ( colonna >= 3 && colonna < 9 )
                    {
                        worksheet.Column( colonna ).Width = 9;
                    }

                    if (colonna > 8)
                    {
                        worksheet.Column( colonna ).Width = 9;
                    }

                    if ( riga >= 5 && colonna > 2 && colonna < 9 )
                    {
                        worksheet.Cell( riga , colonna ).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    }

                    worksheet.Cell( riga , colonna ).Style.Font.FontSize = 11;

                    if ( riga <= 4 )
                    {
                        worksheet.Cell( riga , colonna ).Style.Font.Bold = true;
                        worksheet.Cell( row , colonna ).Style.Fill.BackgroundColor = XLColor.FromArgb( 240 , 240 , 240 );
                    }

                }
            }

            workbook.SaveAs( ms );
            ms.Position = 0;

            string nomeFile = String.Format( "ExportPianoFerieSedeGapp_{0}" , sede );
            return new FileStreamResult( ms , "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" ) { FileDownloadName = nomeFile + ".xlsx" };
        }
    }

    public class PianoFerieControllerScope : SessionScope<PianoFerieControllerScope>
    {
        public PianoFerieControllerScope ( )
        {
        }

        private DateTime? LastUpdate { get; set; }

        public PianoFerieVM ScopeModel
        {
            get
            {
                DateTime current = DateTime.Now;

                if ( LastUpdate.HasValue )
                {
                    TimeSpan t1 = current.Subtract( LastUpdate.Value );
                    double minuti = t1.TotalMinutes;

                    if ( minuti > 5 )
                    {
                        // se son passati più di 5 minuti azzera lo scope
                        this._model = null;
                    }
                }

                return this._model;
            }
            set
            {
                // salva la data dell'ultimo set dello scope
                this.LastUpdate = DateTime.Now;
                this._model = value;
            }
        }

        private PianoFerieVM _model = null;
    }
}