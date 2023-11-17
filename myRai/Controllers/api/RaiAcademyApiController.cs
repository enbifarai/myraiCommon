using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using myRaiData;
using myRaiDataTalentia;
using System.Web.Http.Filters;
using myRai.Controllers.RaiAcademy;
using Newtonsoft.Json;
using myRaiHelper;
using myRai.Data.CurriculumVitae;
using myRaiCommonModel.DataControllers.RaiAcademy;
using myRaiCommonManager;

namespace myRai.Controllers.api
{
    public class RequiredPassword : ActionFilterAttribute
    {
        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            var db = new digiGappEntities();

            var AccessiApi = db.MyRai_AccessoAPI.ToList();
            string action = actionContext.ActionDescriptor.ActionName.ToLower();

            var keyString = actionContext.ControllerContext.Request.Headers.Where(x => x.Key.ToLower() == "keystring").FirstOrDefault();

            if (keyString.Key == null)
            {
                var Authorized = AccessiApi
                        .Where(x => String.IsNullOrWhiteSpace(x.KeyString)
                                     &&
                                     (String.IsNullOrWhiteSpace(x.ActionPermesse) || x.ActionPermesse.ToLower().Contains(action))
                                     &&
                                    (String.IsNullOrWhiteSpace(x.ActionNegate) || !x.ActionNegate.Split(',').Contains(action)))
                        .ToList().Count > 0;
                LogAPIcall(action, null, Authorized, actionContext);
                if (Authorized)
                {
                    base.OnActionExecuting(actionContext);
                }
                else
                {
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent("NON AUTORIZZATO") };
                }
            }
            else
            {
                string keyStringValue = keyString.Value.FirstOrDefault();
                if (keyStringValue != null)
                {
                    var Authorized = AccessiApi
                      .Where(x => x.KeyString.ToLower() == keyStringValue.ToLower()
                                   &&
                                   (String.IsNullOrWhiteSpace(x.ActionPermesse) || x.ActionPermesse.ToLower().Contains(action))
                                   &&
                                  (String.IsNullOrWhiteSpace(x.ActionNegate) || !x.ActionNegate.Split(',').Contains(action)))
                      .ToList().Count > 0;
                    LogAPIcall(action, keyStringValue, Authorized, actionContext);
                    if (Authorized)
                    {
                        base.OnActionExecuting(actionContext);
                    }
                    else
                    {
                        actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent("NON AUTORIZZATO") };
                    }
                    return;
                }
                else
                {
                    LogAPIcall(action, keyStringValue, false, actionContext);
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent("NON AUTORIZZATO") };
                }
            }
        }

        public void LogAPIcall(string action, string keyString, Boolean success, System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            var db = new digiGappEntities();
            try
            {
                MyRai_LogAPI a = new MyRai_LogAPI()
                {
                    API = action,
                    Data = DateTime.Now,
                    KeyString = keyString,
                    Parametri = System.Web.HttpContext.Current.Request.Url.ToString(),
                    IP = System.Web.HttpContext.Current.Request.UserHostAddress,
                    UserAgent = System.Web.HttpContext.Current.Request.UserAgent,
                    Authorized = success
                };
                db.MyRai_LogAPI.Add(a);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "API",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    provenienza = "LogAPIcall"
                });
            }


        }
    }
    [RequiredPassword]
    public class RaiAcademyApiController : ApiController
    {
        Boolean esito = false;
        string strMex = "";
        string baseUrl = System.Web.HttpContext.Current.Request.Url.Scheme + "://" + System.Web.HttpContext.Current.Request.Url.Authority;
        /// <summary>
        /// Carica il catalogo dei corsi per un utente
        /// </summary>
        /// <returns></returns>
        public GetCatalogoResponse getCorsi(string matricola)
        {
            var db = new TalentiaEntities();

            //HttpContext.Current.Request.Url.Authority;
            try
            {

                var query = from corsi in db.CORSO
                            join tema in db.TB_TEMA on corsi.ID_TEMA equals tema.ID_TEMA
                            join tpcorso in db.TB_TPCORSO on corsi.ID_TPCORSO equals tpcorso.ID_TPCORSO
                            join metodo in db.TB_MTDDID on corsi.ID_MTDDID equals metodo.ID_MTDDID
                            join edition in db.EDIZIONE on corsi.ID_CORSO equals edition.ID_CORSO
                            join iscrizioni in db.CURRFORM on edition.ID_EDIZIONE equals iscrizioni.ID_EDIZIONE
                            join anag in db.V_XR_XANAGRA on iscrizioni.ID_PERSONA equals anag.ID_PERSONA
                            where
                            corsi.IND_ATTIVO == "Y" & corsi.IND_TUTORSHIP == "Y"
                            & anag.MATRICOLA == matricola
                            select new MainCatalog
                            {
                                idCorso = corsi.ID_CORSO,
                                urlFotoCorso = "",
                                indObbligatorio = corsi.IND_OBBLIGATORIO,
                                //nomeCategoria = tema.COD_TEMA.Substring(7, 2000),
                                nomeCategoria = tema.COD_TEMA,
                                urlCategoria = "",
                                idCategoria = tema.ID_TEMA,
                                metodoDidattico = metodo.COD_METODODID,
                                dataInizioCorso = edition.DTA_INIZIO,
                                iconaTag = "",
                                labelTag = "",
                                nomeCorso = corsi.COD_CORSO,
                                // tematicaCorso = tpcorso.COD_TIPOCORSO.Substring(7, 2000),
                                tematicaCorso = tpcorso.COD_TIPOCORSO,
                                urlCorso = "",
                                pubblicato = corsi.IND_TUTORSHIP,
                                idIscrizione = iscrizioni.ID_CURRFORM,
                                idPersona = anag.ID_PERSONA,
                                matricola = anag.MATRICOLA,
                            };



                //CatalogoCorsi catalogo = new CatalogoCorsi();
                List<Categoria> listaCategorie = new List<Categoria>();
                //catalogo.categorie = new List<Categoria>();
                //  catalogo.urlCatalogoCorsi = "-";
                //List<FakeTableUtentiCorsi> corsiUtente = getCorsiUtente(matricola);
                Categoria cat = new Categoria();
                foreach (var item in query.GroupBy(x => x.idCategoria))
                {
                    cat = new Categoria();
                    cat.idCategoria = item.Key;
                    cat.nomeCategoria = RimuoviCodice(item.ElementAt(0).nomeCategoria).UpperFirst();
                    cat.urlCategoria = baseUrl + "/raiacademy?area=" + cat.idCategoria.ToString();
                    cat.corsi = new List<Corso>();

                    foreach (var corso in item)
                    {
                        if (cat.corsi.Find(f => f.idCorso == corso.idCorso) == null)
                        {
                            cat.corsi.Add(new Corso
                            {
                                idCorso = corso.idCorso,
                                data = corso.dataInizioCorso.ToString("dd-MM-yyyy"),
                                metodoDidattico = corso.metodoDidattico,
                                nomeCorso = corso.nomeCorso,
                                tags = new List<Tag>(),
                                tematica = RimuoviCodice(corso.tematicaCorso),
                                tipoOfferta = getTipoOffertaStr(corso.indObbligatorio),
                                tipoOffertaEnum = getTipoOfferta(corso.indObbligatorio),
                                url = baseUrl + "/raiacademy/dettagliocorso?idCorso=" + corso.idCorso.ToString(),
                                url_foto = corso.urlFotoCorso,
                                idCategoria = corso.idCategoria,
                                nomeCategoriaRif = RimuoviCodice(item.ElementAt(0).nomeCategoria),
                                idTipoOfferta = corso.indObbligatorio
                            });
                        }
                    }
                }
                #region vecchio sistema
                //foreach (var item in query.GroupBy(x => x.idCategoria))
                //{
                //    cat = new Categoria();
                //    cat.idCategoria = item.Key;
                //    cat.nomeCategoria = RimuoviCodice(item.ElementAt(0).nomeCategoria);
                //    cat.urlCategoria = item.ElementAt(0).urlCategoria;

                //    cat.corsi = new List<Corso>();
                //    foreach (var corso in item)
                //    {
                //        if (corsiUtente.Find(f => f.idCorso == corso.idCorso) != null)
                //        {
                //            if (cat.corsi.Find(f => f.idCorso == corso.idCorso) == null)
                //            {
                //                cat.corsi.Add(new Corso
                //                {
                //                    idCorso = corso.idCorso,
                //                    data = corso.dataInizioCorso.ToString("dd-MM-yyyy"),
                //                    metodoDidattico = corso.metodoDidattico,
                //                    nomeCorso = corso.nomeCorso,
                //                    tags = new List<Tag>(),
                //                    tematica = RimuoviCodice(corso.tematicaCorso),
                //                    tipoOfferta = getTipoOfferta(corso.indObbligatorio),
                //                    url = corso.urlCorso,
                //                    url_foto = corso.urlFotoCorso,
                //                    idCategoria = corso.idCategoria,
                //                    nomeCategoriaRif = RimuoviCodice(item.ElementAt(0).nomeCategoria),
                //                    idTipoOfferta = corso.indObbligatorio

                //                });
                //            }
                //        }
                //    }
                #endregion
                if (cat.corsi != null)
                {
                    cat.corsi = cat.corsi.OrderByDescending(f => dataOrder(f.data)).ToList();

                    if (cat.corsi.Count > 0)
                        listaCategorie.Add(cat);
                }
                cat = null;
                cat = getCorsiRAICV(matricola);
                if (cat != null)
                    listaCategorie.Add(cat);

                cat = null;
                cat = getCorsiExtraRaiCV(matricola);
                if (cat != null)
                    listaCategorie.Add(cat);

                if (listaCategorie.Count == 0)
                    return new GetCatalogoResponse { success = false, error = "Non ci sono corsi per l'utente selezionato", Categorie = null, urlCatalogoCorsi = "" };
                return new GetCatalogoResponse()
                {
                    success = true,
                    error = null,
                    urlCatalogoCorsi = baseUrl + "/raiacademy",
                    Categorie = listaCategorie
                };
            }
            catch (Exception ex)
            {
                return new GetCatalogoResponse()
                {
                    success = false,
                    error = ex.Message,
                    urlCatalogoCorsi = "",
                    Categorie = null
                };
            }
        }

        /// <summary>
        /// Carica il catalogo dei corsi
        /// </summary>
        /// <returns></returns>
        public GetCatalogoResponse getCatalogo(int maxCat=0)
        {
            var db = new TalentiaEntities();
            try
            {

                var lista = db.CORSO.Include("TB_TEMA").Include("TB_TPCORSO").Include("TB_MTDDID").Include("CZNDOCS").Include("EDIZIONE")
                            .Where(x => x.IND_ATTIVO == "Y" && x.IND_TUTORSHIP == "Y")//x.IND_DACONF=="Y")
                            .GroupBy(y => y.TB_TEMA);

                if (lista.Count() > 0)
                {
                    List<Categoria> listaCategorie = new List<Categoria>();
                    Categoria cat = null;

                    Random rnd = null;

                    foreach (var item in lista)
                    {
                        cat = new Categoria();
                        cat.idCategoria = item.Key.ID_TEMA;
                        cat.nomeCategoria = item.Key.COD_TEMA.UpperFirst();
                        cat.urlCategoria = baseUrl + "/raiacademy?area=" + item.Key.ID_TEMA.ToString();
                        cat.tematiche = new List<Tematica>();
                        cat.tematiche.AddRange(
                            item.GroupBy(x => new { a = x.TB_TPCORSO.ID_TPCORSO, b = x.TB_TPCORSO.COD_TIPOCORSO })
                            .Select(y => new Tematica() { idTematica = y.Key.a, codTematica = y.Key.b, url=baseUrl+"/raiacademy?area="+item.Key.ID_TEMA.ToString()+"&tema="+y.Key.a })
                        );
                        cat.corsi = new List<Corso>();

                        int count = 0;

                        List<int> usedI = new List<int>();
                        int itemCount = item.Count();
                        int maxI = maxCat>0? maxCat : itemCount;
                        if (maxCat > 0)
                            rnd = new Random();

                        for (int i = 0; i < maxI; i++)
                        {
                            int index = 0;
                            if (maxCat > 0)
                            {
                                do index = rnd.Next(0, itemCount);
                                while (usedI.Contains(index));
                                usedI.Add(index);
                            }
                            else
                            {
                                index = i;
                            } 
                            
                            var corso = item.ElementAt(index);

                            var tmp = corso.EDIZIONE.Where(x => x.DTA_INIZIO >= DateTime.Now || x.DTA_FINE >= DateTime.Now).OrderBy(y => y.DTA_INIZIO).Distinct();

                            var firstAvailable = tmp.FirstOrDefault(x => x.DTA_INIZIO >= DateTime.Now);

                            string dataDispo = firstAvailable != null ? firstAvailable.DTA_INIZIO.ToString("dd-MM-yyyy") : null;
                            bool corsoOnline = corso.TB_MTDDID.DES_METODODID == "FAD";

                            cat.corsi.Add(new Corso
                            {
                                idCorso = corso.ID_CORSO,
                                data = corsoOnline ?null:dataDispo,
                                metodoDidattico = corso.TB_MTDDID.COD_METODODID,
                                nomeCorso = corso.COD_CORSO,
                                tags = new List<Tag>(){
                                        new Tag(){icona="fas fa-suitcase",label=corso.TB_MTDDID.COD_METODODID},
                                        new Tag(){icona="fas fa-tag",label=getTipoOffertaStr(corso.IND_OBBLIGATORIO)}
                                    },
                                tematica = corso.TB_TPCORSO.COD_TIPOCORSO,
                                idTipoOfferta = corso.IND_OBBLIGATORIO,
                                tipoOfferta = getTipoOffertaStr(corso.IND_OBBLIGATORIO),
                                tipoOffertaEnum = getTipoOfferta(corso.IND_OBBLIGATORIO),
                                url = baseUrl + "/raiacademy/dettagliocorso?idCorso=" + corso.ID_CORSO.ToString(),
                                url_foto = baseUrl+"/api/raiacademyapi/GetCourseImage?idCorso="+corso.ID_CORSO.ToString(),//  corso.CZNDOCS.Count() > 0 ? "data:image/jpeg;base64," + Convert.ToBase64String(corso.CZNDOCS.First().OBJ_OBJECT) : null,
                                idCategoria = item.Key.ID_TEMA,
                                nomeCategoriaRif = item.Key.COD_TEMA.UpperFirst(),
                                label_immagine = corsoOnline ? "Corso online" : (String.IsNullOrWhiteSpace(dataDispo) ? "Data da definire" : null),
                                label_immagine_colore = corsoOnline ? "#5fc645" : (String.IsNullOrWhiteSpace(dataDispo) ? "#d4761c" : null),
                                label_immagine_icona = corsoOnline ? "fas fa-desktop" : null
                            });
                            count++;
                        }

                        //foreach (var corso in item.TakeWhile(x=>maxCat==0 || count<maxCat))
                        //{
                        //    var tmp = corso.EDIZIONE.Where(x => x.DTA_INIZIO >= DateTime.Now || x.DTA_FINE >= DateTime.Now).OrderBy(y => y.DTA_INIZIO).Distinct();

                        //    string dataDispo = tmp != null && tmp.Count()>0 ? tmp.First().DTA_INIZIO.ToString("dd-MM-yyyy") : null;
                        //    bool corsoOnline = corso.TB_MTDDID.DES_METODODID == "FAD";

                        //    cat.corsi.Add(new Corso
                        //        {
                        //            idCorso = corso.ID_CORSO,
                        //            data = dataDispo,
                        //            metodoDidattico = corso.TB_MTDDID.COD_METODODID,
                        //            nomeCorso = corso.COD_CORSO,
                        //            tags = new List<Tag>(){
                        //                new Tag(){icona="fas fa-suitcase",label=corso.TB_MTDDID.COD_METODODID},
                        //                new Tag(){icona="fas fa-tag",label=getTipoOffertaStr(corso.IND_OBBLIGATORIO)}
                        //            },
                        //            tematica = corso.TB_TPCORSO.COD_TIPOCORSO,
                        //            idTipoOfferta = corso.IND_OBBLIGATORIO,
                        //            tipoOfferta = getTipoOffertaStr(corso.IND_OBBLIGATORIO),
                        //            tipoOffertaEnum = getTipoOfferta(corso.IND_OBBLIGATORIO),
                        //            url = baseUrl + "/raiacademy/dettagliocorso?idCorso=" + corso.ID_CORSO.ToString(),
                        //            url_foto = corso.CZNDOCS.Count() > 0 ? "data:image/jpeg;base64," + Convert.ToBase64String(corso.CZNDOCS.First().OBJ_OBJECT) : null,
                        //            idCategoria = item.Key.ID_TEMA,
                        //            nomeCategoriaRif = item.Key.COD_TEMA.UpperFirst(),
                        //            label_immagine = corsoOnline?"Corso online":(String.IsNullOrWhiteSpace(dataDispo)?"Data da definire":null),
                        //            label_immagine_colore = corsoOnline ? "#5fc645" : (String.IsNullOrWhiteSpace(dataDispo) ? "#d4761c" : null),
                        //            label_immagine_icona = corsoOnline?"fas fa-desktop":null
                        //        });
                        //    count++;
                        //}
                        listaCategorie.Add(cat);
                    }

                    if (lista.Any(x => x.Any(y => y.ID_DOMITEMGRUPPO.HasValue && y.ID_DOMITEMGRUPPO.Value > 0)))
                    {
                        string paramTemAgg = CommonHelper.GetParametro<string>(EnumParametriSistema.AcademyTematicheAggiuntive);
                        var temiAgg = paramTemAgg.Split(';').Select(x => x.Split('|')).Select(y => new { area = y[0], gruppo = y[1] });
                        foreach (var temaAgg in temiAgg)
                        {
                            listaCategorie.First(x => x.nomeCategoria.ToUpper() == temaAgg.area).tematiche.Add(
                                new Tematica() {
                                    idTematica=0,
                                    codTematica=temaAgg.gruppo,
                                    url = baseUrl + "/raiacademy?gruppo=" + temaAgg.gruppo
                                });
                        }
                    }

                    return new GetCatalogoResponse()
                    {
                        success = true,
                        error = null,
                        urlCatalogoCorsi = baseUrl + "/raiacademy",
                        Categorie = listaCategorie
                    };
                }
                else
                {
                    return new GetCatalogoResponse { success = false, error = "Non ci sono corsi disponibili", Categorie = null, urlCatalogoCorsi = "" };
                }
            }
            catch (Exception ex)
            {
                return new GetCatalogoResponse()
                {
                    success = false,
                    error = ex.Message,
                    urlCatalogoCorsi = "",
                    Categorie = null
                };
            }
        }


        public HttpResponseMessage GetCourseImage(int idCorso)
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            //response.Content = new StreamContent(RaiAcademyDataController.GetCourseImage(idCorso));
            response.Content = new ByteArrayContent(RaiAcademyDataController.GetCourseImageResized(idCorso));
            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            return response;
        }

        private Categoria getCorsiRAICV(string matricola)
        {

            cv_ModelEntities cvEnt = new cv_ModelEntities();
            List<V_CVCorsiRai> lista = new List<V_CVCorsiRai>();
            //lista = cvEnt.V_CVCorsiRai.Where(x => x.matricola == matricola && x.flagImage==0).ToList();
            lista = RaiAcademyManager.GetCorsiFatti(matricola).Where(x => x.flagImage == 0).ToList();
            Categoria cat = new Categoria();

            if (lista.Count > 0)
            {
                cat.idCategoria = -1;
                cat.nomeCategoria = "Corsi RAI pregressi";
                cat.urlCategoria = "";
                cat.corsi = new List<Corso>();
                foreach (var item in lista)
                {
                    cat.corsi.Add(new Corso
                    {
                        idCorso = -1,
                        data = CommonHelper.ConvertToDate(item.DataInizio).ToString("dd-MM-yyyy"),
                        metodoDidattico = "CORSO IN AULA",
                        nomeCorso = item.TitoloCorso,
                        tags = new List<Tag>(),
                        tipoOffertaEnum = TipoOfferta.aperta,
                        url = "",
                        url_foto = "",
                        idCategoria = -1,
                        nomeCategoriaRif = "Corsi RAI Pregressi",
                        idTipoOfferta = "2",
                        tematica = "ND",
                        data_completamento = CommonHelper.ConvertToDate(item.DataFine).ToString("dd-MM-yyyy"),
                        data_end = CommonHelper.ConvertToDate(item.DataFine)
                    });

                }
                cat.corsi = cat.corsi.OrderByDescending(f => dataOrder(f.data)).ToList();
            }
            else
                return null;
            return cat;
        }
        private Categoria getCorsiExtraRaiCV(string matricola)
        {
            Categoria cat = new Categoria();
            cv_ModelEntities cvEnt = new cv_ModelEntities();
            List<TCVFormExRai> corsi = new List<TCVFormExRai>();
            corsi = cvEnt.TCVFormExRai.Where(x => x.Matricola == matricola).ToList();
            if (corsi.Count > 0)
            {
                cat.idCategoria = -2;
                cat.nomeCategoria = "Corsi Extra RAI pregressi";
                cat.urlCategoria = "";
                cat.corsi = new List<Corso>();
                foreach (var item in corsi)
                {
                    cat.corsi.Add(new Corso
                    {
                        idCorso = -2,
                        data = item.DataOraAgg != null ? item.DataOraAgg.Value.ToString("dd-MM-yyyy") : null,
                        metodoDidattico = "CORSO IN AULA",
                        nomeCorso = item.Corso,
                        tags = new List<Tag>(),
                        tematica = "ND",
                        tipoOffertaEnum = TipoOfferta.suRichiesta,
                        url = "",
                        url_foto = "",
                        idCategoria = -2,
                        nomeCategoriaRif = "Corsi Extra RAI pregressi",
                        idTipoOfferta = "1",
                        data_end = item.DataOraAgg,
                    });
                }
                cat.corsi = cat.corsi.OrderByDescending(f => dataOrder(f.data)).ToList();
            }
            else
                return null;
            return cat;
        }

        private string dataOrder(string strData)
        {
            string res = "";
            //dd-MM-yyy
            if (strData != "")
            {
                string[] dataArr = strData.Split('-');
                res = dataArr[2] + dataArr[1] + dataArr[0];
            }
            return res;
        }
        /// <summary>
        /// Torna la lista dei corsi per una determinata categoria
        /// </summary>
        /// <param name="idCategoria">id della categoria</param>
        /// <returns></returns>
        private GetCategoriaResponse getCategoriaCorsi(int idCategoria)
        {
            var db = new TalentiaEntities();
            try
            {

                var query = from corsi in db.CORSO
                            join tema in db.TB_TEMA on corsi.ID_TEMA equals tema.ID_TEMA
                            join tpcorso in db.TB_TPCORSO on corsi.ID_TPCORSO equals tpcorso.ID_TPCORSO
                            join metodo in db.TB_MTDDID on corsi.ID_MTDDID equals metodo.ID_MTDDID
                            join edition in db.EDIZIONE on corsi.ID_CORSO equals edition.ID_CORSO
                            where corsi.IND_ATTIVO == "Y" && tema.ID_TEMA == idCategoria
                            select new MainCatalog
                            {
                                idCorso = corsi.ID_CORSO,
                                urlFotoCorso = "",
                                indObbligatorio = corsi.IND_OBBLIGATORIO,
                                //nomeCategoria = tema.COD_TEMA.Substring(7, 2000),
                                nomeCategoria = tema.COD_TEMA,
                                urlCategoria = "",
                                idCategoria = tema.ID_TEMA,
                                metodoDidattico = metodo.COD_METODODID,
                                dataInizioCorso = edition.DTA_INIZIO,
                                iconaTag = "",
                                labelTag = "",
                                nomeCorso = corsi.COD_CORSO,
                                //tematicaCorso = tpcorso.COD_TIPOCORSO.Substring(7, 2000),
                                tematicaCorso = tpcorso.COD_TIPOCORSO,
                                urlCorso = ""
                            };

                Categoria cat = new Categoria();
                List<MainCatalog> corsiCat = new List<MainCatalog>();
                corsiCat = query.ToList();
                Corso corsoTemp = new Corso();
                if (corsiCat.Count > 0)
                {
                    cat.idCategoria = corsiCat[0].idCategoria;
                    cat.nomeCategoria = RimuoviCodice(corsiCat[0].nomeCategoria).UpperFirst();
                    cat.urlCategoria = corsiCat[0].urlCategoria;

                    cat.corsi = new List<Corso>();
                    foreach (var corso in query.ToList())
                    {
                        corsoTemp = new Corso();
                        corsoTemp = fromMainCat(corso);
                        cat.corsi.Add(corsoTemp);

                        //cat.corsi.Add(new Corso
                        //{
                        //    idCorso = corso.idCorso,
                        //    data = corso.dataInizioCorso.ToString("dd-MM-yyyy"),
                        //    idCategoria = corso.idCategoria,
                        //    metodoDidattico = corso.metodoDidattico,
                        //    nomeCorso = corso.nomeCorso,
                        //    tagsCorso = new List<Tag>(),
                        //    tematica = RimuoviCodice(corso.tematicaCorso),
                        //    tipoOfferta = getTipoOfferta(corso.indObbligatorio),
                        //    url = corso.urlCorso,
                        //    urlFotoCorso = corso.urlFotoCorso,
                        //    idTipoOfferta = corso.indObbligatorio,
                        //    nomeCategoriaRif = RimuoviCodice(corsiCat[0].nomeCategoria)
                        //});
                    }
                }
                else
                    return new GetCategoriaResponse { success = false, error = "Categoria non contenente corsi o inesistente", categoria = null };
                return new GetCategoriaResponse()
                {
                    success = true,
                    error = null,
                    categoria = cat
                };
            }
            catch (Exception ex)
            {
                return new GetCategoriaResponse()
                {
                    success = false,
                    error = ex.Message,
                    categoria = null
                };
            }
        }
        /// <summary>
        /// torna il dettaglio del corso
        /// </summary>
        /// <param name="idCorso">id del corso</param>
        /// <returns></returns>
        private GetCorsoResponse getCorso(int idCorso)
        {
            var db = new TalentiaEntities();
            try
            {
                Corso corsoRet = new Corso();

                var query = from corsi in db.CORSO
                            join tema in db.TB_TEMA on corsi.ID_TEMA equals tema.ID_TEMA
                            join tpcorso in db.TB_TPCORSO on corsi.ID_TPCORSO equals tpcorso.ID_TPCORSO
                            join metodo in db.TB_MTDDID on corsi.ID_MTDDID equals metodo.ID_MTDDID
                            join edition in db.EDIZIONE on corsi.ID_CORSO equals edition.ID_CORSO
                            where corsi.ID_CORSO == idCorso && corsi.IND_ATTIVO == "Y"
                            select new MainCatalog
                            {
                                idCorso = corsi.ID_CORSO,
                                urlFotoCorso = "",
                                indObbligatorio = corsi.IND_OBBLIGATORIO,
                                //nomeCategoria = tema.COD_TEMA.Substring(7, 2000),
                                nomeCategoria = tema.COD_TEMA,
                                urlCategoria = "",
                                idCategoria = tema.ID_TEMA,
                                metodoDidattico = metodo.COD_METODODID,
                                dataInizioCorso = edition.DTA_INIZIO,
                                iconaTag = "",
                                labelTag = "",
                                nomeCorso = corsi.COD_CORSO,
                                // tematicaCorso = tpcorso.COD_TIPOCORSO.Substring(7, 2000),
                                tematicaCorso = tpcorso.COD_TIPOCORSO,
                                urlCorso = ""
                            };
                //var query = from corsi in db.CORSO
                //            join tema in db.TB_TEMA on corsi.ID_TEMA equals tema.ID_TEMA
                //            join tpcorso in db.TB_TPCORSO on corsi.ID_TPCORSO equals tpcorso.ID_TPCORSO
                //            join metodo in db.TB_MTDDID on corsi.ID_MTDDID equals metodo.ID_MTDDID
                //            join edition in db.EDIZIONE on corsi.ID_CORSO equals edition.ID_CORSO
                //            where corsi.ID_CORSO == idCorso && corsi.IND_ATTIVO == "Y"
                //            select new Corso
                //            {
                //                idCorso = corsi.ID_CORSO,
                //                urlFotoCorso = "",
                //                //tipoOfferta = getTipoOfferta(corsi.IND_OBBLIGATORIO),
                //                //  tagsCorso = new List<Tag>(),
                //                idTipoOfferta = corsi.IND_OBBLIGATORIO,
                //                idCategoria = tema.ID_TEMA,
                //                metodoDidattico = metodo.COD_METODODID,
                //                data = edition.DTA_INIZIO.ToString("dd-mm-yyyy"),
                //                //nomeCategoriaRif = tema.COD_TEMA.Substring(7, 2000),
                //                nomeCategoriaRif = tema.COD_TEMA,
                //                nomeCorso = corsi.COD_CORSO,
                //                //tematicaCorso = tpcorso.COD_TIPOCORSO.Substring(7, 2000),
                //                tematica = tpcorso.COD_TIPOCORSO,
                //                url = ""
                //            };


                corsoRet = fromMainCat(query.FirstOrDefault());
                if (corsoRet != null)
                {
                    corsoRet.tags = new List<Tag>();
                    corsoRet.tipoOfferta = getTipoOffertaStr(corsoRet.idTipoOfferta);
                    corsoRet.tipoOffertaEnum = getTipoOfferta(corsoRet.idTipoOfferta);
                    corsoRet.tematica = RimuoviCodice(corsoRet.tematica);
                    corsoRet.nomeCategoriaRif = RimuoviCodice(corsoRet.nomeCategoriaRif);

                    return new GetCorsoResponse()
                    {
                        success = true,
                        error = null,
                        corso = corsoRet,
                        url = Request.RequestUri.AbsoluteUri.ToLower().
                                   Split(new string[] { "api" }, StringSplitOptions.None)[0] +
                                    "corso"

                    };
                }
                else
                {
                    return new GetCorsoResponse()
                    {
                        success = false,
                        error = "Corso inesistente",
                        corso = null,
                        url = ""

                    };
                }
            }
            catch (Exception ex)
            {
                return new GetCorsoResponse()
                {
                    success = false,
                    error = ex.Message,
                    corso = null
                };
            }
        }
        private Corso fromMainCat(MainCatalog c)
        {
            Corso corsoRet = new Corso();
            if (c != null)
            {
                if (c.dataInizioCorso != null)
                    corsoRet.data = "dd-MM-yyyy";
                corsoRet.idCategoria = c.idCategoria;
                corsoRet.idCorso = c.idCorso;
                corsoRet.metodoDidattico = c.metodoDidattico;
                corsoRet.nomeCorso = c.nomeCorso;
                corsoRet.tags = new List<Tag>();
                corsoRet.tematica = RimuoviCodice(c.tematicaCorso);
                corsoRet.tipoOfferta = getTipoOffertaStr(c.indObbligatorio);
                corsoRet.tipoOffertaEnum = getTipoOfferta(c.indObbligatorio);
                corsoRet.url = c.urlCorso;
                corsoRet.url_foto = c.urlFotoCorso;
                corsoRet.nomeCategoriaRif = RimuoviCodice(c.nomeCategoria);
                corsoRet.idTipoOfferta = c.indObbligatorio;
            }
            else
                corsoRet = null;
            return corsoRet;
        }
        private TipoOfferta getTipoOfferta(string indObbligatorio)
        {
            TipoOfferta offerta_tipo;
            switch (indObbligatorio)
            {
                case "1":
                    offerta_tipo = TipoOfferta.suRichiesta;
                    break;
                case "2":
                    offerta_tipo = TipoOfferta.aperta;
                    break;
                case "3":
                    offerta_tipo = TipoOfferta.obbligatoria;
                    break;
                default:
                    offerta_tipo = TipoOfferta.tutte;
                    break;
            }
            return offerta_tipo;
        }
        private string getTipoOffertaStr(string indObbligatorio)
        {
            string offerta_tipo;
            switch (indObbligatorio)
            {
                case "1":
                    offerta_tipo = "Su Richiesta";
                    break;
                case "2":
                    offerta_tipo = "Aperto a tutti";
                    break;
                case "3":
                    offerta_tipo = "Obbligatoria";
                    break;
                default:
                    offerta_tipo = "-";
                    break;
            }
            return offerta_tipo;
        }

        /// <summary>
        /// torna la lista dei corsi per un determinato utente, con il relativo stato di avanzamento 
        /// </summary>
        /// <param name="matricola">matricola utente</param>
        /// <returns></returns>
        public GetListaStatiCorsiResponse getStatoCorsi(string matricola, StatoCorsiEnum? stato, TipoOfferta? condizione)
        {
            var db = new TalentiaEntities();
            try
            {
                if (stato == null)
                    return new GetListaStatiCorsiResponse { success = false, error = "Specificare uno stato corretto (concluso|in_attesa|in_corso|tutti)", listaStatiCorsi = null };

                List<StatoCorso> listaCorsiStato = new List<StatoCorso>();
                if (condizione == null)
                    condizione = TipoOfferta.tutte;

                listaCorsiStato = getStatiCorsi(matricola, condizione.Value);

                if (stato == StatoCorsiEnum.tutti || stato == StatoCorsiEnum.concluso)
                {
                    #region corsi CV
                    Categoria cat = null;

                    //La condizione dei corsi RAI CV è aperta di default
                    if (condizione == TipoOfferta.tutte || condizione == TipoOfferta.aperta)
                    {
                        cat = getCorsiRAICV(matricola);
                        if (cat != null)
                        {
                            if (condizione != null)
                            {
                                if (condizione != TipoOfferta.tutte)
                                    cat.corsi = cat.corsi.FindAll(f => f.tipoOffertaEnum == condizione);
                            }
                            foreach (var item in cat.corsi)
                            {
                                listaCorsiStato.Add(new StatoCorso
                                {
                                    titolo = item.nomeCorso,
                                    avanzamento = 100,
                                    data = item.data_completamento,
                                    dateEnd = item.data_end.Value
                                });
                            }
                        }
                    }

                    //Si è deciso di non visualizzare i corsi extra RAI
                    ////La condizione dei corsi extra rai è su richiesta di default
                    //if (condizione == TipoOfferta.tutte || condizione == TipoOfferta.suRichiesta)
                    //{
                    //    cat = null;
                    //    cat = getCorsiExtraRaiCV(matricola);
                    //    if (cat != null)
                    //    {
                    //        if (condizione != null)
                    //        {
                    //            if (condizione != TipoOfferta.tutte)
                    //                cat.corsi = cat.corsi.FindAll(f => f.tipoOffertaEnum == condizione);
                    //        }
                    //        foreach (var item in cat.corsi)
                    //        {
                    //            listaCorsiStato.Add(new StatoCorso
                    //            {
                    //                titolo = item.nomeCorso,
                    //                avanzamento = 100,
                    //                data = item.data,
                    //                dateEnd = item.data_end.Value
                    //            });
                    //        }
                    //    }
                    //}
                    #endregion
                }

                string url_corsi = "";
                if (listaCorsiStato.Count > 0)
                {
                    if (stato != null)
                    {
                        if (stato.Value != StatoCorsiEnum.tutti)
                        {
                            switch (stato.Value)
                            {
                                case StatoCorsiEnum.in_attesa:
                                    listaCorsiStato = listaCorsiStato.FindAll(f => f.avanzamento == 0);
                                    url_corsi = baseUrl + "/raiacademy/ElencoInAttesa?showAll=True";
                                    break;
                                case StatoCorsiEnum.concluso:
                                    listaCorsiStato = listaCorsiStato.FindAll(f => f.avanzamento == 100);
                                    url_corsi = baseUrl + "/raiacademy/ElencoCorsiFatti?showAll=True";
                                    break;
                                case StatoCorsiEnum.in_corso:
                                    listaCorsiStato = listaCorsiStato.FindAll(f => f.avanzamento > 0 && f.avanzamento < 100);
                                    url_corsi = baseUrl + "/raiacademy/elencocorsiiniziati?showAll=True";
                                    break;
                            }
                        }
                    }

                    if (listaCorsiStato.Count == 0)
                        return new GetListaStatiCorsiResponse() { success = false, error = "Non sono presenti corsi per la matricola", listaStatiCorsi = null };

                    listaCorsiStato = listaCorsiStato.OrderByDescending(x => x.dateEnd).ToList();

                    return new GetListaStatiCorsiResponse()
                    {
                        success = true,
                        error = null,
                        listaStatiCorsi = listaCorsiStato,
                        url_corsi = url_corsi
                    };
                }
                else
                    return new GetListaStatiCorsiResponse() { success = false, error = "Non sono presenti corsi per la matricola", listaStatiCorsi = null };

            }
            catch (Exception ex)
            {
                return new GetListaStatiCorsiResponse()
                {
                    success = false,
                    error = ex.Message,
                    listaStatiCorsi = null
                };
            }
        }

        public GetSommaStatoCorsiResponse getSommaStatoCorsi(string matricola, StatoCorsiEnum stato)
        {
            var db = new TalentiaEntities();
            try
            {
                var sintesi = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == matricola);
                if (sintesi != null)
                {
                    int sumCorsi = 0;
                    if (stato == StatoCorsiEnum.concluso || stato == StatoCorsiEnum.tutti)
                    {
                        cv_ModelEntities cvEnt = new cv_ModelEntities();
                        //sumCorsi += cvEnt.V_CVCorsiRai.Count(x => x.matricola == matricola);
                        sumCorsi += RaiAcademyManager.GetCorsiFatti(matricola).Count();
                        //Si è deciso di non mostrare i corsi extra RAI
                        //sumCorsi += cvEnt.TCVFormExRai.Count(x => x.Matricola == matricola);
                    }

                    switch (stato)
                    {
                        case StatoCorsiEnum.in_attesa:
                            sumCorsi += db.CORSO.Count(x => x.IND_ATTIVO == "Y" && x.IND_TUTORSHIP == "Y" && x.EDIZIONE.Any(y => y.CURRFORM.Any(z => z.ID_PERSONA == sintesi.ID_PERSONA && z.QTA_COMPLETION == 0)));
                            break;
                        case StatoCorsiEnum.in_corso:
                            sumCorsi += db.CORSO.Count(x => x.IND_ATTIVO == "Y" && x.IND_TUTORSHIP == "Y" && x.EDIZIONE.Any(y => y.CURRFORM.Any(z => z.ID_PERSONA == sintesi.ID_PERSONA && z.QTA_COMPLETION > 0 && z.QTA_COMPLETION < 100)));
                            break;
                        case StatoCorsiEnum.concluso:
                            sumCorsi += db.CORSO.Count(x => x.IND_ATTIVO == "Y" && x.IND_TUTORSHIP == "Y" && x.EDIZIONE.Any(y => y.CURRFORM.Any(z => z.ID_PERSONA == sintesi.ID_PERSONA && z.IND_PARTICIPANT=="Y" && z.IND_STATOEVENTO=="E" && z.QTA_COMPLETION == 100)));
                            break;
                        case StatoCorsiEnum.tutti:
                            sumCorsi += db.CORSO.Count(x => x.IND_ATTIVO == "Y" && x.IND_TUTORSHIP == "Y" && x.EDIZIONE.Any(y => y.CURRFORM.Any(z => z.ID_PERSONA == sintesi.ID_PERSONA)));
                            break;
                        default:
                            break;
                    }

                    if (sumCorsi > 0)
                    {
                        return new GetSommaStatoCorsiResponse()
                        {
                            success = true,
                            error = null,
                            SUM = new SommaCorsiStato { nCorsi = sumCorsi }
                        };
                    }
                    else
                    {
                        return new GetSommaStatoCorsiResponse() { success = false, error = "Non sono presenti corsi per la matricola", SUM = null };
                    }
                }
                else
                {
                    return new GetSommaStatoCorsiResponse() { success = false, error = "Impossibile reperire corsi per la matricola selezionata", SUM = null };
                }

            }
            catch (Exception ex)
            {
                return new GetSommaStatoCorsiResponse()
                {
                    success = false,
                    error = ex.Message,
                    SUM = null
                };
            }
        }
        private List<StatoCorso> getStatiCorsi(string matricola, TipoOfferta tipoOff)
        {

            List<StatoCorso> listaCorsiStato = new List<StatoCorso>();
            try
            {
                List<CorsoUtente> lsCorsi = new List<CorsoUtente>();
                lsCorsi = getListaCorsi(matricola);
                if (tipoOff != TipoOfferta.tutte)
                    lsCorsi = lsCorsi.FindAll(f => f.condizioneEnum == tipoOff);

                foreach (CorsoUtente item in lsCorsi)
                {
                    listaCorsiStato.Add(new StatoCorso
                    {
                        titolo = item.nomeCorso,
                        avanzamento = (int)item.qtaCompletamento,
                        data = item.dataFineFREQ.HasValue? item.dataFineFREQ.Value.ToString("dd-MM-yyyy") : item.dataFineCorso.ToString("dd-MM-yyyy"),
                        dateEnd = item.dataFineFREQ.HasValue ? item.dataFineFREQ.Value: item.dataFineCorso
                    });
                }
            }
            catch (Exception)
            {
                return null;
            }
            return listaCorsiStato;
        }
        private List<CorsoUtente> getListaCorsi(string matricola)
        {
            TalentiaEntities db = new TalentiaEntities();
            List<CorsoUtente> lsCorsi = new List<CorsoUtente>();

            try
            {
                var query = from corsi in db.CORSO
                            join tema in db.TB_TEMA on corsi.ID_TEMA equals tema.ID_TEMA
                            join tpcorso in db.TB_TPCORSO on corsi.ID_TPCORSO equals tpcorso.ID_TPCORSO
                            join metodo in db.TB_MTDDID on corsi.ID_MTDDID equals metodo.ID_MTDDID
                            join edition in db.EDIZIONE on corsi.ID_CORSO equals edition.ID_CORSO
                            join iscrizioni in db.CURRFORM on edition.ID_EDIZIONE equals iscrizioni.ID_EDIZIONE
                            join anag in db.V_XR_XANAGRA on iscrizioni.ID_PERSONA equals anag.ID_PERSONA
                            where
                            corsi.IND_ATTIVO == "Y" & corsi.IND_TUTORSHIP == "Y"
                            & anag.MATRICOLA == matricola
                            select new MainCatalog
                            {
                                idCorso = corsi.ID_CORSO,
                                urlFotoCorso = "",
                                indObbligatorio = corsi.IND_OBBLIGATORIO,
                                //nomeCategoria = tema.COD_TEMA.Substring(7, 2000),
                                nomeCategoria = tema.COD_TEMA,
                                urlCategoria = "",
                                idCategoria = tema.ID_TEMA,
                                metodoDidattico = metodo.COD_METODODID,
                                dataInizioCorso = edition.DTA_INIZIO,
                                iconaTag = "",
                                labelTag = "",
                                nomeCorso = corsi.COD_CORSO,
                                // tematicaCorso = tpcorso.COD_TIPOCORSO.Substring(7, 2000),
                                tematicaCorso = tpcorso.COD_TIPOCORSO,
                                urlCorso = "",
                                pubblicato = corsi.IND_TUTORSHIP,
                                idIscrizione = iscrizioni.ID_CURRFORM,
                                idPersona = anag.ID_PERSONA,
                                matricola = anag.MATRICOLA,
                                dataFineCorso = edition.DTA_FINE,
                                dataInizioFREQ = iscrizioni.DTA_STARTEDON,
                                dataFineFREQ = iscrizioni.DTA_COMPLETEDON,
                                durataTotale = corsi.QTA_DURATA,
                                qtaCompletamento = iscrizioni.QTA_COMPLETION
                            };




                foreach (var item in query)
                {
                    lsCorsi.Add(new CorsoUtente
                    {
                        idCorso = item.idCorso,
                        matricola = item.matricola,
                        nomeCorso = item.nomeCorso,
                        durataTotale = item.durataTotale,
                        dataInizioCorso = item.dataInizioCorso,
                        dataFineCorso = item.dataFineCorso,
                        dataInizioFREQ = item.dataInizioFREQ,
                        dataFineFREQ = item.dataFineFREQ,
                        condizione = getTipoOffertaStr(item.indObbligatorio),
                        qtaCompletamento = item.qtaCompletamento
                    });
                }


            }
            catch (Exception)
            {
                return null;
            }

            return lsCorsi;
        }

        public GetTematicheResponse getAllTematiche()
        {
            var db = new TalentiaEntities();
            try
            {
                List<AreaFormativa> lscategorie = new List<AreaFormativa>();
                var query = from aree in db.TB_TEMA
                            join corsi in db.CORSO on aree.ID_TEMA equals corsi.ID_TEMA
                            join tematiche in db.TB_TPCORSO on corsi.ID_TPCORSO equals tematiche.ID_TPCORSO
                            where corsi.IND_ATTIVO == "Y" && corsi.IND_TUTORSHIP == "Y"
                            select new MainCatalogAree
                            {
                                idArea = aree.ID_TEMA,
                                nomeArea = aree.COD_TEMA,
                                idTematica = tematiche.ID_TPCORSO,
                                codTematica = tematiche.COD_TIPOCORSO
                            };
                AreaFormativa area;
                foreach (var item in query.GroupBy(x => x.idArea))
                {
                    area = new AreaFormativa();
                    area.idArea = item.Key;
                    area.nomeArea = item.ElementAt(0).nomeArea;
                    area.listaTematiche = new List<Tematica>();
                    foreach (var tipoCorso in item)
                    {

                        if (area.listaTematiche.Find(f => f.idTematica == tipoCorso.idTematica) == null)
                        {
                            area.listaTematiche.Add(new Tematica
                            {
                                idTematica = tipoCorso.idTematica,
                                codTematica = tipoCorso.codTematica,
                                url = baseUrl + "/raiacademy?area=" + item.Key.ToString() + "&tema=" + tipoCorso.idTematica
                            });
                        }
                    }
                    lscategorie.Add(area);
                }

                if (lscategorie.Count > 0)
                {
                    lscategorie = lscategorie.OrderBy(f => f.nomeArea).ToList();


                    return new GetTematicheResponse
                    {
                        success = true,
                        error = null,
                        categorie = lscategorie
                    };
                }
                else
                    return new GetTematicheResponse { success = false, error = "Nessuna tematica presente", categorie = null };
            }
            catch (Exception ex)
            {
                return new GetTematicheResponse { success = false, error = ex.Message, categorie = null };
            }
        }

        /// <summary>
        /// torna lo stato del CV dell'utente
        /// </summary>
        /// <param name="matricola">matricola dell'utente </param>
        /// <returns></returns>
        public GetStatoCVResponse getStatoCV(string matricola)
        {
            try
            {
                int perc = CommonHelper.GetPercentualCV(matricola);

                return new GetStatoCVResponse()
                {
                    success = true,
                    error = null,
                    stato = new StatoCV
                    {
                        avanzamento = perc,
                        completato = perc == 100,
                        url_cv = baseUrl + "/cv_online"
                    }
                };
            }
            catch (Exception ex)
            {
                return new GetStatoCVResponse()
                {
                    success = false,
                    error = ex.Message,
                    stato = null
                };
            }
        }
        #region aggiorna corsi

        [HttpPost]
        public AggiornaCorsoResponse SalvaStatoCorsi(Corsi corsi)
        {
            TalentiaEntities db = new TalentiaEntities();
            try
            {
                var query = from edizioni in db.EDIZIONE
                            join corsiDB in db.CORSO on edizioni.ID_CORSO equals corsiDB.ID_CORSO
                            join iscrizioni in db.CURRFORM on edizioni.ID_EDIZIONE equals iscrizioni.ID_EDIZIONE
                            join anagrafica in db.V_XR_XANAGRA on iscrizioni.ID_PERSONA equals anagrafica.ID_PERSONA
                            where
                               corsiDB.IND_ATTIVO == "Y" & corsiDB.IND_TUTORSHIP == "Y"
                            select new MainCatalog
                            {
                               idCorso = corsiDB.ID_CORSO,
                               idIscrizione = iscrizioni.ID_CURRFORM,
                               matricola = anagrafica.MATRICOLA,
                               qtaCompletamento = iscrizioni.QTA_COMPLETION,
                               idEdizione=edizioni.ID_EDIZIONE,
                               codCorsoLms = corsiDB.COD_CORSOLMS
                            };
                bool tosave = false;
                //
                string[] matrs;

                if (corsi.corsi.Count > 0)
                {
                    foreach (var giornataAttuale in corsi.corsi)
                    {
                        if (giornataAttuale.info_corsi_conclusi.Count > 0)
                        {
                            DateTime dataCompletamento = new DateTime();
                            if (!DateTime.TryParseExact(giornataAttuale.data_acquisizione, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out dataCompletamento))
                            {
                                return new AggiornaCorsoResponse
                                {
                                    stato = 1,
                                    descrizione = "Impossibile gestire la data "+giornataAttuale.data_acquisizione
                                };
                            }

                            foreach (var itm in giornataAttuale.info_corsi_conclusi)
                            {
                                matrs = itm.utenti.Split(',');
                                for (int i = 0; i < matrs.Length; i++)
                                {
                                    string t = matrs[i];
                                    string id_corso = itm.id_corso.ToString();
                                    var currentCorso = query.Where(x => x.matricola == t && x.codCorsoLms == id_corso).FirstOrDefault();

                                    if (currentCorso != null)
                                    {
                                        var dbCorso = db.CURRFORM.Where(x => x.ID_CURRFORM == currentCorso.idIscrizione).FirstOrDefault();
                                        dbCorso.QTA_COMPLETION = 100;
                                        dbCorso.DTA_COMPLETEDON = dataCompletamento;
                                        tosave = true;
                                    }
                                    else
                                    {
                                        return new AggiornaCorsoResponse
                                        {
                                            stato = 1,
                                            descrizione = "Il corso " + itm.id_corso +" non esiste"
                                        };
                                    }
                                }
                            }
                        }
                    }
                    if (tosave)
                        db.SaveChanges();
                }
                //foreach (string mat in matricole) {
                //    var currentCorso = query.Where(x => x.matricola == mat && x.idCorso==idCorso).FirstOrDefault();
                //    if (currentCorso != null) {
                //        var dbCorso = db.CURRFORM.Where(x => x.ID_CURRFORM == currentCorso.idIscrizione).FirstOrDefault();
                //        switch (stato) { 
                //            case StatoCorsiEnum.concluso:
                //                dbCorso.QTA_COMPLETION = 100;
                //                dbCorso.DTA_COMPLETEDON = DateTime.Now;
                //                tosave = true;
                //                break;
                //            case StatoCorsiEnum.in_attesa:
                //                dbCorso.QTA_COMPLETION = 0;
                //                dbCorso.DTA_COMPLETEDON = null;
                //                tosave = true;
                //                break;
                //        } 
                //    }
                //}

                //if (tosave) {
                //    db.SaveChanges();
                //}


                return new AggiornaCorsoResponse
                {
                    stato = 0,
                    descrizione = "dati acquisiti correttamente"
                };
            }
            catch (Exception ex)
            {
                return new AggiornaCorsoResponse
                {
                    stato = 1,
                    descrizione = ex.Message
                };
            }
        }


        #endregion
        private string RimuoviCodice(string codiceCompleto)
        {
            string result = codiceCompleto;

            if (result[5] == '-')
                result = result.Substring(7);

            return result;
        }
    }
    public class Corsi
    {
        public List<CorsiInput> corsi { get; set; }
    }
    public class CorsoConcluso
    {
        public int id_corso { get; set; }
        public string utenti { get; set; }
    }
    public class CorsiInput
    {
        public string data_acquisizione { get; set; }
        public int numero_corsi_conclusi { get; set; }
        public List<CorsoConcluso> info_corsi_conclusi { get; set; }

        public CorsiInput()
        {
            info_corsi_conclusi = new List<CorsoConcluso>();
        }
        /*
         
         "corsi": [

        {

          "data_acquisizione": "giorno uno",      //data ricerca

          "numero_corsi_conclusi": "10",          //numero corsi conclusi recuperati in quella data

          "info_corsi_conclusi": [

            {

              "id_corso": "192",                                                  //id corso concluso

              "utenti": "mat1, mat2"                                        //matricole utenti che hanno concluso il corso separate da virgola

            },

            {

              "id_corso": "200",                                                  //id corso concluso

              "utenti": "mat1, mat2"                                        //matricole utenti che hanno concluso il corso separate da virgola

            }

          ]

        }*/
    }
    #region classi usate per le response
    public class Categoria
    {
        public int idCategoria { get; set; }
        public string nomeCategoria { get; set; }
        public string urlCategoria { get; set; }
        public List<Tematica> tematiche { get; set; }
        public List<Corso> corsi { get; set; }
    }
    public class Corso
    {
        public int idCorso { get; set; }
        public string nomeCorso { get; set; }
        public string url { get; set; }
        public string tematica { get; set; }
        public string data { get; set; }
        public string url_foto { get; set; }
        public string metodoDidattico { get; set; }
        public string tipoOfferta { get; set; }
        public TipoOfferta tipoOffertaEnum { get; set; }
        public List<Tag> tags { get; set; }
        public int idCategoria { get; set; }
        public string nomeCategoriaRif { get; set; }
        public string idTipoOfferta { get; set; }
        public string data_completamento { get; set; }
        public string label_immagine { get; set; }
        public string label_immagine_colore { get; set; }
        public string label_immagine_icona { get; set; }

        [JsonIgnore]
        public DateTime? data_end { get; set; }
    }
    public class Tag
    {
        public string icona { get; set; }
        public string label { get; set; }
    }
    #region stato corsi / cv
    public class StatoCorso
    {
        public string titolo { get; set; }
        public int avanzamento { get; set; }
        public string data { get; set; }

        [JsonIgnore]
        public DateTime dateEnd { get; set; }
    }
    public class SommaCorsiStato
    {
        public int nCorsi { get; set; }
    }
    public class StatoCV
    {
        public int avanzamento { get; set; }
        public Boolean completato { get; set; }
        public string url_cv { get; set; }
    }
    #endregion
    #region tematiche
    public class AreaFormativa
    {
        public int idArea { get; set; }
        public string nomeArea { get; set; }
        public List<Tematica> listaTematiche { get; set; }
        public AreaFormativa()
        {
            listaTematiche = new List<Tematica>();
        }
    }
    public class Tematica
    {
        public int idTematica { get; set; }
        public string codTematica { get; set; }
        public string url { get; set; }
    }
    #endregion
    #endregion

    public class MainCatalog
    {
        public int idCorso { get; set; }
        public string nomeCorso { get; set; }
        public string urlCorso { get; set; }
        public string tematicaCorso { get; set; }
        public DateTime dataInizioCorso { get; set; }
        public string urlFotoCorso { get; set; }
        public string metodoDidattico { get; set; }
        public string indObbligatorio { get; set; } // tipo offerta
        public string iconaTag { get; set; }
        public string labelTag { get; set; }
        public int idCategoria { get; set; }
        public string nomeCategoria { get; set; }
        public string urlCategoria { get; set; }
        public string pubblicato { get; set; }
        public int idPersona { get; set; }
        public string matricola { get; set; }
        public int idIscrizione { get; set; }
        public int idEdizione { get; set; }
        public DateTime dataFineCorso { get; set; }
        public DateTime? dataInizioFREQ { get; set; }
        public DateTime? dataFineFREQ { get; set; }
        public decimal durataTotale { get; set; }
        public decimal qtaCompletamento { get; set; }
        public string codCorsoLms { get; set; }

    }
    public class CatalogoCorsi
    {
        public string urlCatalogoCorsi { get; set; }
        public List<Categoria> categorie { get; set; }
        public CatalogoCorsi()
        {
            categorie = new List<Categoria>();
        }
    }
    public class CorsoUtente
    {
        public int idCorso { get; set; }
        public string nomeCorso { get; set; }
        public string matricola { get; set; }
        public DateTime dataInizioCorso { get; set; }
        public DateTime dataFineCorso { get; set; }
        public DateTime? dataInizioFREQ { get; set; }
        public DateTime? dataFineFREQ { get; set; }
        public decimal durataTotale { get; set; }
        public decimal qtaCompletamento { get; set; }
        
        public TipoOfferta condizioneEnum { get; set; }
        public string condizione { get; set; }
    }
    public class MainCatalogAree
    {
        public int idArea { get; set; }
        public string nomeArea { get; set; }
        public int idTematica { get; set; }
        public string codTematica { get; set; }
    }
    #region responses
    /// <summary>
    /// Contiene l'esito della chiamata e la classe Catalogo contenente i dati
    /// </summary>
    public class GetCatalogoResponse
    {
        public Boolean success { get; set; }
        public string error { get; set; }
        public string urlCatalogoCorsi { get; set; }
        public List<Categoria> Categorie { get; set; }
        public GetCatalogoResponse()
        {
            Categorie = new List<Categoria>();
        }

    }
    /// <summary>
    /// Contiene l'esito della chiamata e la Categoria contenente la lista dei corsi e dettaglio categoria
    /// </summary>
    public class GetCategoriaResponse
    {
        public Boolean success { get; set; }
        public string error { get; set; }
        public Categoria categoria { get; set; }
    }
    /// <summary>
    /// Contiene il dettaglio del Corso e l'esito della chiamata
    /// </summary>
    public class GetCorsoResponse
    {
        public Boolean success { get; set; }
        public string error { get; set; }
        public Corso corso { get; set; }
        public string url { get; set; }
    }
    public enum TipoOfferta
    {
        obbligatoria,
        suRichiesta,
        aperta,
        tutte
    }
    public enum StatoCorsiEnum
    {
        in_attesa,
        in_corso,
        concluso,
        tutti
    }
    #endregion
    #region response stato corsi / CV
    /// <summary>
    /// Contiene lo stato del corso e l'esito della chiamata
    /// </summary>
    public class GetStatoCorsoResponse
    {
        public Boolean success { get; set; }
        public string error { get; set; }
        public StatoCorso stato { get; set; }

    }
    /// <summary>
    /// Contiene l'esito della chiamata e la lista dei corsi ed il loro stato
    /// </summary>
    public class GetListaStatiCorsiResponse
    {
        public GetListaStatiCorsiResponse()
        {
            url_corsi = "";
        }

        public Boolean success { get; set; }
        public string error { get; set; }
        public List<StatoCorso> listaStatiCorsi { get; set; }
        public string url_corsi { get; set; }
    }
    public class GetSommaStatoCorsiResponse
    {
        public Boolean success { get; set; }
        public string error { get; set; }
        public SommaCorsiStato SUM { get; set; }
    }
    /// <summary>
    /// Contiene l'esito della chiamata e lo stato del CV
    /// </summary>
    public class GetStatoCVResponse
    {
        public Boolean success { get; set; }
        public string error { get; set; }
        public StatoCV stato { get; set; }
    }
    #endregion


    #region response salvataggio stato corsi
    public class AggiornaCorsoResponse
    {
        public int stato { get; set; }
        public string descrizione { get; set; }
    }
    #endregion
    #region response tematiche
    public class GetTematicheResponse
    {
        public Boolean success { get; set; }
        public string error { get; set; }
        public List<AreaFormativa> categorie { get; set; }
        public GetTematicheResponse()
        {
            categorie = new List<AreaFormativa>();
        }
    }
    #endregion
}
