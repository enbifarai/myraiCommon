using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using myRaiCommonModel;
using myRaiData.Incentivi;
using Newtonsoft.Json;

namespace myRaiGestionale.Helpers
{
    public class DematerializzazioneHelper
    {

        #region PrivateMetods
        public DematerializzazioneDocumentiVM OrdinamentoListaDocumentiManager(DematerializzazioneDocumentiVM model)
        {
            try
            {
                if (model.Documenti != null && model.Documenti.Any())
                {
                    model.Documenti = OrdinamentoListaDocumenti(model.Documenti);
                }
                if (model.DocumentiDaVisionare != null && model.DocumentiDaVisionare.Any())
                {
                    model.DocumentiDaVisionare = OrdinamentoListaDocumenti(model.DocumentiDaVisionare);
                }
                if (model.DocumentiInCaricoAMe != null && model.DocumentiInCaricoAMe.Any())
                {
                    model.DocumentiInCaricoAMe = OrdinamentoListaDocumenti(model.DocumentiInCaricoAMe);
                }
                if (model.DocumentiInCaricoAdAltri != null && model.DocumentiInCaricoAdAltri.Any())
                {
                    model.DocumentiInCaricoAdAltri = OrdinamentoListaDocumenti(model.DocumentiInCaricoAdAltri);
                }
                if (model.DocumentiDaPrendereInCarico != null && model.DocumentiDaPrendereInCarico.Any())
                {
                    model.DocumentiDaPrendereInCarico = OrdinamentoListaDocumenti(model.DocumentiDaPrendereInCarico);
                }

                return model;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<XR_DEM_DOCUMENTI_EXT> OrdinamentoListaDocumenti(List<XR_DEM_DOCUMENTI_EXT> lstDoc)
        {
            try
            {
                List<SortDocument> listaDoc = new List<SortDocument>();

                //Divido i documenti in due liste, chi ha l'attributo "DataScadenzaPratica" e chi non ce l'ha
                foreach (var item in lstDoc)
                {
                    List<AttributiAggiuntivi> objD = null;
                    objD = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(item.CustomDataJSON);

                    bool trovato = false;
                    foreach (var attr in objD)
                    {
                        if (attr.DBRefAttribute != null && attr.DBRefAttribute == "DataScadenzaPratica")
                        {
                            SortDocument sortDoc = new SortDocument();
                            try
                            {
                                sortDoc.data = DateTime.Parse(attr.Valore);
                                sortDoc.doc = item;         
                            }
                            catch (Exception ex)
                            {
                                sortDoc.data = DateTime.MinValue;
                                sortDoc.doc = item;
                            }
                            
                            listaDoc.Add(sortDoc);
                            trovato = true;
                        }
                    }
                    if (!trovato)
                    {
                        SortDocument sortDoc = new SortDocument
                        {
                            data = DateTime.Now.AddDays(365),
                            doc = item
                        };
                        listaDoc.Add(sortDoc);
                    }
                }

                listaDoc = listaDoc.OrderBy(k => k.data).ToList();

                return listaDoc.Select(x => x.doc).ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        
        #endregion

        #region Class
        public class SortDocument
        {
            public DateTime data { get; set; }
            public XR_DEM_DOCUMENTI_EXT doc { get; set; }
        }

        #endregion





    }
}